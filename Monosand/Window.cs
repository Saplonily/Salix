using System.Drawing;

namespace Monosand;

public class Window
{
    internal WindowImpl Impl { get; private set; }
    private bool isClosed = false;

    private Size size;
    private Point position;
    private readonly KeyboardState keyboardState;
    private readonly PointerState pointerState;

    public string Title { get => Impl.Title; set => Impl.Title = value; }

    /// <summary>Is this window closed, will be <see langword="true"/> when the window closed or disposed.</summary>
    public bool IsClosed => isClosed;

    /// <summary>The <see cref="Monosand.Game"/> instance this window belong to.</summary>
    public Game Game { get; private set; }

    #region Position & Size
    /// <summary>The X coord of this window.</summary>
    public int X
    {
        get => position.X;
        set { position.X = value; Impl.Position = new(value, Y); }
    }

    /// <summary>The Y coord of this window.</summary>
    public int Y
    {
        get => position.Y;
        set { position.Y = value; Impl.Position = new(X, value); }
    }

    /// <summary>The Width of this window.</summary>
    public int Width
    {
        get => size.Width;
        set { size.Width = value; Impl.Size = new(value, Height); }
    }

    /// <summary>The Height of this window.</summary>
    public int Height
    {
        get => size.Height;
        set { size.Height = value; Impl.Size = new(Width, value); }
    }

    /// <summary>The Position of this window on the screen.</summary>
    public Point Position
    {
        get => position;
        set { position = value; Impl.Position = new(value.X, value.Y); }
    }

    /// <summary>The Size of this window.</summary>
    public Size Size
    {
        get => size;
        set { size = value; Impl.Size = new(value.Width, value.Height); }
    }
    #endregion

    /// <summary>The <see cref="Monosand.KeyboardState"/> of this window. Usually used for getting keyboard input.</summary>
    public KeyboardState KeyboardState => keyboardState;

    /// <summary>The <see cref="Monosand.PointerState"/> of this window. Usually used for getting mouse input.</summary>
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
    public Window(Game game)
    {
        Game = game;
        keyboardState = new(this);
        pointerState = new(this);
        Impl = Game.Platform.CreateWindowImpl(Game.DefaultWindowWidth, Game.DefaultWindowHeight, nameof(Monosand), this);
    }

    /// <summary>Show the window.</summary>
    public void Show() => Impl.Show();

    /// <summary>Hide the window.</summary>
    public void Hide() => Impl.Hide();

    /// <summary>Close the window.</summary>
    public void Close() => Impl.Destroy();
    internal void PollEvents() => Impl.PollEvents();

    internal void OnCallbackDestroy()
    {
        isClosed = true;
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
        if (Game.RenderContext is not null)
            Game.RenderContext.Viewport = new(0, 0, width, height);
        size = new(width, height);
        Resized?.Invoke(this, width, height);
    }

    /// <summary>Called when the window created.</summary>
    public virtual void OnCreated()
    {
        size = Impl.Size;
        position = Impl.Position;
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

    public virtual void OnPointerPressed(int x, int y, PointerButton button)
    {
        PointerState.SetTrue(1 << (int)button);
        PointerState.SetPosition(new(x, y));
    }

    public virtual void OnPointerReleased(int x, int y, PointerButton button)
    {
        PointerState.SetFalse(1 << (int)button);
        PointerState.SetPosition(new(x, y));
    }

    public virtual void OnPointerMoved(int x, int y)
    {
        PointerState.SetPosition(new(x, y));
    }

    public virtual void OnPointerWheelMoved(int x, int y, float delta)
    {
        PointerState.SetPosition(new(x, y));
        PointerState.AddWheelDelta(delta);
    }

    internal void SwapBuffers() => Impl.SwapBuffers();

    internal void Tick()
    {
        ThrowHelper.ThrowIfInvalid(Game.RenderContext is null, "No RenderContext attched to this window.");

        Update();
        Game.RenderContext.Clear(Color.CornflowerBlue);
        Render();
        Impl.SwapBuffers();
        // I think someone might try handling input in Render()
        // make them happy
        keyboardState.Update();
        pointerState.Update();
    }

    /// <summary>The update logic.</summary>
    public virtual void Update()
    {
    }

    /// <summary>The render logic.</summary>
    public virtual void Render()
    {
    }
}