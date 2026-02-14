using FoxyJumpscare.Core;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Reflection;

namespace FoxyJumpscare.Systems;

public class JumpscareAudio : IDisposable
{
    private readonly Configuration _configuration;
    private readonly byte[] _cachedMp3Bytes;
    private readonly object _lock = new();
    private WaveOutEvent? _waveOut;
    private bool _disposed;

    public JumpscareAudio(Configuration configuration)
    {
        _configuration = configuration;
        _cachedMp3Bytes = LoadMp3Bytes();
    }

    private static byte[] LoadMp3Bytes()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("FoxyJumpscare.Resources.scream.mp3")
            ?? throw new InvalidOperationException("Embedded scream.mp3 resource not found.");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public void PlayScream()
    {
        // Stop any previous playback (quick — just signals the device)
        WaveOutEvent? previous;
        lock (_lock)
        {
            previous = _waveOut;
            _waveOut = null;
        }
        if (previous != null)
        {
            previous.Stop();
            previous.Dispose();
        }

        // Capture volume on the game thread so we don't read config off-thread
        var volume = _configuration.Volume;

        Task.Run(() => PlayScreamAsync(volume));
    }

    private void PlayScreamAsync(float volume)
    {
        var memoryStream = new MemoryStream(_cachedMp3Bytes);
        var reader = new Mp3FileReader(memoryStream);
        var waveOut = new WaveOutEvent();

        lock (_lock)
        {
            if (_disposed)
            {
                reader.Dispose();
                memoryStream.Dispose();
                waveOut.Dispose();
                return;
            }
            _waveOut = waveOut;
        }

        var volumeProvider = new VolumeSampleProvider(reader.ToSampleProvider())
        {
            Volume = volume
        };
        waveOut.Init(volumeProvider);

        // Save the real app volume AFTER Init() so we get the actual mixer value,
        // then max out the device so sample-level volume isn't capped by a low mixer.
        var previousVolume = waveOut.Volume;
        var volumeChanged = false;

        try
        {
            waveOut.Volume = 1.0f;
            volumeChanged = true;

            waveOut.PlaybackStopped += (sender, args) =>
            {
                waveOut.Volume = previousVolume;
                reader.Dispose();
                memoryStream.Dispose();
                waveOut.Dispose();

                lock (_lock)
                {
                    if (_waveOut == waveOut)
                        _waveOut = null;
                }
            };

            waveOut.Play();
        }
        catch
        {
            if (volumeChanged)
            {
                try { waveOut.Volume = previousVolume; } catch { }
            }
            reader.Dispose();
            memoryStream.Dispose();
            waveOut.Dispose();

            lock (_lock)
            {
                if (_waveOut == waveOut)
                    _waveOut = null;
            }
        }
    }

    public void Dispose()
    {
        WaveOutEvent? current;
        lock (_lock)
        {
            if (_disposed)
                return;
            _disposed = true;
            current = _waveOut;
            _waveOut = null;
        }

        if (current != null)
        {
            current.Stop();
            current.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
