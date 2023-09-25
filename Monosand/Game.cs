namespace Monosand;

public class Game
{
    public const int DefaultWindowWidth = 1280;
    public const int DefaultWindowHeight = 720;

    private static Game? instance;
    public static Game Instance
    {
        get => instance ?? throw SR.GameNotNewed;
        set => instance = instance is null ? value : throw SR.GameHasBeenNewed;
    }
    internal static Platform Platform => Instance.platform;
    internal static WinImpl WinImpl => Instance.Window.WinImpl;

    private Window window;
    internal Platform platform;

    public ResourceLoader ResourceLoader { get; }

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
        if (platform is null)
            throw new ArgumentNullException(nameof(platform));
        Instance = this;
        this.platform = platform;
        platform.Init();
        this.window = null!;
        Window = window ?? new Window(this);

        ResourceLoader = new ResourceLoader(this);

        Window.OnCreated();
    }

    public void Run()
    {
        Window.Show();
        while (true)
        {
            Window.PollEvents();
            if (Window.IsInvalid) break;

            Window.Update();
            Window.RenderInternal();
            Thread.Sleep(10);
        }
    }
}