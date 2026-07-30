namespace SpecCheck.GameData.Core.Data;

public interface IDatabaseSeeder
{
    Task EnsureCreatedAsync();
    Task SeedIfEmptyAsync();
}
