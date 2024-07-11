using System.Diagnostics;

namespace Saladim.Salix;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;
    public const string DefaultWindowTitle = nameof(Salix);

    private readonly Platform platform;
    private int ticks;
    private int laggedFrames;
    private bool requestedExit;
    private bool deferredInvoking;
    private readonly List<Action> deferredActions;
    private readonly Stopwatch stopwatch;

    public RenderContext RenderContext { get; private set; }
    public Window Window { get; private set; }
    public Platform Platform => platform;
    public ResourceLoader ResourceLoader { get; private set; }

    /// <summary>Indicates whether the game is lagging
    /// (<see cref="Fps"/> less than <see cref="TargetFps"/> remains for over 16 frames).</summary>
    public bool IsRunningSlowly => laggedFrames >= 16;

    /// <summary>Indicates how many frames have passed since the game started.</summary>
    public int Ticks => ticks;

    public TimeSpan ElapsedTime => stopwatch.Elapsed;

    /// <summary>Target frame time of the game.</summary>
    public double TargetFrameTime { get; set; }

    /// <summary>Target frame time of the game.</summary>
    public float TargetFrameTimeF => (float)TargetFrameTime;

    /// <summary>Target Fps of the game. This is just a shortcut to access '1.0 / <see cref="TargetFrameTime"/>'.</summary>
    public double TargetFps
    {
        get => 1.0 / TargetFrameTime;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), SR.ValueMustBePositive);
            TargetFrameTime = 1.0 / value;
        }
    }

    /// <summary>Actual frame time of the game.</summary>
    public double FrameTime { get; private set; }

    /// <summary>Actual fps of the game.</summary>
    public double Fps { get; private set; }

    /// <summary>Float-casted <see cref="FrameTime"/>.</summary>
    public float FrameTimeF => (float)FrameTime;

    /// <summary>Shortcut to access <see cref="RenderContext.VSyncEnabled"/>.</summary>
    public bool VSyncEnabled { get => RenderContext.VSyncEnabled; set => RenderContext.VSyncEnabled = value; }

    /// <summary>Shortcut to access <see cref="RenderContext.VSyncFrameTime"/>.</summary>
    public double VSyncFrameTime => RenderContext.VSyncFrameTime;

    /// <summary>Shortcut to 1d / <see cref="VSyncFrameTime"/>. Usally the refresh rate of your display.</summary>
    public double VSyncFps => 1d / RenderContext.VSyncFrameTime;

    public int LastDrawCalls { get; private set; }
    public KeyboardState KeyboardState => Window.KeyboardState;
    public MouseState MouseState => Window.MouseState;

    public Game()
    {
        deferredActions = new();
        platform = new Platform();
        platform.Initialize();
        ticks = 0;
        stopwatch = new();
        TargetFps = 60d;
        FrameTime = 1d / 60d;
        Window = new Window(this, DefaultWindowWidth, DefaultWindowHeight, DefaultWindowTitle);
        RenderContext = new RenderContext();
        RenderContext.AttachToWindow(Window);
        ResourceLoader = new(this);
    }

    /// <summary>The update logic.</summary>
    public virtual void Update() { }

    /// <summary>The render logic.</summary>
    public virtual void Render() { }

    public void InvokeDeferred(Action action)
    {
        if (deferredInvoking)
            action();
        else
            deferredActions.Add(action);
    }

    public void RequestExit()
        => requestedExit = true;

    private void Tick()
    {
        Update();
        Render();
        Window.Update();
        Window.SwapBuffers();
        ticks++;
    }

    public void Run()
    {
        long GetTimeUsec() => stopwatch.ElapsedTicks * 1_000_000 / Stopwatch.Frequency;

        RunBegin();
        Window.Show();
        Window.PollEvents();
        FrameTime = TargetFrameTime;

        // FIXME This strange operation avoids screen stuttering on the OpenGL33 backend and
        // minimizes tearing on the D3D11 backend as much as possible (without enabling VSync)
        bool needSync = platform.GraphicsBackend is GraphicsBackend.OpenGL33 or GraphicsBackend.DirectX11;
        if (needSync) SyncWithMonitor();

        stopwatch.Start();

        long current = GetTimeUsec();
        long target = current + (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : TargetFrameTime));
        LastDrawCalls = 0;
        while (true)
        {
            // -------------------------------------------------------------------------------------------
            // |s    e            |s           e      |s                |   e s      e     |s           e|
            // -------------------------------------------------------------------------------------------
            //               ↑ running correctly(e is before |)         ↑ running slowly(e is behind |)
            // s - e : processing & rendering
            // e - | : sleeping
            // | - | : one frame

            if (requestedExit)
                break;
            Window.PollEvents();

            long pdrawcalls = RenderContext.TotalDrawCalls;
            Tick();
            LastDrawCalls = (int)(RenderContext.TotalDrawCalls - pdrawcalls);

            RenderContext.ProcessQueuedActions();

            deferredInvoking = true;
            foreach (var item in deferredActions) item();
            deferredActions.Clear();
            deferredInvoking = false;
            if (Window.IsClosed)
                break;

            // now do sleep
            double frameTime = VSyncEnabled ? VSyncFrameTime : TargetFrameTime;
            long frameTimeUsec = (long)(1_000_000 * frameTime);
            long previous = current;
            current = GetTimeUsec();

            if (!VSyncEnabled)
            {
                int sleepTime = (int)(target - current);
                if (sleepTime > 1000)
                    Thread.Sleep(sleepTime / 1000);
            }
            FrameTime = (current - previous) / 1_000_000d;

            // TODO better fps calculation?
            var fpsFrameTime = frameTime;
            if (current > target)
                fpsFrameTime += (current - target) / 1_000_000d;

            Fps = 1 / fpsFrameTime;

            // if we're facing lagging
            if (current > target)
            {
                long times = (current - target) / frameTimeUsec + 1;
                target += times * frameTimeUsec;
                laggedFrames += 1;
                if (laggedFrames > 16)
                    laggedFrames = 16;
            }
            else
            {
                target += frameTimeUsec;
                laggedFrames -= 1;
                if (laggedFrames < 0)
                    laggedFrames = 0;
            }
        }
        stopwatch.Stop();
        // usually graphics resource deletions
        RenderContext.ProcessQueuedActions();
        RunEnd();
    }

    internal void SyncWithMonitor()
    {
        if (VSyncEnabled) return;

        VSyncEnabled = true;
        Window.SwapBuffers();
        Window.SwapBuffers();
        Window.SwapBuffers();
        VSyncEnabled = false;
    }

    public virtual void RunBegin()
    {
    }

    public virtual void RunEnd()
    {
    }
}