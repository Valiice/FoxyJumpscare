# Foxy Jumpscare for FFXIV

![Build](https://github.com/Valiice/FoxyJumpscare/actions/workflows/build.yml/badge.svg)

Random FNAF Withered Foxy jumpscares while you play FFXIV. Because why not?

## What it does

- Randomly triggers a fullscreen Foxy jumpscare with sound
- Default: 1 in 10,000 chance every second (adjust in settings)
- Type `/foxy` in-game to configure

## Installation

1. Install [XIVLauncher](https://goatcorp.github.io/)
2. Copy `FoxyJumpscare.dll` to `%APPDATA%\XIVLauncher\devPlugins\FoxyJumpscare\`
3. Type `/xlplugins` in-game to reload
4. Type `/foxy` to open settings

## Building

```bash
cd FoxyJumpscare
dotnet build FoxyJumpscare.csproj
```

Output: `FoxyJumpscare/bin/Debug/FoxyJumpscare.dll`

## Settings

- **Enable/Disable** - Toggle it on/off
- **Odds** - 1/1000 to 1/50000 (lower = more scary)
- **Volume** - 0-100%
- **Test Button** - Preview before you regret it

## For Developers

Stack: Dalamud.NET.Sdk 14.0.1, .NET 10, NAudio, ImageSharp

To make a release:
```bash
git tag v1.0.0
git push origin v1.0.0
```

CI/CD handles the rest (builds, releases, dependency updates).
