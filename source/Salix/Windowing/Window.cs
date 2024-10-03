using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Saladim.Salix;

public partial class Window : IDisposable
{
    private IWindowImpl? impl;
    internal IWindowImpl Impl { get { EnsureState(); return impl; } }

    private bool isClosed = false;

    private readonly KeyboardState keyboardState;
    private readonly MouseState mouseState;

    /// <summary>Indicates whether this window is closed or disposed.</summary>
    public bool IsClosed => isClosed;

    public unsafe string Title
    {
        get { EnsureState(); return impl.Title; }
        set
        {
            EnsureState();
            ThrowHelper.ThrowIfNull(value);
            impl.Title = value;
        }
    }

    /// <summary>The x position of this window.</summary>
    public int X { get => Position.X; set => Position = new(value, Y); }

    /// <summary>The y position of this window.</summary>
    public int Y { get => Position.Y; set => Position = new(X, value); }

    /// <summary>The width of this window.</summary>
    public int Width { get => Size.Width; set => Size = new(value, Height); }

    /// <summary>The height of this window.</summary>
    public int Height { get => Size.Height; set => Size = new(Width, value); }

    /// <summary>The position of this window on the screen.</summary>
    public Point Position
    {
        get { EnsureState(); return impl.Position; }
        set { EnsureState(); impl.Position = value; }
    }

    /// <summary>The size of this window.</summary>
    public Size Size
    {
        get { EnsureState(); return impl.Size; }
        set
        {
            EnsureState();
            if (value.Width < 1 || value.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(value), SR.InvalidWindowSize);
            impl.Size = value;
        }
    }

    /// <summary>The <see cref="Salix.KeyboardState"/> of this window. Usually used for getting keyboard input.</summary>
    public KeyboardState KeyboardState => keyboardState;

    /// <summary>The <see cref="Salix.MouseState"/> of this window. Usually used for getting mouse input.</summary>
    public MouseState MouseState => mouseState;

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

    public event Action<Window>? PreviewSwapBuffer;

    /// <summary>Construct a window.</summary>
    public Window(Platform platform, WindowConfig config)
    {
        ThrowHelper.ThrowIfNull(platform);
        ThrowHelper.ThrowIfNull(config);
        config.Verify();

        impl = platform.CreateWindowImpl(config.Width, config.Height, config.Title);

        keyboardState = new();
        mouseState = new();
    }

    internal void Update()
    {
        EnsureState();
        keyboardState.Update();
        mouseState.Update();
    }

    internal void PollEvents()
    {
        EnsureState();
        impl.PollEvents(this);
    }

    public void Show()
    {
        EnsureState();
        impl.Show();
    }

    public void Hide()
    {
        EnsureState();
        impl.Hide();
    }

    public void Close()
    {
        EnsureState();
        impl.Close();
        impl.Dispose();
        isClosed = true;
        impl = null;
        OnClosed();
    }

    internal void SwapBuffers()
    {
        EnsureState();
        PreviewSwapBuffer?.Invoke(this);
        impl.SwapBuffers();
    }

    /// <summary>Called when the window is closed. This is happened <strong>after</strong> the disposing.</summary>
    protected internal virtual void OnClosed()
        => Closed?.Invoke(this);

    /// <summary>Called when the user requests a closing. (For example click the close button)</summary>
    /// <returns><see langword="false"/> to reject the closing.</returns>
    protected internal virtual bool OnClosing()
        => true;

    /// <summary>Called when the window moved.</summary>
    protected internal virtual void OnMoved(int x, int y)
        => Moved?.Invoke(this, x, y);

    /// <summary>Called when the window resized.</summary>
    protected internal virtual void OnResized(int width, int height)
        => Resized?.Invoke(this, width, height);

    /// <summary>Called when a key pressed.</summary>
    protected internal virtual void OnKeyPressed(Key key)
        => KeyboardState.SetTrue(key);

    /// <summary>Called when a key released.</summary>
    protected internal virtual void OnKeyReleased(Key key)
        => KeyboardState.SetFalse(key);

    /// <summary>Called when the window lost focus.</summary>
    protected internal virtual void OnLostFocus()
    {
        KeyboardState.Clear();
        MouseState.Clear();
        LostFocus?.Invoke(this);
    }

    /// <summary>Called when the window got focus.</summary>
    protected internal virtual void OnGotFocus()
        => GotFocus?.Invoke(this);

    protected internal virtual void OnMouseButtonPressed(int x, int y, MouseButton button)
        => MouseState.SetTrue(1 << (int)button);

    protected internal virtual void OnMouseButtonReleased(int x, int y, MouseButton button)
        => MouseState.SetFalse(1 << (int)button);

    protected internal virtual void OnMouseMoved(int x, int y)
        => MouseState.SetPosition(new(x, y));

    protected internal virtual void OnMouseWheelMoved(float delta)
        => MouseState.AddWheelDelta(delta);

    [MemberNotNull(nameof(impl))]
    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(impl is null, this);

#pragma warning disable CA1816
    public virtual void Dispose()
    {
        if (impl is null) return;
        impl.Dispose();
        impl = null;
    }
#pragma warning restore CA1816
}