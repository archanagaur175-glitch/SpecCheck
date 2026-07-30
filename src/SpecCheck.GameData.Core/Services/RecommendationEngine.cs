using Microsoft.EntityFrameworkCore;
using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public class RecommendationEngine : IRecommendationEngine
{
    private readonly SpecCheckDbContext _context;
    private readonly IHardwareDetectionService _hardware;

    private static readonly Dictionary<string, double> GpuTier = new(StringComparer.OrdinalIgnoreCase)
    {
        ["RTX 4090"] = 100, ["RTX 4080"] = 90, ["RTX 4070"] = 80, ["RTX 4060"] = 70,
        ["RTX 3090"] = 85, ["RTX 3080"] = 78, ["RTX 3070"] = 68, ["RTX 3060"] = 58,
        ["RTX 2080"] = 65, ["RTX 2070"] = 55, ["RTX 2060"] = 48,
        ["GTX 1080"] = 55, ["GTX 1070"] = 45, ["GTX 1060"] = 38,
        ["RX 7900"] = 95, ["RX 7800"] = 85, ["RX 7700"] = 75, ["RX 7600"] = 65,
        ["RX 6900"] = 88, ["RX 6800"] = 78, ["RX 6700"] = 65, ["RX 6600"] = 55,
        ["Arc A770"] = 60, ["Arc A750"] = 52, ["Arc A580"] = 42,
    };

    private static readonly (double fps, string quality)[] QualityTiers =
    [
        (120, "Ultra"), (90, "High"), (60, "Medium"), (30, "Low")
    ];

    public RecommendationEngine(SpecCheckDbContext context, IHardwareDetectionService hardware)
    {
        _context = context;
        _hardware = hardware;
    }

    public async Task<List<GameRecommendation>> GetRecommendationsAsync(string? genreFilter = null, int count = 20)
    {
        var query = _context.Games
            .Include(g => g.FpsEstimates)
            .Include(g => g.Requirements)
            .AsQueryable();

        if (genreFilter is not null)
        {
            query = query.Where(g => g.Genre != null && g.Genre.Contains(genreFilter));
        }

        var games = await query
            .Where(g => g.IgdbRating.HasValue)
            .OrderByDescending(g => g.IgdbRating)
            .Take(count * 2)
            .ToListAsync();

        var hardware = await _hardware.DetectAsync();
        var closestProfile = await FindClosestProfileAsync(hardware);
        var gpuPower = GetGpuPower(hardware.GpuName);

        var recommendations = new List<GameRecommendation>();

        foreach (var game in games)
        {
            var estimate = closestProfile is not null
                ? game.FpsEstimates.FirstOrDefault(f => f.HardwareProfileId == closestProfile.Id)
                : null;

            var (predictedFps, qualityLabel, resolution) = estimate is not null
                ? (estimate.EstimatedFps, estimate.QualitySetting, estimate.Resolution)
                : PredictFpsFallback(gpuPower, hardware.RamGB);

            var availableFps = predictedFps;
            var ratingWeight = game.IgdbRating ?? 50;
            var fpsScore = Math.Min(predictedFps / 60.0 * 50, 50);
            var overallScore = (ratingWeight * 0.6) + fpsScore;

            recommendations.Add(new GameRecommendation
            {
                Game = game,
                OverallScore = Math.Round(overallScore, 1),
                PredictedFps = availableFps,
                QualityLabel = qualityLabel,
                Resolution = resolution
            });
        }

        return recommendations
            .OrderByDescending(r => r.OverallScore)
            .Take(count)
            .ToList();
    }

    public async Task<GameRecommendation?> GetRecommendationForGameAsync(int gameId)
    {
        var game = await _context.Games
            .Include(g => g.FpsEstimates)
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game is null) return null;

        var hardware = await _hardware.DetectAsync();
        var closestProfile = await FindClosestProfileAsync(hardware);
        var gpuPower = GetGpuPower(hardware.GpuName);

        var estimate = closestProfile is not null
            ? game.FpsEstimates.FirstOrDefault(f => f.HardwareProfileId == closestProfile.Id)
            : null;

        var (predictedFps, qualityLabel, resolution) = estimate is not null
            ? (estimate.EstimatedFps, estimate.QualitySetting, estimate.Resolution)
            : PredictFpsFallback(gpuPower, hardware.RamGB);

        var ratingWeight = game.IgdbRating ?? 50;
        var fpsScore = Math.Min(predictedFps / 60.0 * 50, 50);
        var overallScore = Math.Round((ratingWeight * 0.6) + fpsScore, 1);

        return new GameRecommendation
        {
            Game = game,
            OverallScore = overallScore,
            PredictedFps = predictedFps,
            QualityLabel = qualityLabel,
            Resolution = resolution
        };
    }

    private async Task<HardwareProfile?> FindClosestProfileAsync(HardwareInfo hardware)
    {
        var profiles = await _context.HardwareProfiles.ToListAsync();

        return profiles
            .OrderBy(p =>
            {
                var gpuMatch = p.GpuName.Contains(hardware.GpuName, StringComparison.OrdinalIgnoreCase) ||
                               hardware.GpuName.Contains(p.GpuName, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                var ramDiff = Math.Abs(p.RamGB - hardware.RamGB);
                return (gpuMatch * 100) + ramDiff;
            })
            .FirstOrDefault();
    }

    private static double GetGpuPower(string gpuName)
    {
        foreach (var (key, power) in GpuTier)
        {
            if (gpuName.Contains(key, StringComparison.OrdinalIgnoreCase))
                return power;
        }

        if (gpuName.Contains("RTX", StringComparison.OrdinalIgnoreCase) ||
            gpuName.Contains("GTX", StringComparison.OrdinalIgnoreCase))
            return 50;

        if (gpuName.Contains("RX", StringComparison.OrdinalIgnoreCase) ||
            gpuName.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
            return 50;

        if (gpuName.Contains("Intel", StringComparison.OrdinalIgnoreCase) ||
            gpuName.Contains("Arc", StringComparison.OrdinalIgnoreCase))
            return 45;

        return 40;
    }

    private static (double fps, string quality, string resolution) PredictFpsFallback(double gpuPower, int ramGB)
    {
        var baseFps = (gpuPower / 100.0) * 80;
        var ramBonus = ramGB >= 32 ? 15 : ramGB >= 16 ? 5 : -10;
        var fps = Math.Max(baseFps + ramBonus, 15);

        for (int i = 0; i < QualityTiers.Length; i++)
        {
            if (fps >= QualityTiers[i].fps)
            {
                var res = i switch
                {
                    0 => ramGB >= 32 ? "4K" : ramGB >= 16 ? "1440p" : "1080p",
                    1 => ramGB >= 16 ? "1440p" : "1080p",
                    2 => "1080p",
                    _ => "1080p"
                };
                return (fps, QualityTiers[i].quality, res);
            }
        }

        return (fps, "Low", "1080p");
    }
}
