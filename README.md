# Foxy Jumpscare Plugin for FFXIV

![Build](https://github.com/Valiice/FoxyJumpscare/actions/workflows/build.yml/badge.svg)
![Release](https://github.com/Valiice/FoxyJumpscare/actions/workflows/release.yml/badge.svg)

A Dalamud plugin that triggers a fullscreen "Withered Foxy" jumpscare at random intervals during Final Fantasy XIV gameplay, inspired by Five Nights at Freddy's mods.

## Features

- **Random Jumpscares**: Configurable odds (default 1 in 10,000 chance per second)
- **Fullscreen Visual**: Displays a scary image that covers the entire screen for 2-3 seconds
- **Scream Sound Effect**: Plays an audio jumpscare synchronized with the visual
- **Volume Control**: Adjustable volume slider (0-100%)
- **Enable/Disable Toggle**: Turn jumpscares on or off
- **Test Button**: Preview the jumpscare effect before playing
- **Customizable Odds**: Adjust frequency from 1/1000 to 1/50000

## Prerequisites

Before you can build or use this plugin, you need:

1. **XIVLauncher** - The custom launcher for FFXIV
   - Download from: https://goatcorp.github.io/

2. **Dalamud** - The plugin framework (installed with XIVLauncher)
   - Dalamud should create a development directory at:
     - Windows: `%APPDATA%\XIVLauncher\addon\Hooks\dev\`
     - Linux: `~/.xlcore/dalamud/Hooks/dev/`
     - macOS: `~/Library/Application Support/XIV on Mac/dalamud/Hooks/dev/`

3. **.NET 10.0 SDK** or later
   - Download from: https://dotnet.microsoft.com/download

## Building the Plugin

### If Dalamud is Installed:

```bash
cd FoxyJumpscare
dotnet restore FoxyJumpscare.csproj
dotnet build FoxyJumpscare.csproj
```

### If Dalamud is Not Installed:

You can set the `DALAMUD_HOME` environment variable to point to a Dalamud installation:

```bash
# Windows (PowerShell)
$env:DALAMUD_HOME = "C:\path\to\dalamud\Hooks\dev"
dotnet build

# Linux/macOS (Bash)
export DALAMUD_HOME="/path/to/dalamud/Hooks/dev"
dotnet build
```

The build will create a `.dll` file in `FoxyJumpscare/bin/Debug/FoxyJumpscare.dll`

## Installation

1. Build the plugin (see above)
2. Copy the built DLL to your Dalamud devPlugins folder:
   - Windows: `%APPDATA%\XIVLauncher\devPlugins\FoxyJumpscare\`
3. Restart the game or use `/xlplugins` to reload plugins
4. Enable the plugin in the Dalamud plugin installer

## Usage

### Opening Settings

Use the `/foxy` command in-game to open the settings window.

### Settings Options

- **Enable Jumpscares**: Toggle the plugin on/off
- **Jumpscare Odds**: Slider from 1/1000 to 1/50000 (default: 1/10000)
  - Lower number = more frequent scares
  - Higher number = less frequent scares
- **Scream Volume**: Adjust from 0% (mute) to 100% (full volume)
- **Test Jumpscare**: Click to preview the jumpscare effect

### How It Works

The plugin checks once per second if a jumpscare should trigger based on your configured odds. When triggered:
1. A fullscreen image appears for 2.5 seconds
2. A scream sound effect plays
3. The jumpscare cannot be dismissed until it completes
4. Normal gameplay resumes automatically

## Project Structure

```
FoxyJumpscare/
├── Plugin.cs                           # Main plugin class, coordinates all systems
├── GlobalUsings.cs                     # Global using directives
├── FoxyJumpscare.json                 # Plugin manifest
│
├── Core/                              # Core infrastructure
│   └── Configuration.cs               # Settings storage and persistence
│
├── Systems/                           # Game systems
│   ├── JumpscareTimer.cs             # Random trigger logic (1-second intervals)
│   ├── JumpscareAudio.cs             # NAudio-based sound playback
│   └── JumpscareOverlay.cs           # ImGui fullscreen animation rendering
│
├── UI/                                # User interface
│   └── SettingsWindow.cs              # Configuration UI
│
└── Resources/                         # Embedded assets
    ├── FNAF2OldFoxyJumpscare.gif     # Animated jumpscare (14 frames)
    └── scream.mp3                     # Scream sound effect
```

## Customization

### Replacing Assets

To use your own jumpscare animation and sound:

1. Replace `Resources/FNAF2OldFoxyJumpscare.gif` with your animated GIF
2. Replace `Resources/scream.mp3` with your audio file (MP3 or WAV format)
3. Update the resource name in `JumpscareOverlay.cs` and `JumpscareAudio.cs`
4. Rebuild the plugin

The resources are embedded into the DLL during build, so no external files are needed at runtime.

### Adjusting Animation Speed

Edit `JumpscareOverlay.cs` and change the `FrameDuration` constant (default: 0.033f for 30 FPS).

## Technical Details

- **Framework**: Dalamud.NET.Sdk 14.0.1
- **Target**: .NET 10.0-windows (x64)
- **Dependencies**:
  - NAudio 2.2.1 (audio playback)
  - SixLabors.ImageSharp 3.1.6 (GIF decoding)
- **UI**: ImGui (provided by Dalamud)
- **Random Generation**: System.Random.Shared (thread-safe)

## CI/CD & Automation

This project includes automated GitHub Actions workflows:

- **Build** (`build.yml`): Runs on every push/PR to master/main - validates that the project compiles
- **Release** (`release.yml`): Triggered by version tags (e.g., `v1.0.0`) or manual dispatch - creates GitHub releases with packaged plugin
- **Dependabot** (`dependabot.yml`): Weekly checks for NuGet package and GitHub Actions updates
- **Auto-merge** (`dependabot-auto-merge.yml`): Automatically merges patch-level dependency updates from Dependabot
- **Scheduled Bump** (`scheduled-bump.yml`): Daily check that auto-bumps patch version when Dependabot updates are merged

### Creating a Release

To create a new release:
```bash
git tag v1.0.0
git push origin v1.0.0
```

Or use the manual workflow dispatch in GitHub Actions to specify a version.

## Troubleshooting

### Build Errors: "ImGuiNET could not be found"

- Ensure Dalamud is installed via XIVLauncher
- Check that `%APPDATA%\XIVLauncher\addon\Hooks\dev\` exists
- Try setting the `DALAMUD_HOME` environment variable

### Jumpscare Doesn't Appear

- Check that the plugin is enabled in settings
- Verify `Enable Jumpscares` is checked
- Try the "Test Jumpscare" button to confirm it works
- Check the odds setting isn't too high (try 1/10 for testing)

### No Sound

- Check the volume slider isn't at 0%
- Verify your system audio is working
- Try rebuilding the plugin to ensure audio file is embedded

## License

This is a fan-made plugin for entertainment purposes. Asset sources should be copyright-free or properly licensed.

## Credits

- Inspired by FNAF jumpscare mods
- Built with Dalamud plugin framework
- Uses NAudio for audio playback
