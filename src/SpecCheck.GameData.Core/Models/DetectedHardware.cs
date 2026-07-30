using System.ComponentModel.DataAnnotations;

namespace SpecCheck.GameData.Core.Models;

public class DetectedHardware
{
    public int Id { get; set; }

    public DateTime DetectedAt { get; set; }

    [MaxLength(200)]
    public required string CpuName { get; set; }

    [MaxLength(200)]
    public required string GpuName { get; set; }

    public int RamGB { get; set; }

    public long? TotalStorageBytes { get; set; }

    [MaxLength(100)]
    public string? OsVersion { get; set; }
}
