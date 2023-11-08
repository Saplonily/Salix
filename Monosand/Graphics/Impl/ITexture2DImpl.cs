namespace Monosand;

internal interface ITexture2DImpl : IGraphicsImpl
{
    unsafe abstract void SetData(int width, int height, void* data, ImageFormat format);
    void SetFilter(TextureFilterType filter);
    void SetWrap(TextureWrapType wrap);
}