# SpecCheck

A Windows desktop app that detects the PC's hardware and recommends games the
machine can run.

SpecCheck reads CPU, GPU, RAM and OS information, scores the machine against a GPU
tier table, and ranks a local catalogue by a combined rating/FPS score. The
catalogue is a SQLite database seeded from a 10-game manifest embedded in the
binary. Everything runs locally; there is no network feature at HEAD.

## Features

- **Hardware detection** — tries LibreHardwareMonitor first and falls back to WMI
  queries (`Win32_Processor`, `Win32_VideoController`, `Win32_ComputerSystem`,
  `Win32_OperatingSystem`). The result records which source was used:
  LibreHardwareMonitor, WMI or Unknown.
- **Hardware summary card** — CPU, GPU, RAM, OS and detection source.
- **Recommendations** — a genre filter (All / RPG / FPS / Action / Strategy) and a
  30-result list.
- **Client-side search** — filters the loaded recommendation list by title.
- **Local database** — EF Core SQLite with Games, HardwareProfiles, FpsEstimates,
  GameRequirements and DetectedHardware tables.

## Requirements

- Windows 10 version 2004 (build 19041) or newer, matching the hardware project's
  target framework.
- .NET SDK 8.0.400. `global.json` pins `8.0.400` with `rollForward:
  latestFeature`.
- The Windows App SDK runtime. The app is unpackaged
  (`WindowsPackageType=None`) and self-contained for the Windows App SDK.

## Build and run

```powershell
cd src
dotnet restore
dotnet build
dotnet run --project SpecCheck
```

The solution is `src/SpecCheck.sln` and contains four projects:

| Project | Kind |
| --- | --- |
| `SpecCheck` | WinUI 3 desktop executable |
| `SpecCheck.GameData.Core` | `net8.0` models, data access and recommendation logic |
| `SpecCheck.Hardware` | `net8.0-windows10.0.19041.0` hardware detection |
| `SpecCheck.Tests` | xUnit test project |

Package versions are centralised in `Directory.Packages.props`. `nuget.config`
points at nuget.org only. `dotnet-ef` 8.0.13 is declared as a local tool in
`.config/dotnet-tools.json`.

## Tests

```powershell
cd src
dotnet test
```

`SpecCheck.Tests` contains 11 test files with 44 `[Fact]`/`[Theory]` cases covering
models, the repository, the DbContext, the seeder, the manifest, the recommendation
engine, sync mapping, IGDB responses and hardware detection.

CI (`.github/workflows/build.yml`) runs on pushes to `main`, `master` and
`develop`, on pull requests to `main`, and on manual dispatch. It has two jobs:
`validate` (restore and build on `windows-2022`) and `build` (restore with retries,
Release build with `-warnaserror`, `dotnet test`, a self-contained `win-x64`
publish and a 7-day artifact). Markdown-only changes are ignored.

No build or test run was performed for this documentation.

## Database

The default connection string is `Data Source=speccheck.db`, so the database file is
created next to the working directory. The schema is created with
`EnsureCreatedAsync`; there are no EF migrations.

The seeder inserts one `HardwareProfile` (i5-12400F / RTX 3060 / 16 GB / MidRange)
when the Games table is empty, and the embedded manifest supplies 10 games. The
manifest contains id, title, genre, release year and IGDB rating.

## How recommendations are scored

- GPU names map to a 0–100 power score (RTX 4090 = 100 down to Arc A580 = 42).
- The overall score is `0.6 × IGDB rating + min(predictedFps / 60 × 50, 50)`,
  rounded to one decimal.
- The closest hardware profile is chosen by GPU-name substring match, then by
  smallest RAM difference.
- If no profile matches, `PredictFpsFallback(gpuPower, ramGB)` supplies the FPS
  estimate.

## Configuration

None. The app reads no environment variables, ships no `appsettings.json`, and
keeps no secrets. The only path is the default `speccheck.db` in the working
directory, and it is not configurable.

## Known limitations

- The IGDB and Steam sync subsystem (`IgdbClient`, `GameSyncService`,
  `SteamEnrichmentService`) is present but nothing constructs it at HEAD, so no
  live metadata sync happens. `IgdbSettings.ClientId` and `ClientSecret` are
  required properties with no configuration source.
- The catalogue is limited to the 10 games in the embedded manifest.
- `DatabaseSeeder.SeedIfEmptyAsync` checks the Games table but seeds only a hardware
  profile, so its name overstates what it does.
- CI installs the `maui-windows` workload even though the solution references no
  MAUI packages.

## License

MIT. See [`LICENSE`](LICENSE).
