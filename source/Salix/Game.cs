﻿namespace Saladim.Salix;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;
    public const string DefaultWindowTitle = nameof(Saladim.Salix);

    private readonly Platform platform;
    private long ticks;
    private int laggedFrames;
    private bool requestedExit;
    private bool deferredInvoking;
    private List<Action> deferredActions;

    public RenderContext RenderContext { get; private set; }
    public Window Window { get; private set; }
    public Platform Platform => platform;
    public ResourceLoader ResourceLoader { get; private set; }

    /// <summary>Indicates whether the game is lagging
    /// (<see cref="Fps"/> less than <see cref="ExpectedFps"/> remains for over 16 frames).</summary>
    public bool IsRunningSlowly => laggedFrames > 5;

    /// <summary>Indicates how many frames have passed since the game started.</summary>
    public long Ticks => ticks;

    /// <summary>Expected frame time of the game.</summary>
    public double ExpectedFrameTime { get; set; }

    /// <summary>Expected frame time of the game.</summary>
    public float ExpectedFrameTimeF => (float)ExpectedFrameTime;

    /// <summary>Expected Fps of the game. This is just a shortcut to access '1.0 / <see cref="ExpectedFrameTime"/>'.</summary>
    public double ExpectedFps
    {
        get => 1.0 / ExpectedFrameTime;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), SR.ValueMustBePositive);
            ExpectedFrameTime = 1.0 / value;
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
        ExpectedFps = 60d;
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

    public void Tick()
    {
        long pdrawcalls = RenderContext.TotalDrawCalls;
        Update();
        Render();
        Window.Update();
        Window.SwapBuffers();
        LastDrawCalls = (int)(RenderContext.TotalDrawCalls - pdrawcalls);
        RenderContext.ProcessQueuedActions();
        deferredInvoking = true;
        foreach (var item in deferredActions) item();
        deferredActions.Clear();
        deferredInvoking = false;
        ticks++;
    }

    public void Run()
    {
        RunBegin();
        Window.Show();
        Window.PollEvents();
        FrameTime = ExpectedFrameTime;

        //// FIXME this is a silly method to sync with the display
        //if (!VSyncEnabled)
        //{
        //    VSyncEnabled = true;
        //    Window.SwapBuffers();
        //    Window.SwapBuffers();
        //    Window.SwapBuffers();
        //    VSyncEnabled = false;
        //}

        long currentTimeLine = Interop.SLX_GetUsecTimeline();
        long expectedTimeLine = currentTimeLine + (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime));
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
            Tick();
            if (Window.IsClosed)
                break;

            // now do sleep
            long realFrameTimeUsec = (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime));
            long pCurrentTimeLine = currentTimeLine;
            currentTimeLine = Interop.SLX_GetUsecTimeline();
            int toSleepUsec = (int)(expectedTimeLine - currentTimeLine);
            if (!VSyncEnabled)
            {
                if (toSleepUsec > 1000)
                    Thread.Sleep(toSleepUsec / 1000);
            }
            FrameTime = (currentTimeLine - pCurrentTimeLine) / 1_000_000d;

            // TODO This fps implementation is a bit weird
            var fpsFT = VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime;
            if (currentTimeLine > expectedTimeLine)
                fpsFT += (currentTimeLine - expectedTimeLine) / 1_000_000d;
            Fps = 1 / fpsFT;

            // if we're facing lagging
            if (currentTimeLine > expectedTimeLine)
            {
                long times = (currentTimeLine - expectedTimeLine) / realFrameTimeUsec + 1;
                expectedTimeLine += times * realFrameTimeUsec;
                laggedFrames += 1;
                if (laggedFrames > 16)
                    laggedFrames = 16;
            }
            else
            {
                expectedTimeLine += realFrameTimeUsec;
                laggedFrames -= 1;
                if (laggedFrames < 0)
                    laggedFrames = 0;
            }
        }
        // usually graphics resource deletions
        RenderContext.ProcessQueuedActions();
        RunEnd();
    }

    public virtual void RunBegin()
    {
    }

    public virtual void RunEnd()
    {
    }
}