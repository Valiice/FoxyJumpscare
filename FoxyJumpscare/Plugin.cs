using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FoxyJumpscare.Core;
using FoxyJumpscare.Systems;
using FoxyJumpscare.UI;

namespace FoxyJumpscare;

public class Plugin : IDalamudPlugin
{
    public static string Name => "Foxy Jumpscare Plugin";

    private const string _commandName = "/foxy";

    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICommandManager _commandManager;
    private readonly Configuration _configuration;
    private readonly WindowSystem _windowSystem;
    private readonly SettingsWindow _settingsWindow;
    private readonly JumpscareTimer _timer;
    private readonly JumpscareAudio _audio;
    private readonly JumpscareOverlay _overlay;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IFramework framework,
        ITextureProvider textureProvider)
    {
        _pluginInterface = pluginInterface;
        _commandManager = commandManager;

        _configuration = LoadOrCreateConfiguration();

        _audio = new JumpscareAudio(_configuration);
        _overlay = new JumpscareOverlay(_pluginInterface.UiBuilder, textureProvider);
        _timer = new JumpscareTimer(framework, _configuration);
        _timer.OnJumpscareTriggered += TriggerJumpscare;

        _windowSystem = new WindowSystem("FoxyJumpscarePlugin");
        _settingsWindow = new SettingsWindow(_configuration, _pluginInterface, TriggerJumpscare);
        _windowSystem.AddWindow(_settingsWindow);

        RegisterUiHandlers();
        RegisterCommand();
    }

    private Configuration LoadOrCreateConfiguration()
    {
        return _pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    }

    private void RegisterUiHandlers()
    {
        _pluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenConfigUi += ToggleSettingsWindow;
        _pluginInterface.UiBuilder.OpenMainUi += ToggleSettingsWindow;
    }

    private void RegisterCommand()
    {
        _commandManager.AddHandler(_commandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the Foxy Jumpscare settings window"
        });
    }

    private void TriggerJumpscare()
    {
        _overlay.TriggerJumpscare();
        _audio.PlayScream();
    }

    private void OnCommand(string command, string args)
    {
        ToggleSettingsWindow();
    }

    private void ToggleSettingsWindow()
    {
        _settingsWindow.IsOpen = !_settingsWindow.IsOpen;
    }

    public void Dispose()
    {
        UnregisterEventHandlers();
        DisposeSubsystems();
        _commandManager.RemoveHandler(_commandName);
        GC.SuppressFinalize(this);
    }

    private void UnregisterEventHandlers()
    {
        _timer.OnJumpscareTriggered -= TriggerJumpscare;
        _pluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        _pluginInterface.UiBuilder.OpenConfigUi -= ToggleSettingsWindow;
        _pluginInterface.UiBuilder.OpenMainUi -= ToggleSettingsWindow;
    }

    private void DisposeSubsystems()
    {
        _timer.Dispose();
        _audio.Dispose();
        _overlay.Dispose();
        _settingsWindow.Dispose();
    }
}
