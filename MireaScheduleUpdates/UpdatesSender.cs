using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MireaScheduleUpdates;

public sealed class UpdatesSender(IConfiguration _configuration) : IDisposable
{
    private readonly IConfiguration _configuration = _configuration;
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public async Task SendUpdatesToListener(ScheduleInfo[] updates, UpdatesListener listener)
    {
        if (listener.SendMode == SendMode.AllUpdates)
        {
            var hash = Convert.ToBase64String(
                SHA512.HashData(
                    Encoding.UTF8.GetBytes(
                        string.Join("", updates.Select(s => $"{s.ScheduleTarget}{s.Id}")))));
            var message = new UpdatesMessage
            {
                Hash = hash,
                Schedule = null,
                Schedules = updates,
            };
            var secret = _configuration.GetValue<string>(listener.SecretName);
            var endpoint = listener.Endpoint.Replace("{SECRET}", secret);
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (!string.IsNullOrEmpty(listener.HeaderNameForSecret))
            {
                request.Headers.Add(listener.HeaderNameForSecret, secret);
            }
            request.Content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, MediaTypeNames.Application.Json);
            await _httpClient.SendAsync(request);
        }
        else
        {
            throw new UnreachableException("cry...");
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
