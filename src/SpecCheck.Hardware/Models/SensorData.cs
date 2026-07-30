namespace SpecCheck.Hardware.Models;

public class SensorData
{
    public double? CpuTemperature { get; set; }
    public double? CpuUsage { get; set; }
    public double? GpuTemperature { get; set; }
    public double? GpuUsage { get; set; }
    public double? GpuCoreClock { get; set; }
    public double? GpuMemoryClock { get; set; }
    public double? RamUsage { get; set; }
    public double? FanSpeed { get; set; }
}
