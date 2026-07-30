using Microsoft.EntityFrameworkCore;
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
    private List<GameRecommendation> _allRecommendations = [];

    public MainWindow()
    {
        Title = "SpecCheck";
        InitializeComponent();

        var factory = new SpecCheckDbContextFactory();
        _dbContext = factory.CreateDbContext();
        _hardwareService = new HardwareDetectionService();
        _recommendationEngine = new RecommendationEngine(_dbContext, _hardwareService);

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();

        var seeder = new DatabaseSeeder(_dbContext);
        await seeder.SeedIfEmptyAsync();

        var manifest = new ManifestService();
        var entries = await manifest.LoadAsync();
        if (entries.Count > 0 && !await _dbContext.Games.AnyAsync())
        {
            foreach (var entry in entries)
            {
                _dbContext.Games.Add(new Game
                {
                    Id = entry.Id,
                    Title = entry.Title,
                    Genre = entry.Genre,
                    ReleaseYear = entry.ReleaseYear,
                    IgdbRating = entry.IgdbRating
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        await OnDetectCoreAsync();

        if (await _dbContext.Games.AnyAsync())
        {
            await RefreshRecommendationsAsync();
        }
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

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_allRecommendations.Count == 0) return;

        var query = SearchBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            RecommendationsList.ItemsSource = _allRecommendations;
        }
        else
        {
            RecommendationsList.ItemsSource = _allRecommendations
                .Where(r => r.Game.Title.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private async void OnRecommendClick(object sender, RoutedEventArgs e)
    {
        RecommendButton.IsEnabled = false;

        var info = await _hardwareService.DetectAsync();
        UpdateHardwareDisplay(info);

        await RefreshRecommendationsAsync();

        RecommendButton.IsEnabled = true;
    }

    private async Task RefreshRecommendationsAsync()
    {
        string? genre = GenreFilter.SelectedItem is ComboBoxItem item && item.Content.ToString() != "All"
            ? item.Content.ToString()
            : null;

        _allRecommendations = await _recommendationEngine.GetRecommendationsAsync(genreFilter: genre, count: 30);
        RecommendationsList.ItemsSource = _allRecommendations;
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
