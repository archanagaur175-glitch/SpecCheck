using SpecCheck.GameData.Core.Models;
using SpecCheck.GameData.Core.Services;

namespace SpecCheck.Tests;

public class HardwareDetectionServiceTests
{
    [Fact]
    public async Task Detect_ReturnsHardwareInfo()
    {
        var service = new MockHardwareService();

        var result = await service.DetectAsync();

        Assert.NotNull(result);
        Assert.Equal("Mock CPU", result.CpuName);
        Assert.Equal("Mock GPU", result.GpuName);
    }

    [Fact]
    public async Task Detect_CanReturnLibreSource()
    {
        var service = new MockHardwareService
        {
            Result = new HardwareInfo
            {
                CpuName = "Real CPU",
                GpuName = "Real GPU",
                RamGB = 32,
                OsVersion = "Windows 11",
                Source = DetectionSource.LibreHardwareMonitor,
                HasSensors = true
            }
        };

        var result = await service.DetectAsync();

        Assert.Equal(DetectionSource.LibreHardwareMonitor, result.Source);
        Assert.True(result.HasSensors);
        Assert.Equal(32, result.RamGB);
    }

    private sealed class MockHardwareService : IHardwareDetectionService
    {
        public HardwareInfo? Result { get; set; }

        public Task<HardwareInfo> DetectAsync()
        {
            return Task.FromResult(Result ?? new HardwareInfo
            {
                CpuName = "Mock CPU",
                GpuName = "Mock GPU",
                RamGB = 16,
                OsVersion = "Mock OS",
                Source = DetectionSource.Wmi,
                HasSensors = false
            });
        }
    }
}
