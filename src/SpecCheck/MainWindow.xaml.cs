using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;
using SpecCheck.GameData.Core.Services;
using SpecCheck.Hardware.Services;

namespace SpecCheck;

public sealed partial class MainWindow : Window
{
    private readonly IHardwareDetectionService _hardwareService;
    private readonly RecommendationEngine _recommendationEngine;
    private readonly SpecCheckDbContext _dbContext;

    public MainWindow()
    {
        InitializeComponent();
        Title = "SpecCheck";

        var factory = new SpecCheckDbContextFactory();
        _dbContext = factory.CreateDbContext();
        _hardwareService = new HardwareDetectionService();
        _recommendationEngine = new RecommendationEngine(_dbContext, _hardwareService);

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
        await OnDetectCoreAsync();
    }

    private async void OnDetectClick(object sender, RoutedEventArgs e)
    {
        DetectButton.IsEnabled = false;
        await OnDetectCoreAsync();
        DetectButton.IsEnabled = true;
    }

    private async Task OnDetectCoreAsync()
    {
        var info = await _hardwareService.DetectAsync();
        UpdateHardwareDisplay(info);
    }

    private async void OnRecommendClick(object sender, RoutedEventArgs e)
    {
        RecommendButton.IsEnabled = false;

        var info = await _hardwareService.DetectAsync();
        UpdateHardwareDisplay(info);

        string? genre = GenreFilter.SelectedItem is ComboBoxItem item && item.Content.ToString() != "All"
            ? item.Content.ToString()
            : null;

        var recommendations = await _recommendationEngine.GetRecommendationsAsync(genreFilter: genre, count: 30);
        RecommendationsList.ItemsSource = recommendations;

        RecommendButton.IsEnabled = true;
    }

    private void UpdateHardwareDisplay(HardwareInfo info)
    {
        CpuText.Text = $"CPU: {info.CpuName}";
        GpuText.Text = $"GPU: {info.GpuName}";
        RamText.Text = $"RAM: {info.RamGB} GB";
        OsText.Text = $"OS: {info.OsVersion}";
        SourceText.Text = $"Source: {info.Source}";
        HardwareSummary.Text = $"{info.CpuName} · {info.GpuName}";
    }
}
