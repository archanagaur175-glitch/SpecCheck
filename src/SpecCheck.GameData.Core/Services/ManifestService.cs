using System.Text.Json;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public class ManifestEntry
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Genre { get; set; }
    public int? ReleaseYear { get; set; }
    public double? IgdbRating { get; set; }
}

public class ManifestService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<List<ManifestEntry>> LoadAsync()
    {
        try
        {
            var assembly = typeof(ManifestService).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => n.EndsWith("games-manifest.json", StringComparison.OrdinalIgnoreCase));

            if (resourceName is null)
            {
                return [];
            }

            await using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null) return [];

            var entries = await JsonSerializer.DeserializeAsync<List<ManifestEntry>>(stream, JsonOptions);
            return entries ?? [];
        }
        catch
        {
            return [];
        }
    }
}
