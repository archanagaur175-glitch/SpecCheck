# Architecture

Static description of the SpecCheck solution. Nothing was built or executed.

## Projects

```
src/
  SpecCheck/                 WinUI 3 desktop executable (net8.0-windows)
  SpecCheck.GameData.Core/   net8.0: models, EF Core context, services
  SpecCheck.Hardware/        net8.0-windows10.0.19041.0: detection
  SpecCheck.Tests/           xUnit tests
  SpecCheck.sln
Directory.Packages.props     central package versions
global.json                  SDK 8.0.400, rollForward latestFeature
nuget.config                 nuget.org only
```

The app is unpackaged (`WindowsPackageType=None`) and self-contained for the
Windows App SDK.

## Startup

`App.OnLaunched` creates and activates a single `MainWindow`. `MainWindow`
constructs the DbContext, `HardwareDetectionService` and `RecommendationEngine`
directly; there is no DI container.

Startup order:

1. `EnsureCreatedAsync` creates the schema if the database file is new.
2. The seeder runs.
3. If the Games table is empty, the embedded manifest is loaded.
4. Hardware detection runs.
5. Recommendations refresh.

"Refresh Hardware" re-runs detection and updates the CPU/GPU/RAM/OS/source labels.
"Get Recommendations" re-detects hardware, then queries the engine with the selected
genre (or none) and a limit of 30. The search box filters the already-loaded list
client-side by title.

## Hardware detection

`HardwareDetectionService` tries LibreHardwareMonitor first and falls back to
`WmiHardwareService`, which queries `Win32_Processor`, `Win32_VideoController`,
`Win32_ComputerSystem` and `Win32_OperatingSystem`. `HardwareInfo` carries a
`DetectionSource` enum with values LibreHardwareMonitor, Wmi and Unknown.

Whether LibreHardwareMonitor needs elevation on a given machine has not been
verified.

## Recommendation engine

Two inputs matter: the GPU power score and the available RAM.

- A GPU tier table maps 25 GPU names to a 0–100 power score (RTX 4090 = 100 …
  Arc A580 = 42).
- Profile matching prefers a GPU-name substring match, then the smallest RAM
  difference.
- When no profile matches, `PredictFpsFallback(gpuPower, ramGB)` predicts FPS.
- The overall score is `0.6 × IGDB rating + min(predictedFps / 60 × 50, 50)`,
  rounded to one decimal.

## Data model

EF Core SQLite, default connection string `Data Source=speccheck.db`. The file is
created next to the working directory; the path is not configurable.

Five DbSets:

| Set | Model |
| --- | --- |
| `Games` | Title (≤500), genre (≤100), release year, IGDB rating, cover URL, summary, requirements, FPS estimates |
| `HardwareProfiles` | Named CPU/GPU/RAM tier profiles |
| `FpsEstimates` | Game + hardware profile + quality setting (≤20) + resolution (≤10) + estimated FPS |
| `GameRequirements` | Per-game requirements |
| `DetectedHardware` | Detection results |

Indexes exist on `Game.Title` and `Game.Genre`, plus a unique composite index on
`FPSEstimate(GameId, HardwareProfileId, QualitySetting, Resolution)`.

Schema is created with `EnsureCreatedAsync`; no `Migrations/` directory exists.
`DatabaseSeeder` inserts one `HardwareProfile` (i5-12400F / RTX 3060 / 16 GB /
MidRange) when the Games table is empty.

## Embedded manifest

`SpecCheck.GameData.Core/Data/games-manifest.json` is embedded as a resource and
read by `ManifestService`. It holds 10 games with id, title, genre, releaseYear and
igdbRating (for example Cyberpunk 2077 and The Witcher 3).

## IGDB and Steam sync (inert)

`IGameSyncService`, `IIgdbClient`, `IgdbClient`, `GameSyncService` and
`SteamEnrichmentService` exist in `SpecCheck.GameData.Core/Services/`. Nothing
constructs `IgdbClient`, `GameSyncService` or `SteamEnrichmentService` at HEAD, so
the subsystem is inert. `IgdbSettings.ClientId` and `ClientSecret` are `required`
with no configuration source.

## Tests and CI

`SpecCheck.Tests` holds 11 files and 44 `[Fact]`/`[Theory]` cases. CI runs two jobs
on `windows-2022`:

1. `validate` — .NET 8.0.x, `dotnet --info`, `dotnet workload install
   maui-windows`, restore, build.
2. `build` — restore with three retries, Release build with `-warnaserror`,
   `dotnet test` with TRX logging, self-contained `win-x64` publish, artifact
   upload with 7-day retention.

CI installs `maui-windows` although the projects reference no MAUI packages. There
is no `packages.lock.json`, so the NuGet cache key hashes only
`Directory.Packages.props`.

## Unknowns

- Whether build and tests pass at HEAD.
- Whether the database file is shipped in the published artifact or created on
  first run.
- Whether LibreHardwareMonitor requires elevation on target machines.
- Whether IGDB sync was wired up in earlier commits.
