using FoxyJumpscare.Core;
using NAudio.Wave;
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
        _waveOut = new WaveOutEvent
        {
            Volume = _configuration.Volume
        };
        _waveOut.Init(reader);

        _waveOut.PlaybackStopped += (sender, args) =>
        {
            reader.Dispose();
            memoryStream.Dispose();
        };

        _waveOut.Play();
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
