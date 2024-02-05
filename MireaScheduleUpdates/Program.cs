using LibGit2Sharp;
using MireaScheduleUpdates;


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

    if (handledSchedules > 20)
    {
        Console.WriteLine("debug stop due too long work");
        break;
    }

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

void CommitAndPush()
{
    using var repo = new Repository("../");
    Commands.Stage(repo, "*");
    // Create the committer's signature and commit
    var author = new Signature("GitHub Actions Bot", "actions@github.com", DateTime.Now);
    var committer = author;

    // Commit to the repository
    Commit commit = repo.Commit("Commit from code", author, committer);

    var remote = repo.Network.Remotes["origin"];
    var options = new PushOptions();
    repo.Network.Push(repo.Branches["main"]);
}
