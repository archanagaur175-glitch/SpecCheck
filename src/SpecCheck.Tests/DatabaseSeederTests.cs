using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Tests;

public class DatabaseSeederTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SpecCheckDbContext _context;
    private readonly DatabaseSeeder _seeder;

    public DatabaseSeederTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SpecCheckDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new SpecCheckDbContext(options);
        _seeder = new DatabaseSeeder(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Fact]
    public async Task EnsureCreatedAsync_CreatesTables()
    {
        await _seeder.EnsureCreatedAsync();

        var canConnect = await _context.Database.CanConnectAsync();
        Assert.True(canConnect);
    }

    [Fact]
    public async Task SeedIfEmptyAsync_SeedsDefaultProfile()
    {
        await _seeder.EnsureCreatedAsync();
        await _seeder.SeedIfEmptyAsync();

        var profiles = await _context.HardwareProfiles.ToListAsync();
        Assert.NotEmpty(profiles);
        Assert.Contains(profiles, p => p.CpuName == "Intel Core i5-12400F");
    }

    [Fact]
    public async Task SeedIfEmptyAsync_SkipsWhenDataExists()
    {
        await _seeder.EnsureCreatedAsync();

        _context.Games.Add(new Game { Id = 1, Title = "Existing Game" });
        await _context.SaveChangesAsync();

        await _seeder.SeedIfEmptyAsync();

        var profiles = await _context.HardwareProfiles.ToListAsync();
        Assert.Empty(profiles);
    }
}
