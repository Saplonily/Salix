namespace Monosand;

public class Shader
{
    private ShaderImpl? impl;

    internal Shader(ShaderImpl impl)
    {
        this.impl = impl;
    }

    public void SetParameter<T>(in ShaderParameter param, T value) where T : unmanaged
    {
        EnsureState();
        impl!.SetParameter(param.Location, value);
    }

    public ShaderParameter GetParameter(string name)
        => new(this, impl!.GetParameterLocation(name));

    public ShaderParameter GetParameter(ReadOnlySpan<byte> nameUtf8)
        => new(this, impl!.GetParameterLocation(nameUtf8));

    public void Use()
    {
        EnsureState();
        impl!.Use();
    }

    private void EnsureState()
    {
        ThrowHelper.ThrowIfDisposed(impl is null, this);
    }
}