namespace Monosand;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    private static Game? instance;
    private Window window;
    private Platform platform;
    private long ticks;
    private double expectedFrameTime;

    internal WinImpl WinImpl => window.WinImpl;
    internal RenderContext RenderContext => WinImpl.GetRenderContext();

    public static Game Instance
    {
        get => instance ?? throw SR.GameNotNewed;
        set => instance = instance is null ? value : throw SR.GameHasBeenNewed;
    }

    public Platform Platform => platform;
    public ResourceLoader ResourceLoader { get; }
    public long Ticks => ticks;
    public double ExpectedFrameTime { get => expectedFrameTime; set => expectedFrameTime = value; }
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
            if (value is null)
                throw new ArgumentNullException(nameof(value));
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
        while (true)
        {
            // FIXME desync may happened here with this way of frame rate control implement
            long before = platform.GetUsecTimeline();

            // ---- start ticking the game
            Window.PollEvents();
            if (Window.IsInvalid) break;
            Window.Tick();
            ticks++;
            // ----

            long after = platform.GetUsecTimeline();

            long passed = after - before;
            double usecPerFrame = ExpectedFrameTime * 1000 * 1000;

            // FIXME any way to get a stable, dynamic FrameTime instead of a fixed VSyncFrameTime?
            if (VSyncEnabled)
                FrameTime = VSyncFrameTime;
            else
                FrameTime = Math.Max(passed / 1000d / 1000d, ExpectedFrameTime);
            if (!VSyncEnabled && passed < usecPerFrame)
            {
                double ticksPerUsec = TimeSpan.TicksPerMillisecond / 1000d;
                double toSleepUsec = usecPerFrame - passed;
                double toSleep = ticksPerUsec * toSleepUsec;
                Thread.Sleep(TimeSpan.FromTicks((long)toSleep));
            }
        }
    }
}