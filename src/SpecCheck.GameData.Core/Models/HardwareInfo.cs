namespace SpecCheck.GameData.Core.Models;

public class HardwareInfo
{
    public required string CpuName { get; set; }
    public required string GpuName { get; set; }
    public int RamGB { get; set; }
    public long? TotalStorageBytes { get; set; }
    public required string OsVersion { get; set; }
    public DetectionSource Source { get; set; }
    public bool HasSensors { get; set; }
}

public enum DetectionSource
{
    LibreHardwareMonitor,
    Wmi,
    Unknown
}
