using SpecCheck.GameData.Core.Models;
using SpecCheck.GameData.Core.Services;
using SpecCheck.Hardware.Services;

namespace SpecCheck.Hardware.Services;

public class HardwareDetectionService : IHardwareDetectionService
{
    private readonly LibreHardwareMonitorService _libreService;
    private readonly WmiHardwareService _wmiService;

    public HardwareDetectionService()
    {
        _libreService = new LibreHardwareMonitorService();
        _wmiService = new WmiHardwareService();
    }

    public async Task<HardwareInfo> DetectAsync()
    {
        try
        {
            var info = _libreService.Detect();
            if (info is not null)
            {
                return info;
            }
        }
        catch
        {
        }

        return await _wmiService.DetectAsync();
    }
}
