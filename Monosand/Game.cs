namespace Monosand;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    //private static Game? instance;
    private Window? window;
    private Platform platform;
    private long ticks;
    private int laggedFrames;

    public RenderContext RenderContext { get; private set; }
    public Window Window
    {
        get => window ?? throw SR.PropNotSet(nameof(Window));
        set
        {
            if (window is not null)
                throw SR.PropSet(nameof(Window));
            window = value;
            platform.AttachRenderContext(RenderContext, window);
            window.OnCreated();
        }
    }

    //public static Game Instance
    //{
    //    get => instance ?? throw SR.GameNotNewed;
    //    set => instance = instance is null ? value : throw SR.GameHasBeenNewed;
    //}

    public Platform Platform => platform;
    public ResourceLoader ResourceLoader { get; }
    /// <summary>Indicates whether the game is lagging
    /// (<see cref="Fps"/> less than <see cref="ExpectedFps"/> remains for over 5 frames.).</summary>
    public bool IsRunningSlowly => laggedFrames > 5;

    /// <summary>Indicates how many frames have passed since the game started.</summary>
    public long Ticks => ticks;

    /// <summary>Expected frame time of the game.</summary>
    public double ExpectedFrameTime { get; set; }

    /// <summary>Expected frame time of the game.</summary>
    public float ExpectedFrameTimeF => (float)ExpectedFrameTime;

    /// <summary>Expected Fps of the game. This is just a shortcut to access '1.0 / <see cref="ExpectedFrameTime"/>'.</summary>
    public double ExpectedFps { get => 1.0 / ExpectedFrameTime; set => ExpectedFrameTime = 1.0 / value; }

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

    public Game(Platform platform)
    {
        ThrowHelper.ThrowIfNull(platform);
        platform.Initialize();
        RenderContext = platform.CreateRenderContext();

        //Instance = this;
        this.platform = platform;
        ExpectedFps = 60d;
        FrameTime = 1d / ExpectedFps;
        ticks = 0;
        ResourceLoader = new ResourceLoader(this);
    }

    public void Run()
    {
        ThrowHelper.ThrowIfInvalid(window is null);
        window.Show();
        FrameTime = ExpectedFrameTime;

        // FIXME this is a silly method to sync with the display
        if (!VSyncEnabled)
        {
            VSyncEnabled = true;
            Window.SwapBuffers();
            Window.SwapBuffers();
            Window.SwapBuffers();
            VSyncEnabled = false;
        }

        long currentTimeLine = platform.GetUsecTimeline();
        long expectedTimeLine = currentTimeLine + (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime));
        while (true)
        {
            // -------------------------------------------------------------------------------------------
            // |s    e            |s           e      |s                |   e s      e     |s           e|
            // -------------------------------------------------------------------------------------------
            //               ↑ running correctly(e is before |)         ↑ running slowly(e is behind |)
            // s - e : processing & rendering
            // e - | : sleeping
            // | - | : one frame


            // ----- tick ------

            RenderContext.ProcessQueuedActions();
            window.PollEvents();
            if (window.IsClosed)
                break;
            window.Tick();

            ticks++;
            // -----------------

            long realFrameTimeUsec = (long)(1_000_000 * (VSyncEnabled ? VSyncFrameTime : ExpectedFrameTime));
            long pCurrentTimeLine = currentTimeLine;
            currentTimeLine = platform.GetUsecTimeline();
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
        RenderContext.ProcessQueuedActions();
    }
}