using LibreHardwareMonitor.Hardware;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Hardware.Services;

public class LibreHardwareMonitorService
{
    public HardwareInfo? Detect()
    {
        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = false
        };

        computer.Open();
        computer.Accept(new UpdateVisitor());

        try
        {
            string? cpuName = null;
            string? gpuName = null;
            int ramGB = 0;

            foreach (var hardware in computer.Hardware)
            {
                switch (hardware.HardwareType)
                {
                    case HardwareType.Cpu:
                        cpuName = hardware.Name;
                        break;
                    case HardwareType.GpuNvidia:
                    case HardwareType.GpuAmd:
                    case HardwareType.GpuIntel:
                        gpuName ??= hardware.Name;
                        break;
                    case HardwareType.Memory:
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Data && sensor.Name.Contains("Memory", StringComparison.OrdinalIgnoreCase))
                            {
                                ramGB = (int)(sensor.Value.GetValueOrDefault() / 1024.0);
                            }
                        }
                        break;
                }
            }

            if (cpuName is null && gpuName is null)
            {
                return null;
            }

            return new HardwareInfo
            {
                CpuName = cpuName ?? "Unknown CPU",
                GpuName = gpuName ?? "Unknown GPU",
                RamGB = ramGB > 0 ? ramGB : 16,
                TotalStorageBytes = null,
                OsVersion = Environment.OSVersion.ToString(),
                Source = DetectionSource.LibreHardwareMonitor,
                HasSensors = true
            };
        }
        finally
        {
            computer.Close();
        }
    }

    private sealed class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer) => computer.Traverse(this);
        public void VisitHardware(IHardware hardware) { hardware.Update(); foreach (var sub in hardware.SubHardware) sub.Accept(this); }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
