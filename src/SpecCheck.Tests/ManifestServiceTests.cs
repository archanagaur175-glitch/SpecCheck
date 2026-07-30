using SpecCheck.GameData.Core.Services;

namespace SpecCheck.Tests;

public class ManifestServiceTests
{
    [Fact]
    public async Task LoadAsync_FromEmbeddedResource_ReturnsEntries()
    {
        var service = new ManifestService();

        var entries = await service.LoadAsync();

        Assert.NotEmpty(entries);
        Assert.Contains(entries, e => e.Title == "Elden Ring");
        Assert.Contains(entries, e => e.Title == "Cyberpunk 2077");
    }

    [Fact]
    public async Task LoadAsync_AllEntriesHaveRequiredFields()
    {
        var service = new ManifestService();

        var entries = await service.LoadAsync();

        foreach (var entry in entries)
        {
            Assert.True(entry.Id > 0);
            Assert.False(string.IsNullOrWhiteSpace(entry.Title));
        }
    }
}
