using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Data;

public class GameRepository : IGameRepository
{
    private readonly SpecCheckDbContext _context;

    public GameRepository(SpecCheckDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(int id)
    {
        return await _context.Games
            .Include(g => g.Requirements)
            .Include(g => g.FpsEstimates)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<List<Game>> SearchAsync(string query, int maxResults = 50)
    {
        return await _context.Games
            .Where(g => EF.Functions.Like(g.Title, $"%{query}%"))
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<List<Game>> GetByGenreAsync(string genre)
    {
        return await _context.Games
            .Where(g => g.Genre == genre)
            .ToListAsync();
    }

    public async Task<FPSEstimate?> GetFpsEstimateAsync(int gameId, int hardwareProfileId, string quality, string resolution)
    {
        return await _context.FpsEstimates
            .FirstOrDefaultAsync(f =>
                f.GameId == gameId &&
                f.HardwareProfileId == hardwareProfileId &&
                f.QualitySetting == quality &&
                f.Resolution == resolution);
    }

    public async Task<List<FPSEstimate>> GetFpsEstimatesForGameAsync(int gameId)
    {
        return await _context.FpsEstimates
            .Include(f => f.HardwareProfile)
            .Where(f => f.GameId == gameId)
            .ToListAsync();
    }

    public async Task<HardwareProfile?> FindClosestHardwareProfileAsync(string cpuName, string gpuName, int ramGB)
    {
        return await _context.HardwareProfiles
            .Where(h => h.RamGB >= ramGB - 2 && h.RamGB <= ramGB + 2)
            .OrderBy(h => Math.Abs(h.RamGB - ramGB))
            .FirstOrDefaultAsync();
    }

    public async Task<List<GameRequirement>> GetRequirementsAsync(int gameId)
    {
        return await _context.GameRequirements
            .Where(r => r.GameId == gameId)
            .ToListAsync();
    }

    public async Task AddOrUpdateGameAsync(Game game)
    {
        var existing = await _context.Games.FindAsync(game.Id);
        if (existing is not null)
        {
            _context.Entry(existing).CurrentValues.SetValues(game);
        }
        else
        {
            await _context.Games.AddAsync(game);
        }
    }

    public async Task AddFpsEstimatesAsync(IEnumerable<FPSEstimate> estimates)
    {
        await _context.FpsEstimates.AddRangeAsync(estimates);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
