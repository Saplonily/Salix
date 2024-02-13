namespace Monosand.EC;

public class Scene
{
    private readonly List<Entity> entities;
    private readonly List<Entity> toAdds;
    private readonly List<Entity> toRemoves;

    public IReadOnlyList<Entity> Entities => entities;
    public IReadOnlyList<Entity> ToAddEntities => toAdds;
    public IReadOnlyList<Entity> ToRemoveEntities => toRemoves;

    public Scene()
    {
        entities = new(16);
        toAdds = new(4);
        toRemoves = new(4);
    }

    public void AddEntity(Entity entity)
    {
        toAdds.Add(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        toRemoves.Remove(entity);
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