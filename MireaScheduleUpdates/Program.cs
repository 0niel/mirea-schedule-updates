using LibGit2Sharp;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
};
var schedulesFolder = new DirectoryInfo("schedules");

if (!schedulesFolder.Exists)
{
    schedulesFolder.Create();
}
using var scheduleClient = new HttpClient
{
    BaseAddress = new Uri("https://schedule-of.mirea.ru")
};

string? nextPageToken = null;
var handledSchedules = 0;
do
{
    var query = new QueryString().Add("pageToken", nextPageToken).ToUriComponent();
    var searchResponse = await scheduleClient.GetFromJsonAsync<SearchResponse>("/schedule/api/search" + query);
    nextPageToken = searchResponse!.NextPageToken;

    foreach (var schedule in searchResponse.Data)
    {
        var targetScheduleDir = new DirectoryInfo(Path.Combine(schedulesFolder.FullName, schedule.ScheduleTarget.ToString(CultureInfo.InvariantCulture)));
        if (!targetScheduleDir.Exists)
        {
            targetScheduleDir.Create();
        }

        var scheduleFilesDir = new DirectoryInfo(Path.Combine(targetScheduleDir.FullName, schedule.Id.ToString(CultureInfo.InvariantCulture)));
        if (!scheduleFilesDir.Exists)
        {
            scheduleFilesDir.Create();
        }
        var scheduleFile = new FileInfo(Path.Combine(scheduleFilesDir.FullName, "meta.json"));
        await File.WriteAllTextAsync(scheduleFile.FullName, JsonSerializer.Serialize(schedule, jsonOptions));
        //var scheduleContentQuery = new QueryString()
        //    .Add("id", schedule.Id.ToString(CultureInfo.InvariantCulture))
        //    .Add("type", schedule.ScheduleTarget.ToString(CultureInfo.InvariantCulture))
        //    .ToUriComponent();
        //var scheduleInfo = new ScheduleStoredInfo
        //{
        //    Info = schedule,
        //    ContentToken = await scheduleClient.GetStringAsync("/schedule/api/schedulecontent" + scheduleContentQuery)
        //};
        //var scheduleInfoAsText = JsonSerializer.Serialize(scheduleInfo, jsonOptions);
        //await File.WriteAllTextAsync(scheduleFile.FullName, scheduleInfoAsText);
        handledSchedules++;
        Console.WriteLine($"Done schedule #{handledSchedules} {schedule.FullTitle}");
    }
} while (nextPageToken is not null);

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

public sealed record ScheduleStoredInfo
{
    /// <summary>
    /// Информация о расписании, возвращенная при поиске
    /// </summary>
    public required ScheduleInfo Info { get; set; }
    /// <summary>
    /// "Контент" расписания, на основании которого должно производиться сравнение
    /// </summary>
    public required string ContentToken { get; set; }
}

public sealed record SearchResponse
{
    [JsonPropertyName("data")]
    public ScheduleInfo[] Data { get; set; } = Array.Empty<ScheduleInfo>();
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public sealed record ScheduleInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("targetTitle")]
    public string TargetTitle { get; set; } = default!;

    [JsonPropertyName("fullTitle")]
    public string FullTitle { get; set; } = default!;

    [JsonPropertyName("scheduleTarget")]
    public int ScheduleTarget { get; set; }

    [JsonPropertyName("iCalLink")]
    public string ICalLink { get; set; } = default!;

    [JsonPropertyName("scheduleImageLink")]
    public string ScheduleImageLink { get; set; } = default!;

    [JsonPropertyName("scheduleUpdateImageLink")]
    public string ScheduleUpdateImageLink { get; set; } = default!;
}

