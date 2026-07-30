namespace SpecCheck.GameData.Core.Models;

public class GameRecommendation
{
    public required Game Game { get; set; }
    public double OverallScore { get; set; }
    public double? PredictedFps { get; set; }
    public string? QualityLabel { get; set; }
    public string? Resolution { get; set; }
    public bool IsPlayable => PredictedFps.HasValue && PredictedFps >= 30;
    public bool IsSmooth => PredictedFps.HasValue && PredictedFps >= 60;
    public string PlayabilityText => IsSmooth ? "✓ Smooth" : IsPlayable ? "Playable" : "Unplayable";
    public string PlayabilityColor => IsSmooth ? "#2ECC71" : IsPlayable ? "#F39C12" : "#E74C3C";
}
