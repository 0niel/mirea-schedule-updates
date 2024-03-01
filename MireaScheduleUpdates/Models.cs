using System.Text.Json.Serialization;

namespace MireaScheduleUpdates;

public sealed record ScheduleStoredInfo
{
    [JsonPropertyName("hash")]
    public required ScheduleInfo Schedule { get; set; }
    [JsonPropertyName("version")]
    public required ScheduleHashVersion Version { get; set; }
}
public sealed record ScheduleHashVersion
{
    [JsonPropertyName("hashVersion")]
    public required int HashVersion { get; set; }
    [JsonPropertyName("hash")]
    public required string Hash { get; set; }
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

public sealed class IncorrectScheduleDataException(string message) : Exception(message);
