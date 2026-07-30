using Microsoft.EntityFrameworkCore;

namespace SpecCheck.GameData.Core.Data;

public class SpecCheckDbContextFactory
{
    private readonly string _connectionString;

    public SpecCheckDbContextFactory(string connectionString = "Data Source=speccheck.db")
    {
        _connectionString = connectionString;
    }

    public SpecCheckDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SpecCheckDbContext>()
            .UseSqlite(_connectionString)
            .Options;

        return new SpecCheckDbContext(options);
    }
}
