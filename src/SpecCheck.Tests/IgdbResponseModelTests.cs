using System.Net;
using System.Text.Json;
using SpecCheck.GameData.Core.Models;
using SpecCheck.GameData.Core.Services;

namespace SpecCheck.Tests;

public class IgdbResponseModelTests
{
    [Fact]
    public void Deserialize_IgdbGameResponse()
    {
        var json = """
        {
            "id": 123,
            "name": "Test Game",
            "summary": "A game for testing",
            "rating": 85.0,
            "cover": { "id": 1, "url": "//images.igdb.com/cover.jpg" },
            "genres": [{ "id": 1, "name": "Action" }],
            "release_dates": [{ "id": 1, "y": 2024 }],
            "websites": [{ "id": 1, "url": "https://store.steampowered.com/app/123", "category": 13 }]
        }
        """;

        var game = JsonSerializer.Deserialize<IgdbGameResponse>(json);

        Assert.NotNull(game);
        Assert.Equal(123, game.Id);
        Assert.Equal("Test Game", game.Name);
        Assert.Equal("A game for testing", game.Summary);
        Assert.Equal(85.0, game.Rating);
        Assert.NotNull(game.Cover);
        Assert.Equal("//images.igdb.com/cover.jpg", game.Cover.Url);
        Assert.Single(game.Genres!);
        Assert.Equal("Action", game.Genres![0].Name);
        Assert.Single(game.ReleaseDates!);
        Assert.Equal(2024, game.ReleaseDates![0].Year);
        Assert.Single(game.Websites!);
        Assert.Equal(13, game.Websites[0].Category);
    }

    [Fact]
    public void Deserialize_IgdbGameResponse_WithNullables()
    {
        var json = """{"id": 456, "name": "Minimal Game"}""";

        var game = JsonSerializer.Deserialize<IgdbGameResponse>(json);

        Assert.NotNull(game);
        Assert.Equal(456, game.Id);
        Assert.Equal("Minimal Game", game.Name);
        Assert.Null(game.Summary);
        Assert.Null(game.Rating);
        Assert.Null(game.Cover);
        Assert.Null(game.Genres);
    }

    [Fact]
    public void Deserialize_SteamStoreResponse()
    {
        var json = """
        {
            "12345": {
                "success": true,
                "data": {
                    "steam_appid": 12345,
                    "name": "Steam Game",
                    "short_description": "A Steam game",
                    "header_image": "https://cdn.steamstatic.com/header.jpg",
                    "pc_requirements": {
                        "minimum": "OS: Windows 10",
                        "recommended": "OS: Windows 11"
                    }
                }
            }
        }
        """;

        using var doc = JsonDocument.Parse(json);
        var appEntry = doc.RootElement.GetProperty("12345");
        Assert.True(appEntry.GetProperty("success").GetBoolean());

        var data = JsonSerializer.Deserialize<SteamAppResponse>(appEntry.GetProperty("data").GetRawText());

        Assert.NotNull(data);
        Assert.Equal(12345, data.SteamAppId);
        Assert.Equal("Steam Game", data.Name);
        Assert.Equal("A Steam game", data.ShortDescription);
        Assert.Equal("https://cdn.steamstatic.com/header.jpg", data.HeaderImage);
        Assert.NotNull(data.PcRequirements);
        Assert.Contains("Windows 10", data.PcRequirements.Minimum);
    }

    [Fact]
    public void Deserialize_ManifestEntry()
    {
        var json = """{"id": 1, "title": "Game", "genre": "RPG", "releaseYear": 2023, "igdbRating": 90.0}""";

        var entry = JsonSerializer.Deserialize<ManifestEntry>(json);

        Assert.NotNull(entry);
        Assert.Equal(1, entry.Id);
        Assert.Equal("Game", entry.Title);
        Assert.Equal("RPG", entry.Genre);
        Assert.Equal(2023, entry.ReleaseYear);
        Assert.Equal(90.0, entry.IgdbRating);
    }
}
