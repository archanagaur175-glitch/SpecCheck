using SpecCheck.GameData.Core.Data;
using SpecCheck.GameData.Core.Models;

namespace SpecCheck.GameData.Core.Services;

public class GameSyncService : IGameSyncService
{
    private readonly IIgdbClient _igdbClient;
    private readonly IGameRepository _repository;
    private readonly SteamEnrichmentService _steam;
    private readonly ManifestService _manifest;

    public GameSyncService(
        IIgdbClient igdbClient,
        IGameRepository repository,
        SteamEnrichmentService steam,
        ManifestService manifest)
    {
        _igdbClient = igdbClient;
        _repository = repository;
        _steam = steam;
        _manifest = manifest;
    }

    public async Task<int> SyncFromIgdbAsync(int count = 200)
    {
        var added = 0;
        var offset = 0;
        var batchSize = 50;

        while (added < count)
        {
            var take = Math.Min(batchSize, count - added);
            var games = await _igdbClient.FetchRecentGamesAsync(offset, take);
            if (games.Count == 0) break;

            foreach (var igdbGame in games)
            {
                await _repository.AddOrUpdateGameAsync(MapToGame(igdbGame));
            }

            await _repository.SaveChangesAsync();
            added += games.Count;
            offset += batchSize;
        }

        return added;
    }

    public async Task<int> SyncSearchAsync(string query)
    {
        var results = await _igdbClient.SearchGamesAsync(query);
        foreach (var igdbGame in results)
        {
            await _repository.AddOrUpdateGameAsync(MapToGame(igdbGame));
        }
        await _repository.SaveChangesAsync();
        return results.Count;
    }

    public async Task EnrichFromSteamAsync(int gameId, int steamAppId)
    {
        var game = await _repository.GetByIdAsync(gameId);
        if (game is null) return;

        var steamData = await _steam.FetchAppDataAsync(steamAppId);
        if (steamData is null) return;

        game.CoverUrl ??= steamData.HeaderImage;

        if (steamData.PcRequirements?.Minimum is not null)
        {
            var existing = game.Requirements.FirstOrDefault(r => r.RequirementType == "Minimum");
            if (existing is null)
            {
                game.Requirements.Add(new GameRequirement
                {
                    GameId = game.Id,
                    RequirementType = "Minimum",
                    CpuName = steamData.PcRequirements.Minimum
                });
            }
        }

        await _repository.SaveChangesAsync();
    }

    public async Task LoadManifestAsync()
    {
        var manifestGames = await _manifest.LoadAsync();
        foreach (var entry in manifestGames)
        {
            var game = new Game
            {
                Id = entry.Id,
                Title = entry.Title,
                Genre = entry.Genre,
                ReleaseYear = entry.ReleaseYear,
                IgdbRating = entry.IgdbRating
            };
            await _repository.AddOrUpdateGameAsync(game);
        }
        await _repository.SaveChangesAsync();
    }

    private static Game MapToGame(IgdbGameResponse src)
    {
        var genre = src.Genres?.FirstOrDefault()?.Name;
        var releaseYear = src.ReleaseDates?.FirstOrDefault()?.Year;
        var steamUrl = src.Websites?.FirstOrDefault(w => w.Category == 13)?.Url;

        return new Game
        {
            Id = src.Id,
            Title = src.Name,
            Genre = genre,
            ReleaseYear = releaseYear,
            IgdbRating = src.Rating.HasValue ? src.Rating / 100.0 : null,
            CoverUrl = src.Cover?.Url is not null ? $"https:{src.Cover.Url.Replace("t_thumb", "t_cover_big")}" : null,
            Summary = src.Summary
        };
    }
}
