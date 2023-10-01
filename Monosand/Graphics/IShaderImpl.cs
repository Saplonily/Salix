namespace Monosand;

internal interface IShaderImpl : IDisposable
{
    int GetParameterLocation(string name);
    int GetParameterLocation(ReadOnlySpan<byte> nameUtf8);

    void SetParameter<T>(int location, T value) where T : unmanaged;
}