namespace Monosand.EC;

public class ECGame : Game
{
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

    public ECGame()
    {
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
            scene = nextScene;
            nextScene = null;
        }
        scene?.Update();
    }

    public override void Render()
    {
        scene?.Render();
    }
}