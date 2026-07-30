using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpecCheck.GameData.Core.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SpecCheckDbContext>
{
    public SpecCheckDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SpecCheckDbContext>()
            .UseSqlite("Data Source=speccheck.db")
            .Options;

        return new SpecCheckDbContext(options);
    }
}
