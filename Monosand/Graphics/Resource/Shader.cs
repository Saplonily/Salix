namespace Monosand;

public sealed class Shader : GraphicsResource
{
    internal IShaderImpl Impl { get; private set; }

    internal Shader(RenderContext context, IShaderImpl impl)
        : base(context)
        => Impl = impl;

    public void SetParameter<T>(in ShaderParameter param, ref T value) where T : unmanaged
        => Impl.SetParameter(param.Location, ref value);

    public ShaderParameter GetParameter(string name)
        => new(this, Impl.GetParameterLocation(name));

    public ShaderParameter GetParameter(ReadOnlySpan<byte> nameUtf8)
        => new(this, Impl.GetParameterLocation(nameUtf8));

    public override void Dispose()
        => Impl.Dispose();

    public void Use()
        => RenderContext.Shader = this;
}