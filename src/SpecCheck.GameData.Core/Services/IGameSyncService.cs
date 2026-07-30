using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public interface IGameSyncService
{
    Task<int> SyncFromIgdbAsync(int count = 200);
    Task<int> SyncSearchAsync(string query);
    Task EnrichFromSteamAsync(int gameId, int steamAppId);
    Task LoadManifestAsync();
}
