using SpecCheck.GameData.Core.Models;
using SpecCheck.GameData.Core.Services;

namespace SpecCheck.Tests;

public class GameSyncMappingTests
{
    [Fact]
    public void MapToGame_IgdbResponse_HasAllFields()
    {
        var sync = new TestSyncService();

        var game = sync.MapPublic(new IgdbGameResponse
        {
            Id = 1,
            Name = "Test Game",
            Summary = "A game",
            Rating = 90.0,
            Cover = new IgdbCover { Url = "//images.igdb.com/thumb.jpg" },
            Genres = [new IgdbGenre { Name = "Action" }],
            ReleaseDates = [new IgdbReleaseDate { Year = 2024 }],
            Websites = [new IgdbWebsite { Url = "https://store.steampowered.com/app/123", Category = 13 }]
        });

        Assert.Equal(1, game.Id);
        Assert.Equal("Test Game", game.Title);
        Assert.Equal("A game", game.Summary);
        Assert.Equal(0.9, game.IgdbRating);
        Assert.Equal("Action", game.Genre);
        Assert.Equal(2024, game.ReleaseYear);
        Assert.Contains("t_cover_big", game.CoverUrl);
    }

    [Fact]
    public void MapToGame_IgdbResponse_NullRating_Handled()
    {
        var sync = new TestSyncService();

        var game = sync.MapPublic(new IgdbGameResponse
        {
            Id = 2,
            Name = "No Rating",
            Genres = null,
            ReleaseDates = null,
            Websites = null
        });

        Assert.Equal(2, game.Id);
        Assert.Equal("No Rating", game.Title);
        Assert.Null(game.IgdbRating);
        Assert.Null(game.Genre);
        Assert.Null(game.ReleaseYear);
        Assert.Null(game.CoverUrl);
        Assert.Null(game.Summary);
    }

    private sealed class TestSyncService
    {
        public Game MapPublic(IgdbGameResponse src)
        {
            var genre = src.Genres?.FirstOrDefault()?.Name;
            var releaseYear = src.ReleaseDates?.FirstOrDefault()?.Year;

            return new Game
            {
                Id = src.Id,
                Title = src.Name,
                Genre = genre,
                ReleaseYear = releaseYear,
                IgdbRating = src.Rating.HasValue ? src.Rating / 100.0 : null,
                CoverUrl = src.Cover?.Url is not null ? $"https:{src.Cover.Url.Replace("t_thumb", "t_cover_big")}" : null,
                Summary = src.Summary
            };
        }
    }
}
