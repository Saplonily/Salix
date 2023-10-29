namespace Monosand;

public abstract class Effect
{
    protected Shader shader;

    public Effect(Shader shader)
        => this.shader = shader;

    public virtual void Use()
        => shader.Use();
}