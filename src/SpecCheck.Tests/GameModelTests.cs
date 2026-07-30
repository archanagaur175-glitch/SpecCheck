using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Tests;

public class GameModelTests
{
    [Fact]
    public void Game_Create_WithRequiredFields()
    {
        var game = new Game { Title = "Cyberpunk 2077" };

        Assert.Equal("Cyberpunk 2077", game.Title);
        Assert.Null(game.Genre);
        Assert.Empty(game.Requirements);
        Assert.Empty(game.FpsEstimates);
    }

    [Fact]
    public void Game_Create_WithAllFields()
    {
        var game = new Game
        {
            Id = 12345,
            Title = "Baldur's Gate 3",
            Genre = "RPG",
            ReleaseYear = 2023,
            IgdbRating = 96.0,
            CoverUrl = "https://images.igdb.com/cover.jpg",
            Summary = "A great RPG"
        };

        Assert.Equal(12345, game.Id);
        Assert.Equal("Baldur's Gate 3", game.Title);
        Assert.Equal("RPG", game.Genre);
        Assert.Equal(2023, game.ReleaseYear);
        Assert.Equal(96.0, game.IgdbRating);
        Assert.Equal("https://images.igdb.com/cover.jpg", game.CoverUrl);
        Assert.Equal("A great RPG", game.Summary);
    }

    [Fact]
    public void HardwareProfile_Create()
    {
        var profile = new HardwareProfile
        {
            CpuName = "AMD Ryzen 5 5600X",
            GpuName = "NVIDIA GeForce RTX 4070",
            RamGB = 32,
            Category = "HighEnd"
        };

        Assert.Equal("AMD Ryzen 5 5600X", profile.CpuName);
        Assert.Equal("NVIDIA GeForce RTX 4070", profile.GpuName);
        Assert.Equal(32, profile.RamGB);
        Assert.Equal("HighEnd", profile.Category);
        Assert.Empty(profile.FpsEstimates);
    }

    [Fact]
    public void FPSEstimate_Create()
    {
        var game = new Game { Title = "Test Game" };
        var profile = new HardwareProfile { CpuName = "CPU", GpuName = "GPU", RamGB = 16 };

        var estimate = new FPSEstimate
        {
            Game = game,
            HardwareProfile = profile,
            QualitySetting = "High",
            Resolution = "1080p",
            EstimatedFps = 85.5
        };

        Assert.Equal("High", estimate.QualitySetting);
        Assert.Equal("1080p", estimate.Resolution);
        Assert.Equal(85.5, estimate.EstimatedFps);
        Assert.Same(game, estimate.Game);
        Assert.Same(profile, estimate.HardwareProfile);
    }

    [Fact]
    public void GameRequirement_Create()
    {
        var game = new Game { Title = "Test Game" };

        var req = new GameRequirement
        {
            Game = game,
            RequirementType = "Minimum",
            CpuName = "Intel Core i5-8400",
            GpuName = "NVIDIA GTX 1060",
            RamGB = 8,
            StorageGB = 60,
            Os = "Windows 10"
        };

        Assert.Equal("Minimum", req.RequirementType);
        Assert.Equal("Intel Core i5-8400", req.CpuName);
        Assert.Equal("NVIDIA GTX 1060", req.GpuName);
        Assert.Equal(8, req.RamGB);
        Assert.Equal(60, req.StorageGB);
        Assert.Equal("Windows 10", req.Os);
        Assert.Same(game, req.Game);
    }

    [Fact]
    public void DetectedHardware_Create()
    {
        var hw = new DetectedHardware
        {
            DetectedAt = new DateTime(2026, 7, 30, 12, 0, 0, DateTimeKind.Utc),
            CpuName = "Intel Core i7-13700K",
            GpuName = "NVIDIA GeForce RTX 4090",
            RamGB = 64,
            TotalStorageBytes = 2_000_000_000_000,
            OsVersion = "Windows 11 24H2"
        };

        Assert.Equal(new DateTime(2026, 7, 30, 12, 0, 0, DateTimeKind.Utc), hw.DetectedAt);
        Assert.Equal("Intel Core i7-13700K", hw.CpuName);
        Assert.Equal("NVIDIA GeForce RTX 4090", hw.GpuName);
        Assert.Equal(64, hw.RamGB);
        Assert.Equal(2_000_000_000_000, hw.TotalStorageBytes);
        Assert.Equal("Windows 11 24H2", hw.OsVersion);
    }
}
