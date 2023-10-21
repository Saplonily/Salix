namespace Monosand;

internal interface ITexture2DImpl : IDisposable
{
    int Width { get; }
    int Height { get; }

    unsafe abstract void SetData(int width, int height, void* data, ImageFormat format);
}