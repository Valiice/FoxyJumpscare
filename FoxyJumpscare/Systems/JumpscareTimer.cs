using Dalamud.Plugin.Services;
using FoxyJumpscare.Core;

namespace FoxyJumpscare.Systems;

public class JumpscareTimer : IDisposable
{
    private readonly IFramework _framework;
    private readonly Configuration _configuration;
    private float _timeSinceLastCheck;

    public event Action? OnJumpscareTriggered;

    public JumpscareTimer(IFramework framework, Configuration configuration)
    {
        _framework = framework;
        _configuration = configuration;

        _framework.Update += OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!_configuration.Enabled)
            return;

        _timeSinceLastCheck += (float)framework.UpdateDelta.TotalSeconds;

        if (ShouldPerformCheck())
        {
            ResetTimer();
            CheckForJumpscare();
        }
    }

    private bool ShouldPerformCheck()
    {
        return _timeSinceLastCheck >= 1.0f;
    }

    private void ResetTimer()
    {
        _timeSinceLastCheck = 0f;
    }

    private void CheckForJumpscare()
    {
        int roll = Random.Shared.Next(1, _configuration.JumpscareOdds + 1);

        if (roll == 1)
        {
            OnJumpscareTriggered?.Invoke();
        }
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
        GC.SuppressFinalize(this);
    }
}
