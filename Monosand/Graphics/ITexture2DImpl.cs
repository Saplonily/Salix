namespace Monosand;

internal interface ITexture2DImpl : IDisposable
{
    internal unsafe abstract void SetData(int width, int height, void* data);
}