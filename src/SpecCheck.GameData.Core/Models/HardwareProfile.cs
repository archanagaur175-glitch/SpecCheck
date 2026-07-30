using System.ComponentModel.DataAnnotations;

namespace SpecCheck.GameData.Core.Models;

public class HardwareProfile
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string CpuName { get; set; }

    [MaxLength(200)]
    public required string GpuName { get; set; }

    public int RamGB { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    public ICollection<FPSEstimate> FpsEstimates { get; set; } = [];
}
