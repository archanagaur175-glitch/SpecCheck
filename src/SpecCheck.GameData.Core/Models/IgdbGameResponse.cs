using System.Text.Json.Serialization;

namespace SpecCheck.GameData.Core.Models;

public class IgdbGameResponse
{
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("cover")]
    public IgdbCover? Cover { get; set; }

    [JsonPropertyName("genres")]
    public List<IgdbGenre>? Genres { get; set; }

    [JsonPropertyName("release_dates")]
    public List<IgdbReleaseDate>? ReleaseDates { get; set; }

    [JsonPropertyName("websites")]
    public List<IgdbWebsite>? Websites { get; set; }
}

public class IgdbCover
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}

public class IgdbGenre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class IgdbReleaseDate
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("y")]
    public int? Year { get; set; }
}

public class IgdbWebsite
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public int Category { get; set; }
}
