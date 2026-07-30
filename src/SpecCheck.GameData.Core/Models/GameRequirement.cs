using System.ComponentModel.DataAnnotations;

namespace SpecCheck.GameData.Core.Models;

public class GameRequirement
{
    public int Id { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    [MaxLength(20)]
    public required string RequirementType { get; set; }

    [MaxLength(200)]
    public string? CpuName { get; set; }

    [MaxLength(200)]
    public string? GpuName { get; set; }

    public int? RamGB { get; set; }

    public int? StorageGB { get; set; }

    [MaxLength(50)]
    public string? Os { get; set; }
}
