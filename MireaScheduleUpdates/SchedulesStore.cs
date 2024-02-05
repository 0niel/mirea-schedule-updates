using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MireaScheduleUpdates;

internal class SchedulesStore
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

    public async Task WriteScheduleVersions(ScheduleInfo schedule, ScheduleHashVersion[] versions)
    {
        var scheduleFilesDir = GetFolderForSchedule(schedule);
        var scheduleFile = new FileInfo(Path.Combine(scheduleFilesDir.FullName, "versions.json"));
        await File.WriteAllTextAsync(scheduleFile.FullName, JsonSerializer.Serialize(versions, jsonOptions));
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
