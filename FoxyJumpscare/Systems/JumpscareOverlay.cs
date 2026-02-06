using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;

namespace FoxyJumpscare.Systems;

public class JumpscareOverlay : IDisposable
{
    private readonly IUiBuilder _uiBuilder;
    private readonly ITextureProvider _textureProvider;
    private readonly List<IDalamudTextureWrap> _foxyFrames = [];
    private bool _isJumpscareActive = false;
    private DateTime _lastFrameTime;
    private int _currentFrame = 0;
    private const float _frameDuration = 0.033f; // 30 FPS (33ms per frame)
    private bool _animationComplete = false;
    private bool _framesLoaded = false;

    public JumpscareOverlay(IUiBuilder uiBuilder, ITextureProvider textureProvider)
    {
        _uiBuilder = uiBuilder;
        _textureProvider = textureProvider;
        _uiBuilder.Draw += DrawJumpscare;

        LoadGifFrames();
    }

    private void LoadGifFrames()
    {
        try
        {
            using var resourceStream = GetEmbeddedGifResource();
            if (resourceStream == null)
                return;

            using var image = Image.Load<Rgba32>(resourceStream);
            ConvertGifFramesToTextures(image);

            _framesLoaded = _foxyFrames.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load GIF: {ex.Message}");
        }
    }

    private static Stream? GetEmbeddedGifResource()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceName = "FoxyJumpscare.Resources.FNAF2FoxyJumpscare.gif";
        return assembly.GetManifestResourceStream(resourceName);
    }

    private void ConvertGifFramesToTextures(Image<Rgba32> image)
    {
        for (int i = 0; i < image.Frames.Count; i++)
        {
            var texture = ConvertFrameToTexture(image.Frames.CloneFrame(i));
            if (texture != null)
            {
                _foxyFrames.Add(texture);
            }
        }
    }

    private IDalamudTextureWrap? ConvertFrameToTexture(Image<Rgba32> frame)
    {
        var pixels = new byte[frame.Width * frame.Height * 4];
        frame.CopyPixelDataTo(pixels);

        var texture = _textureProvider.CreateFromRaw(
            Dalamud.Interface.Textures.RawImageSpecification.Rgba32(frame.Width, frame.Height),
            pixels
        );

        frame.Dispose();
        return texture;
    }

    public void TriggerJumpscare()
    {
        if (!CanTriggerJumpscare())
            return;

        ResetAnimationState();
    }

    private bool CanTriggerJumpscare()
    {
        return _framesLoaded && _foxyFrames.Count > 0;
    }

    private void ResetAnimationState()
    {
        _isJumpscareActive = true;
        _lastFrameTime = DateTime.Now;
        _currentFrame = 0;
        _animationComplete = false;
    }

    private void DrawJumpscare()
    {
        if (!ShouldDrawJumpscare())
            return;

        UpdateAnimationFrame();

        var displaySize = ImGui.GetIO().DisplaySize;
        BringWindowToFront();
        SetupFullscreenWindow(displaySize);
        ApplyTransparentStyling();

        if (ImGui.Begin("##FoxyJumpscare", CreateFullscreenWindowFlags()))
        {
            DrawCurrentFrame(displaySize);
        }
        ImGui.End();

        RestoreImGuiStyles();
    }

    private static void BringWindowToFront()
    {
        ImGui.SetNextWindowFocus();
    }

    private bool ShouldDrawJumpscare()
    {
        return _isJumpscareActive && !_animationComplete && _framesLoaded;
    }

    private void UpdateAnimationFrame()
    {
        var timeSinceLastFrame = (DateTime.Now - _lastFrameTime).TotalSeconds;
        if (timeSinceLastFrame < _frameDuration)
            return;

        _lastFrameTime = DateTime.Now;

        if (IsOnLastFrame())
        {
            CompleteAnimation();
            return;
        }

        _currentFrame++;
    }

    private bool IsOnLastFrame()
    {
        return _currentFrame >= _foxyFrames.Count - 1;
    }

    private void CompleteAnimation()
    {
        _animationComplete = true;
        _isJumpscareActive = false;
    }

    private static void SetupFullscreenWindow(Vector2 displaySize)
    {
        ImGui.SetNextWindowPos(Vector2.Zero);
        ImGui.SetNextWindowSize(displaySize);
    }

    private static void ApplyTransparentStyling()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
    }

    private static ImGuiWindowFlags CreateFullscreenWindowFlags()
    {
        return ImGuiWindowFlags.NoTitleBar |
               ImGuiWindowFlags.NoResize |
               ImGuiWindowFlags.NoMove |
               ImGuiWindowFlags.NoScrollbar |
               ImGuiWindowFlags.NoScrollWithMouse |
               ImGuiWindowFlags.NoCollapse |
               ImGuiWindowFlags.NoSavedSettings |
               ImGuiWindowFlags.NoInputs |
               ImGuiWindowFlags.NoBackground;
    }

    private void DrawCurrentFrame(Vector2 displaySize)
    {
        var texture = _foxyFrames[_currentFrame];
        ImGui.Image(texture.Handle, displaySize);
    }

    private static void RestoreImGuiStyles()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleVar(3);
    }

    public void Dispose()
    {
        _uiBuilder.Draw -= DrawJumpscare;
        foreach (var frame in _foxyFrames)
        {
            frame?.Dispose();
        }
        _foxyFrames.Clear();
        GC.SuppressFinalize(this);
    }
}
