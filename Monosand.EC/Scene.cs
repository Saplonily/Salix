namespace Monosand.EC;

public class Scene
{
    private ECGame? game;
    private bool updating;
    private bool rendering;
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

    public void AddEntity(Entity entity)
    {
        if (updating)
        {
            toAdds.Add(entity);
        }
        else if (!rendering)
        {
            entities.Add(entity);
            entity.OnAdded(this);
        }
        else
        {
            throw new InvalidOperationException("Attempt to add entity while rendering.");
        }
    }

    public void RemoveEntity(Entity entity)
    {
        if (updating)
        {
            toRemoves.Add(entity);
        }
        else if (!rendering)
        {
            entities.Add(entity);
            entity.OnRemoved(this);
        }
        else
        {
            throw new InvalidOperationException("Attempt to remove entity while rendering.");
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

    public virtual void Update()
    {
        updating = true;
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
        updating = false;
    }

    public virtual void Render()
    {
        rendering = true;
        foreach (var entity in entities)
            entity.Render();
        rendering = false;
    }
}