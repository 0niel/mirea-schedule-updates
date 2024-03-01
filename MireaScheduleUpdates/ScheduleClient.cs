using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MireaScheduleUpdates;

public sealed class ScheduleClient : IDisposable
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://schedule-of.mirea.ru"),
    };

    public async IAsyncEnumerable<ScheduleInfo> GetAllSchedules()
    {
        string? nextPageToken = null;
        do
        {
            var query = new QueryString().Add("pageToken", nextPageToken).ToUriComponent();
            var searchResponse = await _httpClient.GetFromJsonAsync<SearchResponse>("/schedule/api/search" + query)
                ?? throw new UnreachableException("search schedules returns null");
            nextPageToken = searchResponse.NextPageToken;
            foreach (var schedule in searchResponse.Data)
            {
                yield return schedule;
            }
        } while (nextPageToken is not null);
    }

    public async Task<ScheduleHashVersion[]> GetScheduleVersions(ScheduleInfo schedule)
    {
        var scheduleContentQuery = new QueryString()
            .Add("id", schedule.Id.ToString(CultureInfo.InvariantCulture))
            .Add("target", schedule.ScheduleTarget.ToString(CultureInfo.InvariantCulture))
            .ToUriComponent();

        var scheduleVersions = await _httpClient.GetFromJsonAsync<ScheduleHashVersion[]>("/schedule/api/scheduleversion" + scheduleContentQuery)
            ?? throw new UnreachableException("get schedule version returns null");
        return scheduleVersions;
    }
    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private sealed record SearchResponse
    {
        [JsonPropertyName("data")]
        public ScheduleInfo[] Data { get; set; } = Array.Empty<ScheduleInfo>();
        [JsonPropertyName("nextPageToken")]
        public string? NextPageToken { get; set; }
    }
}
