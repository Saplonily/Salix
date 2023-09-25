using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace Monosand;

public class Window : IDisposable
{
    private WinImpl? impl;
    private Game? game;
    private RenderContext? rc;

    public bool IsInvalid => impl == null || game == null;
    public Game Game
    {
        get => game ?? throw SR.PropNotSet(nameof(Game));
        internal set
        {
            if (game is not null) throw SR.PropSet(nameof(Game));
            game = value;
            InitCreateWindow();
        }
    }
    internal WinImpl WinImpl => impl ?? throw SR.PropNotSet(nameof(WinImpl));
    public RenderContext RenderContext => rc ?? throw SR.PropNotSet(nameof(RenderContext));
    public int X
    {
        get => WinImpl.GetPosition().X;
        set => WinImpl.SetPosition(value, Y);
    }
    public int Y
    {
        get => WinImpl.GetPosition().Y;
        set => WinImpl.SetPosition(X, value);
    }
    public int Width
    {
        get => WinImpl.GetSize().Width;
        set => WinImpl.SetSize(value, Height);
    }
    public int Height
    {
        get => WinImpl.GetSize().Height;
        set => WinImpl.SetSize(Width, value);
    }
    public Point Position
    {
        get => WinImpl.GetPosition();
        set => WinImpl.SetPosition(value.X, value.Y);
    }
    public Size Size
    {
        get => WinImpl.GetSize();
        set => WinImpl.SetSize(value.Width, value.Height);
    }

    public event Action<Window>? Closed;
    public event Action<Window, int, int>? Moved;

    public Window()
    {
    }

    public Window(Game game)
    {
        this.game = game;
        InitCreateWindow();
    }

    private void InitCreateWindow()
    {
        impl = Game.platform.CreateWindowImpl(Game.DefaultWindowWidth, Game.DefaultWindowHeight, nameof(Monosand), this);
        rc = impl.GetRenderContext();
        // don't do this at this time
        // we can just do this in OnResize()
        // as the Show() will actually call the OnResize()
        //WinImpl.SetViewport(0, 0, Game.DefaultWindowWidth, Game.DefaultWindowHeight);
    }

    public void Show() => WinImpl.Show();
    public void Hide() => WinImpl.Hide();
    internal void PollEvents() => WinImpl.PollEvents();

    public virtual bool OnRequestedClose() => true;

    internal void OnCallbackDestroy()
    {
        // prevent our Window from destroying twice when closing
        impl = null;
        Dispose(true);
        OnClosed();
    }

    public virtual void OnClosed() => Closed?.Invoke(this);
    public virtual bool OnClosing() => true;
    public virtual void OnMoved(int x, int y) => Moved?.Invoke(this, x, y);
    public virtual void OnResize(int width, int height) => RenderContext.SetViewport(0, 0, width, height);
    public virtual void OnCreated() { }

    internal void RenderInternal()
    {
        RenderContext.Clear(Color.CornflowerBlue);
        Render();
        RenderContext.SwapBuffers();
    }

    public virtual void Update()
    {
    }

    public virtual void Render()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (impl != null)
        {
            impl.Destroy();
            impl = null;
        }
    }

    ~Window() => Dispose(disposing: false);

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}