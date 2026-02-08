using FoxyJumpscare.Core;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Reflection;

namespace FoxyJumpscare.Systems;

public class JumpscareAudio(Configuration configuration) : IDisposable
{
    private readonly Configuration _configuration = configuration;
    private WaveOutEvent? _waveOut;
    private bool _disposed;

    public void PlayScream()
    {
        try
        {
            StopAudio();

            using var resourceStream = GetEmbeddedAudioResource();
            if (resourceStream == null)
                return;

            var memoryStream = CopyToMemoryStream(resourceStream);
            var reader = CreateAudioReader(memoryStream);

            PlayAudioStream(reader, memoryStream);
        }
        catch (Exception)
        {
        }
    }

    private static Stream? GetEmbeddedAudioResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "FoxyJumpscare.Resources.scream.mp3";
        return assembly.GetManifestResourceStream(resourceName);
    }

    private static MemoryStream CopyToMemoryStream(Stream source)
    {
        var memoryStream = new MemoryStream();
        source.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private static WaveStream CreateAudioReader(MemoryStream memoryStream)
    {
        try
        {
            return new WaveFileReader(memoryStream);
        }
        catch
        {
            memoryStream.Position = 0;
            return new Mp3FileReader(memoryStream);
        }
    }

    private void PlayAudioStream(WaveStream reader, MemoryStream memoryStream)
    {
        var waveOut = new WaveOutEvent();
        _waveOut = waveOut;

        var volumeProvider = new VolumeSampleProvider(reader.ToSampleProvider())
        {
            Volume = _configuration.Volume
        };
        waveOut.Init(volumeProvider);

        // Save the real app volume AFTER Init() so we get the actual mixer value,
        // then max out the device so sample-level volume isn't capped by a low mixer.
        var previousVolume = waveOut.Volume;
        waveOut.Volume = 1.0f;

        waveOut.PlaybackStopped += (sender, args) =>
        {
            try { waveOut.Volume = previousVolume; } catch { }
            reader.Dispose();
            memoryStream.Dispose();
        };

        waveOut.Play();
    }

    private void StopAudio()
    {
        if (_waveOut != null)
        {
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopAudio();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
