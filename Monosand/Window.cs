using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Monosand;

public partial class Window
{
    private bool isClosed = false;

    private Size size;
    private Point position;
    private readonly KeyboardState keyboardState;
    private readonly MouseState mouseState;

    private IntPtr nativeHandle;

    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    /// <summary>Is this window closed, will be <see langword="true"/> when the window closed or disposed.</summary>
    public bool IsClosed => isClosed;

    /// <summary>The <see cref="Monosand.Game"/> instance this window belong to.</summary>
    public Game Game { get; private set; }

    public unsafe string Title
    {
        get
        {
            EnsureState();
            int len = Interop.MsdGetWindowTitle(nativeHandle, null);
            char* pchar = (char*)Marshal.AllocHGlobal(len * sizeof(char));
            _ = Interop.MsdGetWindowTitle(nativeHandle, pchar);
            string str = new string(pchar);
            Marshal.FreeHGlobal((nint)pchar);
            return str;
        }
        set
        {
            EnsureState();
            ThrowHelper.ThrowIfNull(value);
            fixed (char* str = value)
                Interop.MsdSetWindowTitle(nativeHandle, str);
        }
    }

    #region Position & Size

    /// <summary>The X coord of this window.</summary>
    public int X
    {
        get => position.X;
        set { position.X = value; Position = new(value, Y); }
    }

    /// <summary>The Y coord of this window.</summary>
    public int Y
    {
        get => position.Y;
        set { position.Y = value; Position = new(X, value); }
    }

    /// <summary>The Width of this window.</summary>
    public int Width
    {
        get => size.Width;
        set { Size = new(value, Height); size.Width = value; }
    }

    /// <summary>The Height of this window.</summary>
    public int Height
    {
        get => size.Height;
        set { Size = new(Width, value); size.Height = value; }
    }

    /// <summary>The Position of this window on the screen.</summary>
    public Point Position
    {
        get { EnsureState(); return position; }
        set { EnsureState(); position = value; Interop.MsdSetWindowPos(nativeHandle, value.X, value.Y); }
    }

    /// <summary>The Size of this window.</summary>
    public Size Size
    {
        get { EnsureState(); return size; }
        set
        {
            if (value.Width < 1 || value.Height < 1)
                throw new ArgumentOutOfRangeException(nameof(value), SR.InvalidWindowSize);
            EnsureState();
            size = value;
            Interop.MsdSetWindowSize(nativeHandle, value.Width, value.Height);
        }
    }

    #endregion

    /// <summary>The <see cref="Monosand.KeyboardState"/> of this window. Usually used for getting keyboard input.</summary>
    public KeyboardState KeyboardState => keyboardState;

    /// <summary>The <see cref="Monosand.MouseState"/> of this window. Usually used for getting mouse input.</summary>
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

    public event Action? PreviewSwapBuffer;

    /// <summary>Construct a window.</summary>
    internal unsafe Window(Game game, int width, int height, string title)
    {
        ThrowHelper.ThrowIfNull(game);
        if (width < 1)
            throw new ArgumentOutOfRangeException(nameof(width), SR.InvalidWindowSize);
        if (height < 1)
            throw new ArgumentOutOfRangeException(nameof(height), SR.InvalidWindowSize);
        ThrowHelper.ThrowIfNull(title);

        Game = game;
        keyboardState = new(this);
        mouseState = new(this);

        IntPtr winHandle;
        fixed (char* ptitle = title)
        {
            var r = Interop.MsdCreateWindow(width, height, ptitle, (IntPtr)GCHandle.Alloc(this, GCHandleType.Weak));
            if (r.OK)
                winHandle = r.Value;
            else
                throw new FrameworkException(SR.FailedToCreateWindow, new ErrorCodeException(r.ErrorCode));
        }

        nativeHandle = winHandle;
    }

    public void Show()
    {
        EnsureState();
        Interop.MsdShowWindow(nativeHandle);
    }

    public void Hide()
    {
        EnsureState();
        Interop.MsdHideWindow(nativeHandle);
    }

    public void Close()
    {
        EnsureState();
        Game.InvokeDeferred(() =>
        {
            Interop.MsdDestroyWindow(nativeHandle);
            isClosed = true;
            nativeHandle = IntPtr.Zero;
            OnClosed();
        });
    }

    internal void SwapBuffers()
    {
        EnsureState();
        PreviewSwapBuffer?.Invoke();
        Interop.MsdgSwapBuffers(nativeHandle);
    }

    internal void Update()
    {
        EnsureState();
        keyboardState.Update();
        mouseState.Update();
    }

    internal void AttachRenderContext(RenderContext context)
    {
        var r = Interop.MsdAttachRenderContext(NativeHandle, context.NativeHandle);
        if (!r.OK)
            throw new FrameworkException(SR.FailedToAttachRenderContext, new ErrorCodeException(r.ErrorCode, r.PlatformResult));
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
        Game.RenderContext.OnWindowResized(width, height);
        size = new(width, height);
        Resized?.Invoke(this, width, height);
    }

    /// <summary>Called when the window is ready to initialize.</summary>
    public virtual void OnInitialize()
    {
        var r = Interop.MsdGetWindowRect(nativeHandle);
        size = new(r.right - r.left, r.bottom - r.top);
        position = new(r.left, r.top);
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
        MouseState.Clear();
        LostFocus?.Invoke(this);
    }

    /// <summary>Called when the window got focus.</summary>
    public virtual void OnGotFocus()
    {
        GotFocus?.Invoke(this);
    }

    public virtual void OnMouseButtonPressed(int x, int y, MouseButton button)
    {
        MouseState.SetTrue(1 << (int)button);
    }

    public virtual void OnMouseButtonReleased(int x, int y, MouseButton button)
    {
        MouseState.SetFalse(1 << (int)button);
    }

    public virtual void OnMouseMoved(int x, int y)
    {
        MouseState.SetPosition(new(x, y));
    }

    public virtual void OnMouseWheelMoved(float delta)
    {
        MouseState.AddWheelDelta(delta);
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);
}