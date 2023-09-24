namespace Monosand;

internal abstract class Texture2DImpl
{
    internal abstract void Dispose();

    internal unsafe abstract void SetData(int width, int height, void* data);
}