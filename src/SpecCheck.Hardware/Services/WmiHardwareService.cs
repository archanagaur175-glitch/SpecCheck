using System.Management;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.Hardware.Services;

public class WmiHardwareService
{
    public async Task<HardwareInfo> DetectAsync()
    {
        var cpuName = await QueryFirstAsync("SELECT Name FROM Win32_Processor", "Name") ?? "Unknown CPU";
        var gpuName = await QueryFirstAsync("SELECT Name FROM Win32_VideoController", "Name") ?? "Unknown GPU";

        var ramBytes = await QueryFirstAsync("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory");
        var ramGB = ramBytes is not null && long.TryParse(ramBytes, out var bytes) ? (int)(bytes / (1024L * 1024L * 1024L)) : 16;

        var os = await QueryFirstAsync("SELECT Caption FROM Win32_OperatingSystem", "Caption") ?? Environment.OSVersion.ToString();

        return new HardwareInfo
        {
            CpuName = cpuName,
            GpuName = gpuName,
            RamGB = ramGB,
            TotalStorageBytes = null,
            OsVersion = os,
            Source = DetectionSource.Wmi,
            HasSensors = false
        };
    }

    private static async Task<string?> QueryFirstAsync(string query, string property)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(query);
                foreach (var obj in searcher.Get())
                {
                    return obj[property]?.ToString();
                }
            }
            catch
            {
            }
            return null;
        });
    }
}
