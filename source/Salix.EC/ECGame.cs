namespace Saladim.Salix.EC;

public class ECGame : Game
{
    private static ECGame? current;

    private Scene? scene;
    private Scene? nextScene;
    private readonly SpriteBatch spriteBatch;

    public SpriteBatch SpriteBatch => spriteBatch;
    public bool IsNoSceneRunning => scene is null;

    /// <summary>Current scene of the game. Use <see cref="IsNoSceneRunning"/> to check if it's <see langword="null"/>.</summary>
    public Scene Scene => scene!;

    public Scene? NextScene
    {
        get => nextScene;
        set => nextScene = value;
    }

    public static ECGame Current => current!;

    public ECGame()
    {
        current = this;
        spriteBatch = new(this);
    }

    public void GotoScene(Scene scene)
    {
        ThrowHelper.ThrowIfNull(scene);
        nextScene = scene;
    }

    public override void Update()
    {
        if (nextScene is not null)
        {
            var from = scene;
            var to = nextScene;
            from?.SceneEnd(this, nextScene);
            scene = nextScene;
            nextScene = null;
            to.SceneBegin(this, from);
        }
        scene?.UpdateInternal();
    }

    public override void Render()
    {
        scene?.RenderInternal();
    }
}