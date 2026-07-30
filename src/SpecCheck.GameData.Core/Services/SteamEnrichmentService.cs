using System.Net.Http.Json;
using System.Text.Json;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public class SteamEnrichmentService
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string StoreApi = "https://store.steampowered.com/api/appdetails";

    public SteamEnrichmentService()
    {
        _http = new HttpClient();
    }

    public async Task<SteamAppResponse?> FetchAppDataAsync(int steamAppId)
    {
        try
        {
            var url = $"{StoreApi}?appids={steamAppId}";
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
            if (doc is null) return null;

            var appKey = steamAppId.ToString();
            if (!doc.RootElement.TryGetProperty(appKey, out var appEntry))
                return null;

            var success = appEntry.GetProperty("success").GetBoolean();
            if (!success) return null;

            if (!appEntry.TryGetProperty("data", out var data))
                return null;

            return JsonSerializer.Deserialize<SteamAppResponse>(data.GetRawText(), JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
