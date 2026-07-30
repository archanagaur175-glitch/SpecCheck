using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Data;

public class SpecCheckDbContext : DbContext
{
    public SpecCheckDbContext(DbContextOptions<SpecCheckDbContext> options) : base(options) { }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<HardwareProfile> HardwareProfiles => Set<HardwareProfile>();
    public DbSet<FPSEstimate> FpsEstimates => Set<FPSEstimate>();
    public DbSet<GameRequirement> GameRequirements => Set<GameRequirement>();
    public DbSet<DetectedHardware> DetectedHardware => Set<DetectedHardware>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Game>(e =>
        {
            e.HasIndex(g => g.Title);
            e.HasIndex(g => g.Genre);
        });

        modelBuilder.Entity<FPSEstimate>(e =>
        {
            e.HasIndex(f => new { f.GameId, f.HardwareProfileId, f.QualitySetting, f.Resolution }).IsUnique();
            e.HasOne(f => f.Game).WithMany(g => g.FpsEstimates).HasForeignKey(f => f.GameId);
            e.HasOne(f => f.HardwareProfile).WithMany(h => h.FpsEstimates).HasForeignKey(f => f.HardwareProfileId);
        });

        modelBuilder.Entity<GameRequirement>(e =>
        {
            e.HasOne(r => r.Game).WithMany(g => g.Requirements).HasForeignKey(r => r.GameId);
        });
    }
}
