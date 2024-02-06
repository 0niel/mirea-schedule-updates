using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using MireaScheduleUpdates;

var listenersFile = new FileInfo("../listeners.json");
var configuration = new ConfigurationBuilder()
    .AddJsonFile(listenersFile.FullName, optional: false)
    .AddEnvironmentVariables()
    // секреты пользователя от dotnet, id из csproj
    .AddUserSecrets("43a378d4-74ce-4f3c-80c9-10e608865a46")
    .Build();

var appConfiguration = new AppConfiguration();

ConfigurationBinder.Bind(configuration, appConfiguration);

using var updatesSender = new UpdatesSender(configuration);
using var scheduleClient = new ScheduleClient();
var scheduleStore = new SchedulesStore("../schedules");
var updatedSchedules = new List<ScheduleInfo>();



var handledSchedules = 0;
await foreach (var schedule in scheduleClient.GetAllSchedules())
{
    var scheduleDebugView = $"#{handledSchedules} {schedule.FullTitle} {schedule.ScheduleTarget} {schedule.Id}";
    Console.WriteLine($"Handle {scheduleDebugView}");
    await scheduleStore.WriteScheduleMeta(schedule);

    var savedScheduleVersions = await scheduleStore.ReadScheduleVersion(schedule);

    var actualScheduleVersion = await scheduleClient.GetScheduleVersions(schedule);

    if (IsScheduleUpdated(savedScheduleVersions, actualScheduleVersion))
    {
        updatedSchedules.Add(schedule);
    }

    var latestVersionHashRecord = actualScheduleVersion.MaxBy(v => v.HashVersion)
        ?? throw new IncorrectScheduleDataException($"no schedule version in schedule response for schedule {schedule.TargetTitle} {schedule.Id} {schedule.FullTitle}");
    await scheduleStore.WriteScheduleVersion(schedule, latestVersionHashRecord);

    handledSchedules++;
    Console.WriteLine($"Done   {scheduleDebugView}");
}

if (updatedSchedules.Count == 0)
{
    Console.WriteLine("no updates in schedule");
    return;
}

Console.WriteLine($"Found updates in schedules {updatedSchedules.Count}");
foreach (var schedule in updatedSchedules)
{
    Console.WriteLine($"Updated: {schedule.FullTitle} {schedule.Id} {schedule.ScheduleTarget}");
}

var updatesAsArray = updatedSchedules.ToArray();
foreach (var listener in appConfiguration.Listeners)
{
    Console.WriteLine($"Handle listener {listener.Title}");
    try
    {
        await updatesSender.SendUpdatesToListener(updatesAsArray, listener);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in listener {listener.Title}");
        // TODO: переписать сервис на использование logger-а с корректным выводом исключения
        Console.WriteLine(ex.Message);
        // TODO: создавать issue для ответственного за обработку
    }
}
var moscowTime = DateTime.UtcNow.AddHours(3);
if (appConfiguration.InCI)
{
    CommitAndPush($"{moscowTime:yyyy-MM-dd HH mm} {updatesAsArray.Length} updates");
}
else
{
    Console.WriteLine("Skip commit and push due to not in CI, you cat commit manually");
}

static bool IsScheduleUpdated(ScheduleHashVersion? saved, ScheduleHashVersion[] actual)
{
    if (saved is null)
    {
        Console.WriteLine("No saved version, save new version without notify");
        return false;
    }

    var hashWithSameVersion = actual.SingleOrDefault(v => v.HashVersion == saved.HashVersion);
    if (hashWithSameVersion is null)
    {
        Console.WriteLine($"Saved schedule version is {saved.HashVersion}, but actual contains {string.Join(',', actual.Select(v => v.HashVersion))}. Save new version without notify.");
        return false;
    }
    return saved.Hash != hashWithSameVersion.Hash;
}

void CommitAndPush(string commitMessage)
{
    using var repo = new Repository("../");
    Commands.Stage(repo, "*");
    // Create the committer's signature and commit
    var author = new Signature("GitHub Actions Bot", "actions@github.com", DateTime.Now);
    var committer = author;

    // Commit to the repository
    Commit commit = repo.Commit(commitMessage, author, committer);

    var remote = repo.Network.Remotes["origin"];
    var options = new PushOptions
    {
        CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
        {
            Username = $"x-access-token",
            Password = configuration.GetValue<string>("GITHUB_TOKEN"),
        }
    };
    repo.Network.Push(repo.Branches["main"], options);
}
