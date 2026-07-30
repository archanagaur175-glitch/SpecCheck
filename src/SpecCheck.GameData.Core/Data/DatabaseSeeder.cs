using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Data;

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly SpecCheckDbContext _context;

    public DatabaseSeeder(SpecCheckDbContext context)
    {
        _context = context;
    }

    public async Task EnsureCreatedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task SeedIfEmptyAsync()
    {
        if (await _context.Games.AnyAsync())
        {
            return;
        }

        var profile = new HardwareProfile
        {
            CpuName = "Intel Core i5-12400F",
            GpuName = "NVIDIA GeForce RTX 3060",
            RamGB = 16,
            Category = "MidRange"
        };

        _context.HardwareProfiles.Add(profile);
        await _context.SaveChangesAsync();
    }
}
