using Dalamud.Configuration;
using Dalamud.Plugin;

namespace FoxyJumpscare.Core;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    // Jumpscare odds: 1 in X chance per second
    // Default: 1/10000 (matches original mod)
    // Range: 1/1000 to 1/50000
    public int JumpscareOdds { get; set; } = 10000;

    // Volume: 0.0 (mute) to 1.0 (100%)
    // Default: 0.8 (80%)
    public float Volume { get; set; } = 0.8f;

    // Enable/disable plugin functionality
    public bool Enabled { get; set; } = true;

    public void Save(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.SavePluginConfig(this);
    }
}
