using LibGit2Sharp;
using MireaScheduleUpdates;


using var scheduleClient = new ScheduleClient();
var scheduleStore = new SchedulesStore("../schedules");


var handledSchedules = 0;
await foreach (var schedule in scheduleClient.GetAllSchedules())
{
    await scheduleStore.WriteScheduleMeta(schedule);

    var scheduleVersions = await scheduleClient.GetScheduleVersions(schedule);

    await scheduleStore.WriteScheduleVersions(schedule, scheduleVersions);

    handledSchedules++;
    Console.WriteLine($"Done schedule #{handledSchedules} {schedule.FullTitle}");

    if (handledSchedules > 20)
    {
        Console.WriteLine("debug stop due too long work");
        break;
    }

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
