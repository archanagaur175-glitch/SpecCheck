using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public interface IRecommendationEngine
{
    Task<List<GameRecommendation>> GetRecommendationsAsync(string? genreFilter = null, int count = 20);
    Task<GameRecommendation?> GetRecommendationForGameAsync(int gameId);
}
