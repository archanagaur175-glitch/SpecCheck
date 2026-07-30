using System.Net.Http.Json;
using System.Text.Json;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public class IgdbClient : IIgdbClient, IDisposable
{
    private readonly HttpClient _http;
    private readonly IgdbSettings _settings;
    private string? _token;
    private DateTime _tokenExpiry;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string TokenUrl = "https://id.twitch.tv/oauth2/token";
    private const string ApiBase = "https://api.igdb.com/v4";

    public IgdbClient(IgdbSettings settings)
    {
        _settings = settings;
        _http = new HttpClient();
    }

    public async Task<List<IgdbGameResponse>> SearchGamesAsync(string query, int limit = 10)
    {
        await EnsureTokenAsync();
        var escaped = query.Replace("\"", "\\\"");
        var body = $"search \"{escaped}\"; fields name,summary,rating,cover.url,genres.name,release_dates.y,websites.url,websites.category; limit {limit};";
        return await PostAsync<List<IgdbGameResponse>>($"{ApiBase}/games", body) ?? [];
    }

    public async Task<List<IgdbGameResponse>> GetGamesByIdsAsync(IEnumerable<int> ids)
    {
        await EnsureTokenAsync();
        var idList = string.Join(",", ids);
        var body = $"where id = ({idList}); fields name,summary,rating,cover.url,genres.name,release_dates.y,websites.url,websites.category; limit {ids.Count()};";
        return await PostAsync<List<IgdbGameResponse>>($"{ApiBase}/games", body) ?? [];
    }

    public async Task<List<IgdbGameResponse>> FetchRecentGamesAsync(int offset = 0, int limit = 50)
    {
        await EnsureTokenAsync();
        var body = $"fields name,summary,rating,cover.url,genres.name,release_dates.y,websites.url,websites.category; sort first_release_date desc; limit {limit}; offset {offset};";
        return await PostAsync<List<IgdbGameResponse>>($"{ApiBase}/games", body) ?? [];
    }

    private async Task EnsureTokenAsync()
    {
        if (_token is not null && DateTime.UtcNow < _tokenExpiry)
        {
            return;
        }

        var url = $"{TokenUrl}?client_id={_settings.ClientId}&client_secret={_settings.ClientSecret}&grant_type=client_credentials";
        var response = await _http.PostAsync(url, null);
        response.EnsureSuccessStatusCode();

        var doc = await response.Content.ReadFromJsonAsync<JsonDocument>();
        _token = doc?.RootElement.GetProperty("access_token").GetString();
        var expiresIn = doc?.RootElement.GetProperty("expires_in").GetInt32() ?? 3600;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);
    }

    private async Task<T?> PostAsync<T>(string url, string body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain")
        };
        request.Headers.Add("Client-ID", _settings.ClientId);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public void Dispose()
    {
        _http.Dispose();
    }
}
