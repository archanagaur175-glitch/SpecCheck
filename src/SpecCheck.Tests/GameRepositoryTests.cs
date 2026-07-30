using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Tests;

public class GameRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SpecCheckDbContext _context;
    private readonly GameRepository _repository;

    public GameRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SpecCheckDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SpecCheckDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new GameRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Fact]
    public async Task AddOrUpdateGame_AddsNew()
    {
        var game = new Game { Id = 10, Title = "New Game", ReleaseYear = 2025 };

        await _repository.AddOrUpdateGameAsync(game);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(10);
        Assert.NotNull(retrieved);
        Assert.Equal("New Game", retrieved.Title);
    }

    [Fact]
    public async Task AddOrUpdateGame_UpdatesExisting()
    {
        var game = new Game { Id = 11, Title = "Original Title" };
        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var updated = new Game { Id = 11, Title = "Updated Title" };
        await _repository.AddOrUpdateGameAsync(updated);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(11);
        Assert.NotNull(retrieved);
        Assert.Equal("Updated Title", retrieved.Title);
    }

    [Fact]
    public async Task SearchAsync_ReturnsMatchingGames()
    {
        _context.Games.Add(new Game { Id = 20, Title = "The Witcher 3" });
        _context.Games.Add(new Game { Id = 21, Title = "Witcher Tales" });
        _context.Games.Add(new Game { Id = 22, Title = "Cyberpunk 2077" });
        await _context.SaveChangesAsync();

        var results = await _repository.SearchAsync("Witcher");

        Assert.Equal(2, results.Count);
        Assert.Contains(results, g => g.Title == "The Witcher 3");
        Assert.Contains(results, g => g.Title == "Witcher Tales");
    }

    [Fact]
    public async Task GetByGenreAsync_ReturnsFiltered()
    {
        _context.Games.Add(new Game { Id = 30, Title = "Doom", Genre = "FPS" });
        _context.Games.Add(new Game { Id = 31, Title = "Halo", Genre = "FPS" });
        _context.Games.Add(new Game { Id = 32, Title = "Civilization", Genre = "Strategy" });
        await _context.SaveChangesAsync();

        var fpsGames = await _repository.GetByGenreAsync("FPS");

        Assert.Equal(2, fpsGames.Count);
        Assert.All(fpsGames, g => Assert.Equal("FPS", g.Genre));
    }

    [Fact]
    public async Task GetFpsEstimateAsync_ReturnsCorrectEstimate()
    {
        var game = new Game { Id = 40, Title = "Test Game" };
        var profile = new HardwareProfile { Id = 1, CpuName = "CPU", GpuName = "GPU", RamGB = 16 };

        _context.Games.Add(game);
        _context.HardwareProfiles.Add(profile);
        await _context.SaveChangesAsync();

        _context.FpsEstimates.Add(new FPSEstimate
        {
            GameId = 40,
            HardwareProfileId = 1,
            QualitySetting = "Ultra",
            Resolution = "4K",
            EstimatedFps = 30
        });
        await _context.SaveChangesAsync();

        var estimate = await _repository.GetFpsEstimateAsync(40, 1, "Ultra", "4K");

        Assert.NotNull(estimate);
        Assert.Equal(30, estimate.EstimatedFps);
    }

    [Fact]
    public async Task FindClosestHardwareProfile_MatchesByRam()
    {
        _context.HardwareProfiles.Add(new HardwareProfile { Id = 2, CpuName = "CPU-A", GpuName = "GPU-A", RamGB = 8 });
        _context.HardwareProfiles.Add(new HardwareProfile { Id = 3, CpuName = "CPU-B", GpuName = "GPU-B", RamGB = 16 });
        _context.HardwareProfiles.Add(new HardwareProfile { Id = 4, CpuName = "CPU-C", GpuName = "GPU-C", RamGB = 32 });
        await _context.SaveChangesAsync();

        var match = await _repository.FindClosestHardwareProfileAsync("Any CPU", "Any GPU", 16);

        Assert.NotNull(match);
        Assert.Equal(16, match.RamGB);
    }

    [Fact]
    public async Task GetRequirementsAsync_ReturnsRequirements()
    {
        var game = new Game { Id = 50, Title = "Demanding Game" };
        game.Requirements.Add(new GameRequirement
        {
            RequirementType = "Minimum",
            CpuName = "CPU Min",
            GpuName = "GPU Min",
            RamGB = 8
        });
        game.Requirements.Add(new GameRequirement
        {
            RequirementType = "Recommended",
            CpuName = "CPU Rec",
            GpuName = "GPU Rec",
            RamGB = 16
        });

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var reqs = await _repository.GetRequirementsAsync(50);

        Assert.Equal(2, reqs.Count);
        Assert.Contains(reqs, r => r.RequirementType == "Minimum");
        Assert.Contains(reqs, r => r.RequirementType == "Recommended");
    }
}
