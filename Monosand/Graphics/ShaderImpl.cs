namespace Monosand;

internal abstract class ShaderImpl
{
    internal abstract int GetParameterLocation(string name);
    internal abstract int GetParameterLocation(ReadOnlySpan<byte> nameUtf8);

    internal abstract void SetParameter<T>(int location, T value) where T : unmanaged;
    internal abstract void Use();
}