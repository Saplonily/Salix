﻿using System.Diagnostics;

namespace Monosand;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    private static Game? instance;
    private Window window;
    private Platform platform;
    private long ticks;
    private int laggedFrames;

    internal WinImpl WinImpl => window.WinImpl;
    internal RenderContext RenderContext => WinImpl.GetRenderContext();

    public static Game Instance
    {
        get => instance ?? throw SR.GameNotNewed;
        set => instance = instance is null ? value : throw SR.GameHasBeenNewed;
    }

    public Platform Platform => platform;
    public ResourceLoader ResourceLoader { get; }
    public bool IsRunningSlowly => laggedFrames > 5;
    public long Ticks => ticks;
    public double ExpectedFrameTime { get; set; }
    public double ExpectedFps { get => 1.0 / ExpectedFrameTime; set => ExpectedFrameTime = 1.0 / value; }
    public double FrameTime { get; private set; }
    public double Fps { get => 1d / FrameTime; }
    public float FrameTimeF => (float)FrameTime;

    /// <summary>Shortcut to access <see cref="RenderContext.VSyncEnabled"/>.</summary>
    public bool VSyncEnabled { get => RenderContext.VSyncEnabled; set => RenderContext.VSyncEnabled = value; }

    /// <summary>Shortcut to access <see cref="RenderContext.VSyncFrameTime"/>.</summary>
    public double VSyncFrameTime => RenderContext.VSyncFrameTime;

    /// <summary>Shortcut to 1d / <see cref="VSyncFrameTime"/>.</summary>
    public double VSyncFps => 1d / RenderContext.VSyncFrameTime;

    public Window Window
    {
        get => window;
        set
        {
            ThrowHelper.ThrowIfNull(value);
            window = value;
            window.Game = this;
        }
    }

    public Game(Platform platform, Window? window = null)
    {
        ThrowHelper.ThrowIfNull(platform);
        Instance = this;
        this.platform = platform;
        this.window = null!;
        ExpectedFps = 60d;
        FrameTime = 1d / ExpectedFps;
        ticks = 0;
        platform.Init();
        Window = window ?? new Window();

        ResourceLoader = new ResourceLoader(this);

        Window.OnCreated();
    }

    public void Run()
    {
        Window.Show();
        FrameTime = ExpectedFrameTime;

        // FIXME this is a silly method to sync with the monitor
        if (!VSyncEnabled)
        {
            VSyncEnabled = true;
            Window.RenderContext.SwapBuffers();
            Window.RenderContext.SwapBuffers();
            Window.RenderContext.SwapBuffers();
            VSyncEnabled = false;
        }

        long expectedTimeLine = platform.GetUsecTimeline() + (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime));
        while (true)
        {
            // ----------------------------------------------------------------------
            // |s    e            |s           e      |s                |   e
            // ----------------------------------------------------------------------
            //               ↑ running correctly(e is before |)         ↑ running slowly(e is behind |)
            // s - e : processing & rendering
            // e - | : sleeping
            // | - | : one frame


            // ----- tick ------
            Window.PollEvents();
            if (Window.IsInvalid) break;
            Window.Tick();
            ticks++;
            // -----------------

            long realFrameTimeUsec = (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime));
            long currentTimeLine = platform.GetUsecTimeline();
            int toSleepUsec = (int)(expectedTimeLine - currentTimeLine);
            if (!VSyncEnabled)
            {
                if (toSleepUsec > 1000)
                    Thread.Sleep(toSleepUsec / 1000);
            }
            FrameTime = VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime;

            // if we're facing lagging
            if (currentTimeLine > expectedTimeLine)
            {
                FrameTime += (currentTimeLine - expectedTimeLine) / 1_000_000d;
                long distance = currentTimeLine - expectedTimeLine;
                long times = distance / realFrameTimeUsec + 1;
                expectedTimeLine += times * realFrameTimeUsec;
                laggedFrames += 1;
                if (laggedFrames > 10)
                    laggedFrames = 10;
            }
            else
            {
                expectedTimeLine += realFrameTimeUsec;
                laggedFrames -= 1;
                if (laggedFrames < 0)
                    laggedFrames = 0;
            }
        }
    }
}