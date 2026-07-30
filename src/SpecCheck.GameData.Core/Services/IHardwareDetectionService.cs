using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public interface IHardwareDetectionService
{
    Task<HardwareInfo> DetectAsync();
}
