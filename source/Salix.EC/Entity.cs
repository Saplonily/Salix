namespace Saladim.Salix.EC;

public class Entity
{
    private Scene? scene;

    public bool InsideScene => scene is not null;

    /// <summary>
    /// Scene that this entity inside.
    /// Will be <see langword="null"/>, use <see cref="InsideScene"/> to check if this entity is inside a scene.
    /// </summary>
    public Scene Scene => scene!;

    public virtual void OnAdded(Scene scene)
    {
        this.scene = scene;
    }

    public virtual void OnRemoved(Scene scene)
    {
        this.scene = null;
    }

    public virtual void Awake(Scene scene)
    {

    }

    public virtual void Update()
    {

    }

    public virtual void Render()
    {

    }
}