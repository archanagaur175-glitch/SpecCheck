using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Data;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(int id);
    Task<List<Game>> SearchAsync(string query, int maxResults = 50);
    Task<List<Game>> GetByGenreAsync(string genre);
    Task<FPSEstimate?> GetFpsEstimateAsync(int gameId, int hardwareProfileId, string quality, string resolution);
    Task<List<FPSEstimate>> GetFpsEstimatesForGameAsync(int gameId);
    Task<HardwareProfile?> FindClosestHardwareProfileAsync(string cpuName, string gpuName, int ramGB);
    Task<List<GameRequirement>> GetRequirementsAsync(int gameId);
    Task AddOrUpdateGameAsync(Game game);
    Task AddFpsEstimatesAsync(IEnumerable<FPSEstimate> estimates);
    Task<int> SaveChangesAsync();
}
