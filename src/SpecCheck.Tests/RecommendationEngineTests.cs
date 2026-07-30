using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;
using SpecCheck.GameData.Core.Services;

namespace SpecCheck.Tests;

public class RecommendationEngineTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SpecCheckDbContext _context;
    private readonly RecommendationEngine _engine;
    private readonly MockHardwareService _hardware;

    public RecommendationEngineTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SpecCheckDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SpecCheckDbContext(options);
        _context.Database.EnsureCreated();

        _hardware = new MockHardwareService();
        _engine = new RecommendationEngine(_context, _hardware);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Fact]
    public async Task GetRecommendationsAsync_ReturnsScoredGames()
    {
        SeedGames();

        var results = await _engine.GetRecommendationsAsync(count: 5);

        Assert.NotEmpty(results);
        Assert.All(results, r =>
        {
            Assert.NotNull(r.Game);
            Assert.True(r.OverallScore > 0);
        });
    }

    [Fact]
    public async Task GetRecommendationsAsync_OrdersByScoreDescending()
    {
        SeedGames();

        var results = await _engine.GetRecommendationsAsync(count: 10);

        for (int i = 1; i < results.Count; i++)
        {
            Assert.True(results[i - 1].OverallScore >= results[i].OverallScore);
        }
    }

    [Fact]
    public async Task GetRecommendationsAsync_FiltersByGenre()
    {
        SeedGames();

        var results = await _engine.GetRecommendationsAsync(genreFilter: "RPG");

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal("RPG", r.Game.Genre));
    }

    [Fact]
    public async Task GetRecommendationsAsync_TopResult_HasHighestScore()
    {
        SeedGames();

        var results = await _engine.GetRecommendationsAsync(count: 3);

        Assert.True(results[0].OverallScore >= results[^1].OverallScore);
    }

    [Fact]
    public async Task GetRecommendationForGameAsync_ReturnsNullForMissing()
    {
        var result = await _engine.GetRecommendationForGameAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRecommendationForGameAsync_ReturnsValidRecommendation()
    {
        SeedGames();

        var result = await _engine.GetRecommendationForGameAsync(1);

        Assert.NotNull(result);
        Assert.Equal("Cyberpunk 2077", result.Game.Title);
        Assert.True(result.OverallScore > 0);
    }

    [Fact]
    public async Task PlayableFlag_SetCorrectly()
    {
        SeedGames();

        var results = await _engine.GetRecommendationsAsync();

        foreach (var r in results)
        {
            if (r.PredictedFps >= 30)
                Assert.True(r.IsPlayable);
            else
                Assert.False(r.IsPlayable);

            if (r.PredictedFps >= 60)
                Assert.True(r.IsSmooth);
        }
    }

    private void SeedGames()
    {
        _context.Games.AddRange(
            new Game { Id = 1, Title = "Cyberpunk 2077", Genre = "RPG", IgdbRating = 86 },
            new Game { Id = 2, Title = "Elden Ring", Genre = "Action RPG", IgdbRating = 95 },
            new Game { Id = 3, Title = "DOOM Eternal", Genre = "FPS", IgdbRating = 88 },
            new Game { Id = 4, Title = "Baldur's Gate 3", Genre = "RPG", IgdbRating = 96 },
            new Game { Id = 5, Title = "Starfield", Genre = "RPG", IgdbRating = 83 }
        );
        _context.SaveChanges();
    }

    private sealed class MockHardwareService : IHardwareDetectionService
    {
        public Task<HardwareInfo> DetectAsync()
        {
            return Task.FromResult(new HardwareInfo
            {
                CpuName = "Intel Core i7-13700K",
                GpuName = "NVIDIA GeForce RTX 4070",
                RamGB = 32,
                OsVersion = "Windows 11",
                Source = DetectionSource.Wmi,
                HasSensors = false
            });
        }
    }
}
