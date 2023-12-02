using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Monosand;

public class Window
{
    private bool isClosed = false;

    private Size size;
    private Point position;
    private readonly KeyboardState keyboardState;
    private readonly PointerState pointerState;

    private IntPtr nativeHandle;

    // TODO changes to libmsd will break this
    /// <summary>Native handle of this Window, on windows it's an HWND.</summary>
    public unsafe IntPtr Handle => (IntPtr)(*(void**)NativeHandle);

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
            char* str = stackalloc char[256];
            Interop.MsdGetWindowTitle(nativeHandle, str);
            return new string(str);
        }
        set
        {
            EnsureState();
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
        set { size.Width = value; Size = new(value, Height); }
    }

    /// <summary>The Height of this window.</summary>
    public int Height
    {
        get => size.Height;
        set { size.Height = value; Size = new(Width, value); }
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
        set { EnsureState(); size = value; Interop.MsdSetWindowSize(nativeHandle, value.Width, value.Height); }
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

    internal event Action? PreviewSwapBuffer;

    /// <summary>Construct a window.</summary>
    public unsafe Window(Game game, int width, int height, string title)
    {
        Game = game;
        keyboardState = new(this);
        pointerState = new(this);

        IntPtr winHandle;
        fixed (char* ptitle = title)
            winHandle = Interop.MsdCreateWindow(width, height, ptitle, (IntPtr)GCHandle.Alloc(this, GCHandleType.Weak));

        if (winHandle == IntPtr.Zero)
            throw new OperationFailedException("Can't create window.");

        nativeHandle = winHandle;
    }

    public Window(Game game) : this(game, Game.DefaultWindowWidth, Game.DefaultWindowHeight, nameof(Monosand))
    { }

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
        pointerState.Update();
    }

    internal void AttachRenderContext(RenderContext context)
        => Interop.MsdAttachRenderContext(NativeHandle, context.NativeHandle);

    internal unsafe void PollEvents()
    {
        EnsureState();
        Game.RenderContext.ProcessQueuedActions();
        int count;
        int* e;
        void* handle = Interop.MsdBeginPollEvents(nativeHandle, out var ncount, out e);
        if (ncount > int.MaxValue)
            throw new OperationFailedException("Too many win events.(> int.MaxValue)");
        count = (int)ncount;
        int sizeInInt = 4 + sizeof(IntPtr) / 4;

        // magic number at ../Monosand.Win32.Native/win32_msg_loop.cpp :: event
        for (int i = 0; i < count * sizeInInt; i += sizeInInt)
        {
            IntPtr v = ((IntPtr*)(e + i + 4))[0];
            Window win = HandleToWin(v);
            switch (e[i])
            {
            case 1: if (win.OnClosing()) Close(); break;
            case 3: win.OnMoved(e[i + 1], e[i + 2]); break;
            case 4: win.OnResized(e[i + 1], e[i + 2]); break;
            case 5: win.OnKeyPressed((Key)e[i + 1]); break;
            case 6: win.OnKeyReleased((Key)e[i + 1]); break;
            case 7: win.OnGotFocus(); break;
            case 8: win.OnLostFocus(); break;
            case 9:
            {
                int x = e[i + 1], y = e[i + 2];
                short btnType = *((short*)(e + i + 3) + 0);
                PointerButton button = (PointerButton)btnType;
                short downType = *((short*)(e + i + 3) + 1);
                if (downType == 0)
                    win.OnPointerPressed(x, y, button);
                else if (downType == 1)
                    win.OnPointerReleased(x, y, button);
                else if (downType == 2 && button == PointerButton.None)
                    win.OnPointerMoved(x, y);
            }
            break;
            case 10:
            {
                int x = e[i + 1], y = e[i + 2];
                int delta = e[i + 3];
                win.OnPointerWheelMoved(x, y, delta);
            }
            break;
            default: Debug.Fail("Unknown event type."); break;
            }
        }

        Interop.MsdEndPollEvents(nativeHandle, handle);

        static Window HandleToWin(IntPtr handle)
            => (Window)GCHandle.FromIntPtr(handle).Target!;
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

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);
}