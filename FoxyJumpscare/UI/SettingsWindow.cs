using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Utility;
using FoxyJumpscare.Core;
using System.Numerics;

namespace FoxyJumpscare.UI;

public class SettingsWindow : Window, IDisposable
{
    private readonly Configuration _configuration;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Action _onTestJumpscare;
    private float _kofiButtonWidth;

    public SettingsWindow(Configuration configuration, IDalamudPluginInterface pluginInterface, Action onTestJumpscare)
        : base("Foxy Jumpscare Settings###FoxySettings")
    {
        _configuration = configuration;
        _pluginInterface = pluginInterface;
        _onTestJumpscare = onTestJumpscare;

        Size = new Vector2(400, 250);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        DrawKofiButton();
        DrawEnableCheckbox();
        DrawSeparator();
        DrawJumpscareOddsSlider();
        DrawSeparator();
        DrawVolumeSlider();
        DrawSeparator();
        DrawTestButton();
        DrawWarningText();
    }

    private void DrawKofiButton()
    {
        var startPos = ImGui.GetCursorPos();
        if (_kofiButtonWidth > 0)
        {
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - _kofiButtonWidth);
        }
        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Coffee, "Support",
            new Vector4(1.0f, 0.35f, 0.35f, 0.9f),
            new Vector4(1.0f, 0.25f, 0.25f, 1.0f),
            new Vector4(1.0f, 0.35f, 0.35f, 0.75f)))
        {
            Util.OpenLink("https://ko-fi.com/valiice");
        }
        _kofiButtonWidth = ImGui.GetItemRectSize().X;
        ImGui.SetCursorPos(startPos);
    }

    private void DrawEnableCheckbox()
    {
        var enabled = _configuration.Enabled;
        if (ImGui.Checkbox("Enable Jumpscares", ref enabled))
        {
            _configuration.Enabled = enabled;
            SaveConfiguration();
        }
    }

    private void DrawJumpscareOddsSlider()
    {
        ImGui.Text("Jumpscare Odds");
        ImGui.TextColored(new Vector4(0.6f, 0.6f, 0.6f, 1.0f), $"Current: 1 in {_configuration.JumpscareOdds} chance per second");

        var odds = _configuration.JumpscareOdds;
        var changed = ImGui.SliderInt("##JumpscareOdds", ref odds, 1000, 50000, $"1 in {odds}");

        // Right-click to reset to default
        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                odds = 10000; // Default value
                changed = true;
            }

            ImGui.SetTooltip("Drag to adjust odds\nRight-click to reset to default (1 in 10000)");
        }

        if (changed)
        {
            _configuration.JumpscareOdds = odds;
            SaveConfiguration();
        }
    }

    private void DrawVolumeSlider()
    {
        ImGui.Text("Scream Volume");
        var volumePercent = (int)(_configuration.Volume * 100f);
        var changed = ImGui.SliderInt("##Volume", ref volumePercent, 0, 100, $"{volumePercent}%%");

        // Right-click to reset to default
        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                volumePercent = 80; // Default: 80% (0.8f * 100)
                changed = true;
            }

            ImGui.SetTooltip("Drag to adjust volume\nRight-click to reset to default (80%)");
        }

        if (changed)
        {
            _configuration.Volume = volumePercent / 100f;
            SaveConfiguration();
        }
    }

    private void DrawTestButton()
    {
        if (ImGui.Button("Test Jumpscare", new Vector2(150, 30)))
        {
            _onTestJumpscare?.Invoke();
        }
    }

    private static void DrawWarningText()
    {
        ImGui.Spacing();
        ImGui.TextWrapped("Warning: The jumpscare will appear randomly during gameplay. " +
                         "Set odds higher (e.g., 1 in 50000) for less frequent scares.");
    }

    private static void DrawSeparator()
    {
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
    }

    private void SaveConfiguration()
    {
        _configuration.Save(_pluginInterface);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
