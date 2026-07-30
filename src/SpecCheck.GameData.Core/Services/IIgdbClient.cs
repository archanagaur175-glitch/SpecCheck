using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public class IgdbSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}

public interface IIgdbClient
{
    Task<List<IgdbGameResponse>> SearchGamesAsync(string query, int limit = 10);
    Task<List<IgdbGameResponse>> GetGamesByIdsAsync(IEnumerable<int> ids);
    Task<List<IgdbGameResponse>> FetchRecentGamesAsync(int offset = 0, int limit = 50);
}
