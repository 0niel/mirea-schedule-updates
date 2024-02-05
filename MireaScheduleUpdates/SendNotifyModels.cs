using System.Text.Json.Serialization;

namespace MireaScheduleUpdates;

public sealed record AppConfiguration
{
    public UpdatesListener[] Listeners { get; set; } = [];
}

public enum SendMode
{
    /// <summary>
    /// Неизвестный тип режима отправки
    /// </summary>
    Unknown,
    /// <summary>
    /// Все обновления будут отправлены единым запросом
    /// </summary>
    AllUpdates,
    /// <summary>
    /// Кажджое обновление будет отправлено отдельным запросом
    /// </summary>
    OneByOne
}

public record UpdatesListener
{
    /// <summary>
    /// Уникальное имя системы получателя уведомлений
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// Описание системы в свободном формате
    /// </summary>
    public required string Description { get; set; }
    /// <summary>
    /// Никнейм ответстенного за систему на GitHub для создания issue в случае проблем
    /// </summary>
    public required string GithubOwnerUsername { get; set; }

    /// <summary>
    /// URL для отправки POST запроса с обновлениями. Может содержать конструкцию {SECRET} для подстановки на его место секрета
    /// </summary>
    public required string Endpoint { get; set; }
    /// <summary>
    /// Режим отправки уведомлений
    /// </summary>
    public required SendMode SendMode { get; set; }
    /// <summary>
    /// Имя секрета на GitHub, которое будет приложено с запросом
    /// </summary>
    public required string SecretName { get; set; }
    /// <summary>
    /// Имя заголовка для прикладывания в него секрета
    /// </summary>
    public required string? HeaderNameForSecret { get; set; }
}

public record UpdatesMessage
{
    /// <summary>
    /// Поле для игнорирования дублирующихся сообщений на стороне приемника
    /// </summary>
    [JsonPropertyName("hash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string Hash { get; set; }
    /// <summary>
    /// Обновленное расписание
    /// </summary>
    [JsonPropertyName("schedule")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required ScheduleInfo? Schedule { get; set; }
    /// <summary>
    /// Обновленные расписания
    /// </summary>
    [JsonPropertyName("schedules")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required ScheduleInfo[]? Schedules { get; set; }
}
