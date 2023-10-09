using System.Drawing;

namespace Monosand;

public class Window : IDisposable
{
    private Size size;
    private Point position;

    private WinImpl? impl;
    private Game? game;
    private RenderContext? rc;
    private readonly KeyboardState keyboardState;
    private readonly PointerState pointerState;

    /// <summary>Is this window invalid, usually be <see langword="true"/> when the window closed or disposed.</summary>
    public bool IsInvalid => impl == null || game == null;

    /// <summary>The <see cref="Monosand.Game"/> instance the window belong to.</summary>
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

    /// <summary>
    /// The <see cref="Monosand.RenderContext"/> of this window, use it to call methods like
    /// <see cref="RenderContext.DrawPrimitives{T}(VertexDeclaration, PrimitiveType, T*, int)"/> or
    /// construct a <see cref="SpriteBatch"/> with it.
    /// </summary>
    public RenderContext RenderContext => rc ?? throw SR.PropNotSet(nameof(RenderContext));

    #region Position & Size
    /// <summary>The X coord of this window.</summary>
    public int X
    {
        get => position.X;
        set { position.X = value; WinImpl.SetPosition(value, Y); }
    }

    /// <summary>The Y coord of this window.</summary>
    public int Y
    {
        get => position.Y;
        set { position.Y = value; WinImpl.SetPosition(X, value); }
    }

    /// <summary>The Width of this window.</summary>
    public int Width
    {
        get => size.Width;
        set { size.Width = value; WinImpl.SetSize(value, Height); }
    }

    /// <summary>The Height of this window.</summary>
    public int Height
    {
        get => size.Height;
        set { size.Height = value; WinImpl.SetSize(Width, value); }
    }

    /// <summary>The Position of this window on the screen.</summary>
    public Point Position
    {
        get => position;
        set { position = value; WinImpl.SetPosition(value.X, value.Y); }
    }

    /// <summary>The Size of this window.</summary>
    public Size Size
    {
        get => size;
        set { size = value; WinImpl.SetSize(value.Width, value.Height); }
    }
    #endregion

    /// <summary>The <see cref="Monosand.KeyboardState"/> of this window.</summary>
    public KeyboardState KeyboardState => keyboardState;

    public PointerState PointerState => pointerState;

    /// <summary>Occurs after the window closed. After the <see cref="OnClosing"/> be called.</summary>
    public event Action<Window>? Closed;
    /// <summary>Occurs after the window moved.</summary>
    public event Action<Window, int, int>? Moved;
    /// <summary>Occurs after the window resized.</summary>
    public event Action<Window, int, int>? Resized;
    /// <summary>Occurs after the window lost focus.</summary>
    public event Action<Window>? LostFocus;
    /// <summary>Occurs after the window got focus.</summary>
    public event Action<Window>? GotFocus;

    /// <summary>Construct a window.</summary>
    public Window()
    {
        keyboardState = new(this);
        pointerState = new(this);
    }

    private void InitCreateWindow()
    {
        impl = Game.Platform.CreateWindowImpl(Game.DefaultWindowWidth, Game.DefaultWindowHeight, nameof(Monosand), this);
        rc = impl.GetRenderContext();
        // don't do this at this time
        // we can just do this in OnResize()
        // as the Show() will actually call the OnResize()
        //WinImpl.SetViewport(0, 0, Game.DefaultWindowWidth, Game.DefaultWindowHeight);
    }

    /// <summary>Show the window.</summary>
    public void Show() => WinImpl.Show();

    /// <summary>Hide the window.</summary>
    public void Hide() => WinImpl.Hide();

    /// <summary>Close the window.</summary>
    public void Close() => WinImpl.Destroy();
    internal void PollEvents() => WinImpl.PollEvents();

    internal void OnCallbackDestroy()
    {
        impl!.MainThreadDispose();
        // prevent our Window from destroying twice when closing
        impl = null;
        Dispose(true);
        OnClosed();
    }

    /// <summary>Called when the window closed.</summary>
    public virtual void OnClosed()
        => Closed?.Invoke(this);

    /// <summary>Called when the user requests a closing. (for example, click the close button)</summary>
    /// <returns><see langword="false"/> to reject the closing.</returns>
    public virtual bool OnClosing()
        => true;

    /// <summary>Called when the window moved.</summary>
    public virtual void OnMoved(int x, int y)
    {
        position = new(x, y);
        Moved?.Invoke(this, x, y);
    }

    /// <summary>Called when the window resized.</summary>
    public virtual void OnResized(int width, int height)
    {
        RenderContext.SetViewport(0, 0, width, height);
        size = new(width, height);
        Resized?.Invoke(this, width, height);
    }

    /// <summary>Called when the window created.</summary>
    public virtual void OnCreated()
    {
        size = WinImpl.GetSize();
        position = WinImpl.GetPosition();
    }

    /// <summary>Called when a key pressed.</summary>
    public virtual void OnKeyPressed(Key key)
        => KeyboardState.SetTrue(key);

    /// <summary>Called when a key released.</summary>
    public virtual void OnKeyReleased(Key key)
        => KeyboardState.SetFalse(key);

    /// <summary>Called when the window lost focus.</summary>
    public virtual void OnLostFocus()
    {
        KeyboardState.Clear();
        PointerState.Clear();
        LostFocus?.Invoke(this);
    }

    /// <summary>Called when the window got focus.</summary>
    public virtual void OnGotFocus()
    {
        GotFocus?.Invoke(this);
    }

    public virtual void OnPointerPressed(PointerButton button)
    {
        PointerState.SetTrue(1 << (int)button);
    }

    public virtual void OnPointerReleased(PointerButton button)
    {
        PointerState.SetFalse(1 << (int)button);
    }

    internal void RenderInternal()
    {
        RenderContext.Clear(Color.CornflowerBlue);
        Render();
        RenderContext.SwapBuffers();
    }

    internal void UpdateInternal()
    {
        Update();
        keyboardState.Update();
    }

    /// <summary>The update logic.</summary>
    public virtual void Update()
    {
    }

    /// <summary>The render logic.</summary>
    public virtual void Render()
    {
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    protected virtual void Dispose(bool disposing)
    {
        if (impl != null)
        {
            impl.Destroy();
            impl = null;
        }
    }

    ~Window()
        => Dispose(disposing: false);

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}