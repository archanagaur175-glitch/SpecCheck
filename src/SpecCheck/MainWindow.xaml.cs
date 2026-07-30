using Microsoft.UI.Xaml;
using SpecCheck.GameData.Core.Services;
using SpecCheck.Hardware.Services;

namespace SpecCheck;

public sealed partial class MainWindow : Window
{
    private readonly IHardwareDetectionService _hardwareService;

    public MainWindow()
    {
        InitializeComponent();
        Title = "SpecCheck";
        _hardwareService = new HardwareDetectionService();
        _ = LoadHardwareAsync();
    }

    private async Task LoadHardwareAsync()
    {
        var info = await _hardwareService.DetectAsync();

        CpuText.Text = $"CPU: {info.CpuName}";
        GpuText.Text = $"GPU: {info.GpuName}";
        RamText.Text = $"RAM: {info.RamGB} GB";
        OsText.Text = $"OS: {info.OsVersion}";
        SourceText.Text = $"Source: {info.Source}";
    }

    private async void OnDetectClick(object sender, RoutedEventArgs e)
    {
        DetectButton.IsEnabled = false;
        await LoadHardwareAsync();
        DetectButton.IsEnabled = true;
    }
}
