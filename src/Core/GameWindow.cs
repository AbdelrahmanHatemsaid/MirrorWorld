using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTkWindow = OpenTK.Windowing.Desktop.GameWindow;

namespace MirrorWorld.Core;

public sealed class GameWindow : OpenTkWindow
{
    private readonly GameContext _context;

    public GameWindow(GameContext context)
        : base(CreateGameWindowSettings(), CreateNativeWindowSettings())
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(GameConstants.BackgroundClearColor);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        _context.UpdateViewportSize(Size.X, Size.Y);
        foreach (var system in _context.Systems)
        {
            system.Init(_context);
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        _context.UpdateViewportSize(e.Width, e.Height);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
            return;
        }

        float dt = MathF.Min((float)args.Time, GameConstants.MaxDeltaTime);
        _context.ElapsedTime += dt;

        foreach (var system in _context.Systems)
        {
            system.Update(dt, _context);
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        foreach (var system in _context.Systems)
        {
            system.Render(_context);
        }

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        _context.DisposeResources();
        base.OnUnload();
    }

    private static GameWindowSettings CreateGameWindowSettings() =>
        GameWindowSettings.Default;

    private static NativeWindowSettings CreateNativeWindowSettings() => new()
    {
        ClientSize = new Vector2i(GameConstants.WindowWidth, GameConstants.WindowHeight),
        Title = GameConstants.WindowTitle,
        APIVersion = new Version(3, 3),
        Profile = ContextProfile.Core,
        Flags = ContextFlags.ForwardCompatible
    };
}
