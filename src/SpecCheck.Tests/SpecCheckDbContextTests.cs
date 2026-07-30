using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Tests;

public class SpecCheckDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SpecCheckDbContext _context;

    public SpecCheckDbContextTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SpecCheckDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SpecCheckDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Fact]
    public async Task CanInsertAndRetrieveGame()
    {
        var game = new Game
        {
            Id = 1,
            Title = "Elden Ring",
            Genre = "Action RPG",
            ReleaseYear = 2022,
            IgdbRating = 95.0
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Games.FindAsync(1);

        Assert.NotNull(retrieved);
        Assert.Equal("Elden Ring", retrieved.Title);
    }

    [Fact]
    public async Task GameWithRequirements_SavesRelationships()
    {
        var game = new Game { Id = 2, Title = "Starfield" };
        game.Requirements.Add(new GameRequirement
        {
            RequirementType = "Minimum",
            CpuName = "AMD Ryzen 5 2600",
            GpuName = "AMD Radeon 5700",
            RamGB = 16,
            StorageGB = 125
        });

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Games
            .Include(g => g.Requirements)
            .FirstOrDefaultAsync(g => g.Id == 2);

        Assert.NotNull(retrieved);
        Assert.Single(retrieved.Requirements);
        Assert.Equal("AMD Ryzen 5 2600", retrieved.Requirements.First().CpuName);
    }

    [Fact]
    public async Task FPSEstimate_UniqueConstraint_Enforced()
    {
        var game = new Game { Id = 3, Title = "Game" };
        var profile = new HardwareProfile
        {
            Id = 1,
            CpuName = "CPU",
            GpuName = "GPU",
            RamGB = 16
        };

        _context.Games.Add(game);
        _context.HardwareProfiles.Add(profile);
        await _context.SaveChangesAsync();

        _context.FpsEstimates.Add(new FPSEstimate
        {
            GameId = 3,
            HardwareProfileId = 1,
            QualitySetting = "High",
            Resolution = "1080p",
            EstimatedFps = 60
        });
        await _context.SaveChangesAsync();

        _context.FpsEstimates.Add(new FPSEstimate
        {
            GameId = 3,
            HardwareProfileId = 1,
            QualitySetting = "High",
            Resolution = "1080p",
            EstimatedFps = 90
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
    }

    [Fact]
    public async Task HardwareProfile_CanStoreAndRetrieve()
    {
        var profile = new HardwareProfile
        {
            CpuName = "Intel Core i9-13900K",
            GpuName = "NVIDIA GeForce RTX 4090",
            RamGB = 64,
            Category = "Ultra"
        };

        _context.HardwareProfiles.Add(profile);
        await _context.SaveChangesAsync();

        var retrieved = await _context.HardwareProfiles.FirstOrDefaultAsync();

        Assert.NotNull(retrieved);
        Assert.Equal("Intel Core i9-13900K", retrieved.CpuName);
        Assert.Equal("NVIDIA GeForce RTX 4090", retrieved.GpuName);
        Assert.Equal(64, retrieved.RamGB);
        Assert.Equal("Ultra", retrieved.Category);
    }

    [Fact]
    public async Task DetectedHardware_StoresTimestamp()
    {
        var ts = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);

        var hw = new DetectedHardware
        {
            DetectedAt = ts,
            CpuName = "CPU",
            GpuName = "GPU",
            RamGB = 32,
            OsVersion = "Windows 11"
        };

        _context.DetectedHardware.Add(hw);
        await _context.SaveChangesAsync();

        var retrieved = await _context.DetectedHardware.FirstOrDefaultAsync();
        Assert.NotNull(retrieved);
        Assert.Equal(ts, retrieved.DetectedAt);
    }
}
