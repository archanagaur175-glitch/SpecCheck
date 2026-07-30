using System.Text.Json.Serialization;

namespace SpecCheck.GameData.Core.Models;

public class SteamAppResponse
{
    [JsonPropertyName("steam_appid")]
    public int SteamAppId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    [JsonPropertyName("header_image")]
    public string? HeaderImage { get; set; }

    [JsonPropertyName("pc_requirements")]
    public SteamRequirements? PcRequirements { get; set; }

    [JsonPropertyName("minimum_requirements")]
    public SteamRequirements? MinimumRequirements { get; set; }

    [JsonPropertyName("recommended_requirements")]
    public SteamRequirements? RecommendedRequirements { get; set; }
}

public class SteamRequirements
{
    [JsonPropertyName("minimum")]
    public string? Minimum { get; set; }

    [JsonPropertyName("recommended")]
    public string? Recommended { get; set; }
}

public class SteamStoreResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public SteamAppResponse? Data { get; set; }
}
