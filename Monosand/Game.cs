namespace Monosand;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    private static Game? instance;
    private Window window;
    private Platform platform;
    private long ticks;
    private double expectedDelta;

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
    public double ExpectedDelta { get => expectedDelta; set => expectedDelta = value; }
    public double ExpectedFps { get => 1.0 / ExpectedDelta; set => ExpectedDelta = 1.0 / value; }
    public double Delta { get; private set; }
    public double Fps { get => 1d / Delta; }

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
        Delta = 1d / ExpectedFps;
        ticks = 0;
        platform.Init();
        Window = window ?? new Window();

        ResourceLoader = new ResourceLoader(this);

        Window.OnCreated();
    }

    public void Run()
    {
        Window.Show();
        Delta = ExpectedDelta;
        while (true)
        {
            long before = platform.GetUsecTimeline();
            // ---- start ticking the game
            Window.PollEvents();
            if (Window.IsInvalid) break;

            Window.Tick();
            ticks++;
            // ----
            long after = platform.GetUsecTimeline();

            long passed = after - before;
            double usecPerFrame = ExpectedDelta * 1000 * 1000;
            if (passed < usecPerFrame)
            {
                double ticksPerUsec = TimeSpan.TicksPerMillisecond / 1000d;
                double toSleepUsec = usecPerFrame - passed;
                double toSleep = ticksPerUsec * toSleepUsec;
                Thread.Sleep(TimeSpan.FromTicks((long)toSleep));
            }
            Delta = Math.Max(passed / 1000d / 1000d, ExpectedDelta);
        }
    }
}