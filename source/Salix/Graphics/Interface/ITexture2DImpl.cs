namespace Saladim.Salix;

internal interface ITexture2DImpl : IDisposable
{
    int Width { get; }

    int Height { get; }

    unsafe void SetData(void* data, ImageFormat format);
}
