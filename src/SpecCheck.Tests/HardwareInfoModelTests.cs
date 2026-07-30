using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Tests;

public class HardwareInfoModelTests
{
    [Fact]
    public void HardwareInfo_Create_WithLibreHardwareMonitorSource()
    {
        var info = new HardwareInfo
        {
            CpuName = "Intel Core i7-13700K",
            GpuName = "NVIDIA GeForce RTX 4090",
            RamGB = 64,
            TotalStorageBytes = 2_000_000_000_000,
            OsVersion = "Windows 11 24H2",
            Source = DetectionSource.LibreHardwareMonitor,
            HasSensors = true
        };

        Assert.Equal("Intel Core i7-13700K", info.CpuName);
        Assert.Equal("NVIDIA GeForce RTX 4090", info.GpuName);
        Assert.Equal(64, info.RamGB);
        Assert.Equal(2_000_000_000_000, info.TotalStorageBytes);
        Assert.Equal("Windows 11 24H2", info.OsVersion);
        Assert.Equal(DetectionSource.LibreHardwareMonitor, info.Source);
        Assert.True(info.HasSensors);
    }

    [Fact]
    public void HardwareInfo_Create_WithWmiSource()
    {
        var info = new HardwareInfo
        {
            CpuName = "AMD Ryzen 5 5600X",
            GpuName = "AMD Radeon RX 6800",
            RamGB = 32,
            TotalStorageBytes = null,
            OsVersion = "Windows 10",
            Source = DetectionSource.Wmi,
            HasSensors = false
        };

        Assert.Equal("AMD Ryzen 5 5600X", info.CpuName);
        Assert.Equal(DetectionSource.Wmi, info.Source);
        Assert.False(info.HasSensors);
        Assert.Null(info.TotalStorageBytes);
    }
}
