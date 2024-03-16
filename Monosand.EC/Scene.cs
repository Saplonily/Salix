namespace Monosand.EC;

public class Scene
{
    enum SceneState : byte { Idle, Update, Render }

    private SceneState state;
    private ECGame? game;
    private readonly List<Entity> entities;
    private readonly List<Entity> toAdds;
    private readonly List<Entity> toRemoves;

    public IReadOnlyList<Entity> Entities => entities;
    public IReadOnlyList<Entity> ToAddEntities => toAdds;
    public IReadOnlyList<Entity> ToRemoveEntities => toRemoves;

    public ECGame Game => game!;
    public bool RunningOnGame => game is not null;

    public Scene()
    {
        entities = new(16);
        toAdds = new(4);
        toRemoves = new(4);
    }

    public Scene(IEnumerable<Entity> entities)
        : this()
    {
        this.entities.AddRange(entities);
    }

    public void AddEntity(Entity entity)
    {
        if (state is SceneState.Update)
        {
            toAdds.Add(entity);
        }
        else if (state is not SceneState.Render)
        {
            entities.Add(entity);
            entity.OnAdded(this);
        }
        else
        {
            throw new InvalidOperationException(SR.AddEntityWhileRendering);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (state is SceneState.Update)
        {
            toRemoves.Add(entity);
        }
        else if (state is not SceneState.Render)
        {
            entities.Add(entity);
            entity.OnRemoved(this);
        }
        else
        {
            throw new InvalidOperationException(SR.RemoveEntityWhileRendering);
        }
    }

    public virtual void SceneBegin(ECGame game, Scene? previousScene)
    {
        this.game = game;
    }

    public virtual void SceneEnd(ECGame game, Scene toScene)
    {
        this.game = null;
    }

    internal void UpdateInternal()
    {
        state = SceneState.Update;
        Update();
        state = SceneState.Idle;
    }

    internal void RenderInternal()
    {
        state = SceneState.Render;
        Render();
        state = SceneState.Idle;
    }

    public virtual void Update()
    {
        foreach (var entity in entities)
            entity.Update();
        if (toRemoves.Count != 0)
        {
            entities.RemoveAll(toRemoves.Contains);
            foreach (var e in toRemoves)
                e.OnRemoved(this);
            toRemoves.Clear();
        }
        if (toAdds.Count != 0)
        {
            foreach (var e in toAdds)
            {
                entities.Add(e);
                e.OnAdded(this);
            }
            foreach (var e in toAdds)
                e.Awake(this);
            toAdds.Clear();
        }
    }

    public virtual void Render()
    {
        foreach (var entity in entities)
            entity.Render();
    }
}