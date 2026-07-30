using System.ComponentModel.DataAnnotations;

namespace SpecCheck.GameData.Core.Models;

public class FPSEstimate
{
    public int Id { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int HardwareProfileId { get; set; }
    public HardwareProfile HardwareProfile { get; set; } = null!;

    [MaxLength(20)]
    public required string QualitySetting { get; set; }

    [MaxLength(10)]
    public required string Resolution { get; set; }

    public double EstimatedFps { get; set; }
}
