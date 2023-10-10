namespace Monosand;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    private static Game? instance;
    private Window window;
    private Platform platform;
    private long ticks;

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
    public double ExpectedDelta { get; set; }
    public double ExpectedFps { get => 1.0 / ExpectedDelta; set => ExpectedDelta = 1.0 / value; }

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
        ExpectedDelta = 1 / 60d;
        ticks = 0;
        platform.Init();
        Window = window ?? new Window();

        ResourceLoader = new ResourceLoader(this);

        Window.OnCreated();
    }

    public void Run()
    {
        Window.Show();
        while (true)
        {
            long before = platform.GetUsecTimeline();
            //
            Window.PollEvents();
            if (Window.IsInvalid) break;

            Window.Tick();
            ticks++;
            //
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
        }
    }
}