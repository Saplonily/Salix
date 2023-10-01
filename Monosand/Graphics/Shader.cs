namespace Monosand;

#pragma warning disable CA1816

public class Shader : GraphicsResource
{
    private readonly IShaderImpl impl;
    private readonly RenderContext context;

    internal Shader(RenderContext context, IShaderImpl impl)
        => (this.impl, this.context) = (impl, context);

    public void SetParameter<T>(in ShaderParameter param, ref T value) where T : unmanaged
        => impl.SetParameter(param.Location, ref value);

    public ShaderParameter GetParameter(string name)
        => new(this, impl.GetParameterLocation(name));

    public ShaderParameter GetParameter(ReadOnlySpan<byte> nameUtf8)
        => new(this, impl.GetParameterLocation(nameUtf8));

    internal IShaderImpl GetImpl()
        => impl;

    // we trust that subclasses of 'Shader' won't have a finalizer,
    // if it does, it's still easy to reimplement the Dispose method.
    public override void Dispose()
        => impl.Dispose();

    public void Use()
        => context.SetShader(this);
}