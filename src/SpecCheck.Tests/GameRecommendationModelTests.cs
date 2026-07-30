using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Tests;

public class GameRecommendationModelTests
{
    [Fact]
    public void IsPlayable_WhenFpsAbove30_ReturnsTrue()
    {
        var rec = new GameRecommendation
        {
            Game = new Game { Id = 1, Title = "Test" },
            OverallScore = 80,
            PredictedFps = 45,
            QualityLabel = "Medium",
            Resolution = "1080p"
        };

        Assert.True(rec.IsPlayable);
        Assert.False(rec.IsSmooth);
    }

    [Fact]
    public void IsSmooth_WhenFpsAbove60_ReturnsTrue()
    {
        var rec = new GameRecommendation
        {
            Game = new Game { Id = 1, Title = "Test" },
            OverallScore = 90,
            PredictedFps = 75,
            QualityLabel = "High",
            Resolution = "1440p"
        };

        Assert.True(rec.IsPlayable);
        Assert.True(rec.IsSmooth);
    }

    [Fact]
    public void NotPlayable_WhenFpsBelow30()
    {
        var rec = new GameRecommendation
        {
            Game = new Game { Id = 1, Title = "Test" },
            OverallScore = 30,
            PredictedFps = 20,
            QualityLabel = "Low",
            Resolution = "1080p"
        };

        Assert.False(rec.IsPlayable);
        Assert.False(rec.IsSmooth);
    }

    [Fact]
    public void NullFps_DefaultsToNotPlayable()
    {
        var rec = new GameRecommendation
        {
            Game = new Game { Id = 1, Title = "Test" },
            OverallScore = 50,
            PredictedFps = null
        };

        Assert.False(rec.IsPlayable);
        Assert.False(rec.IsSmooth);
        Assert.Null(rec.QualityLabel);
        Assert.Null(rec.Resolution);
    }
}
