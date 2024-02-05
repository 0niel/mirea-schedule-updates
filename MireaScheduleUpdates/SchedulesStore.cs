using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

namespace MireaScheduleUpdates;

public class SchedulesStore
{
    private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private readonly DirectoryInfo schedulesFolder;

    public SchedulesStore(string rootFolder)
    {
        schedulesFolder = new DirectoryInfo(rootFolder);
        if (!schedulesFolder.Exists)
        {
            schedulesFolder.Create();
        }
    }


    public async Task WriteScheduleMeta(ScheduleInfo schedule)
    {
        var scheduleFilesDir = GetFolderForSchedule(schedule);
        var scheduleFile = new FileInfo(Path.Combine(scheduleFilesDir.FullName, "meta.json"));
        await File.WriteAllTextAsync(scheduleFile.FullName, JsonSerializer.Serialize(schedule, jsonOptions));
    }

    public async Task WriteScheduleVersion(ScheduleInfo schedule, ScheduleHashVersion version)
    {
        var scheduleVersionsFile = GetScheduleVersionsFile(schedule);
        await File.WriteAllTextAsync(scheduleVersionsFile.FullName, JsonSerializer.Serialize(version, jsonOptions));
    }


    public async Task<ScheduleHashVersion?> ReadScheduleVersion(ScheduleInfo schedule)
    {
        var scheduleVersionsFile = GetScheduleVersionsFile(schedule);
        if (!scheduleVersionsFile.Exists)
        {
            return null;
        }
        return JsonSerializer.Deserialize<ScheduleHashVersion>(await File.ReadAllTextAsync(scheduleVersionsFile.FullName), jsonOptions)
            ?? throw new UnreachableException("Parsing versions file returns null");
    }
    private FileInfo GetScheduleVersionsFile(ScheduleInfo schedule)
    {
        var scheduleFilesDir = GetFolderForSchedule(schedule);
        var scheduleFile = new FileInfo(Path.Combine(scheduleFilesDir.FullName, "version.json"));
        return scheduleFile;
    }

    private DirectoryInfo GetFolderForSchedule(ScheduleInfo schedule)
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

        return scheduleFilesDir;
    }
}
