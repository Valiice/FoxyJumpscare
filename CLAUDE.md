# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Foxy Jumpscare** is a Dalamud plugin for Final Fantasy XIV that triggers random FNAF-style Withered Foxy jumpscares during gameplay. Built using Dalamud.NET.Sdk 14.0.1, targeting .NET 10.0-windows.

**Note:** The project directory has been renamed to `FoxyJumpscare`, matching the namespace and output DLL name.

## Build & Development Commands

### Building the Project (Debug only - user preference)
```bash
dotnet build FoxyJumpscare/FoxyJumpscare.csproj
```

**Note:** Only build Debug version unless user specifically requests Release build.

### Restore NuGet Packages
```bash
dotnet restore FoxyJumpscare/FoxyJumpscare.csproj
```

### Clean Build Artifacts
```bash
dotnet clean FoxyJumpscare/FoxyJumpscare.csproj
```

## Project Architecture

### Framework & Dependencies
- **Dalamud.NET.Sdk 14.0.1**: Plugin framework and build tooling
- **Target Framework**: .NET 10.0-windows (x64)
- **NAudio 2.2.1**: Audio playback for scream sound effect
- **SixLabors.ImageSharp 3.1.6**: GIF decoding and frame extraction

### Project Structure (Separation of Concerns)

```
FoxyJumpscare/
├── Plugin.cs                      # Main entry point (like Program.cs)
├── GlobalUsings.cs                # Global using directives
├── FoxyJumpscare.json            # Plugin manifest for distribution
│
├── Core/                          # Core infrastructure
│   └── Configuration.cs           # Settings storage and persistence
│
├── Systems/                       # Game systems
│   ├── JumpscareTimer.cs         # Random trigger logic (1/X chance per second)
│   ├── JumpscareAudio.cs         # NAudio sound playback
│   └── JumpscareOverlay.cs       # ImGui fullscreen animation rendering
│
├── UI/                           # User interface
│   └── SettingsWindow.cs         # Configuration window
│
└── Resources/                    # Embedded assets
    ├── FNAF2OldFoxyJumpscare.gif # Animated jumpscare (decoded at runtime)
    └── scream.mp3                # Scream sound effect
```

### Key Components

**Plugin.cs**
- Implements `IDalamudPlugin`
- Registers `/foxy` command
- Coordinates all subsystems
- Handles OpenMainUi and OpenConfigUi callbacks

**JumpscareTimer.cs**
- Subscribes to `IFramework.Update`
- Checks every 1 second with configurable odds (default 1/10000)
- Triggers jumpscare event when roll succeeds

**JumpscareOverlay.cs**
- Loads GIF and decodes all frames at startup using ImageSharp
- Animates at 30 FPS (0.033s per frame)
- Fullscreen ImGui window with transparency effects
- Plays animation once then disappears
- **Important:** Applies transparent window background during jumpscare

**JumpscareAudio.cs**
- Uses NAudio (WaveOutEvent) for audio playback
- Loads embedded MP3/WAV from manifest resources
- Configurable volume (0.0 - 1.0)

**Configuration.cs**
- `JumpscareOdds`: 1/X chance (range: 1000-50000, default: 10000)
- `Volume`: Audio volume (0.0-1.0, default: 0.8)
- `Enabled`: Toggle jumpscare functionality
- Persists to JSON via Dalamud plugin config system

**SettingsWindow.cs**
- Sliders for odds and volume
- Enable/disable toggle
- "Test Jumpscare" button

### Technical Implementation Details

**GIF Animation:**
- GIF decoded at startup into individual frames
- Frames stored as `IDalamudTextureWrap` array
- 30 FPS playback (matches FNAF mod reference)
- Animation plays once and ends (no looping)

**ImGui Transparency Effect:**
- Applied ONLY during jumpscare (not before/after)
- `ImGui.PushStyleColor(ImGuiCol.WindowBg, transparent)`
- `ImGuiWindowFlags.NoBackground`
- Styles pushed/popped each frame during active jumpscare
- Automatically restores when jumpscare ends

**Random Trigger:**
- `Random.Shared.Next(1, odds + 1)` - thread-safe
- Triggers only when roll == 1
- Respects `Enabled` configuration setting

## Development Workflow

1. **Testing**: Copy `FoxyJumpscare/bin/Debug/FoxyJumpscare.dll` to `%APPDATA%\XIVLauncher\devPlugins\FoxyJumpscare\`
2. **Reload**: Use `/xlplugins` command in-game
3. **Settings**: Type `/foxy` to open configuration window
4. **Test**: Click "Test Jumpscare" button to preview

## Plugin Manifest

Two manifest sources (both required):
- **FoxyJumpscare.json**: Distribution manifest with RepoUrl, IconUrl, etc.
- **.csproj PropertyGroup**: Build-time manifest (required by DalamudPackager)

## Important Notes

- **Build preference**: Debug builds only (unless user requests Release)
- **ImGui namespace**: Use `Dalamud.Bindings.ImGui` with global using alias
- **Texture API**: `ITextureProvider.CreateFromRaw()` with `RawImageSpecification.Rgba32()`
- **Thread safety**: All Dalamud callbacks run on game thread
- **Validation warnings fixed**: OpenMainUi registered, Tags added to manifest
- **No Release builds**: User prefers Debug for testing

## Asset Sources

- **Foxy GIF**: `FNAF2OldFoxyJumpscare.gif` (1.3MB, 14 frames)
- **Scream Sound**: `scream.mp3` (27KB, actual FNAF sound)
- Both embedded as resources in the DLL

## Performance Considerations

- GIF decoded once at startup (not per-trigger)
- All frames pre-loaded as textures
- 30 FPS animation (0.033s frame duration)
- No pixel manipulation (removed grey background code that broke image)
- Minimal overhead when jumpscare inactive

## CI/CD & GitHub Actions

### Automated Workflows

**.github/workflows/build.yml**
- Triggers: Push/PR to master or main branches
- Builds project in Release mode on Ubuntu
- Caches Dalamud SDK for faster builds
- Uploads build artifacts

**.github/workflows/release.yml**
- Triggers: Tag push (`v*.*.*`) or manual workflow dispatch
- Builds project with version stamping
- Creates GitHub release with packaged plugin ZIP
- Example: `git tag v1.0.0 && git push origin v1.0.0`

**.github/workflows/dependabot-auto-merge.yml**
- Auto-merges patch-level dependency updates from Dependabot
- Requires PR checks to pass before merging
- Only patches (no minor/major versions)

**.github/workflows/scheduled-bump.yml**
- Runs daily at 4 AM UTC
- Checks for Dependabot code dependency merges since last tag
- Auto-bumps patch version and creates new tag
- Ignores GitHub Actions updates and manual commits
- Requires PAT_TOKEN secret for pushing tags

**.github/dependabot.yml**
- Weekly checks for NuGet package updates
- Weekly checks for GitHub Actions version updates

### Release Process

**Automatic (recommended):**
1. Dependabot creates PR for dependency update
2. CI build validates the update
3. Auto-merge merges patch updates
4. Daily scheduler detects merge and creates new tag
5. Release workflow builds and publishes

**Manual:**
```bash
git tag v1.0.0
git push origin v1.0.0
```

Or use GitHub Actions "Run workflow" button on release.yml
