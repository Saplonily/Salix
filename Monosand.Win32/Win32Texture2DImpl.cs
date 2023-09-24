namespace Monosand.Win32;

internal sealed class Win32Texture2DImpl : Texture2DImpl
{
    private IntPtr winHandle;
    private IntPtr texHandle;
    internal int width, height;

    internal Win32Texture2DImpl(WinImpl winImpl, int width, int height)
    {
        (this.width, this.height) = (width, height);
        winHandle = ((Win32WinImpl)winImpl).Handle;
        texHandle = Interop.MsdgCreateTexture(winHandle, width, height);
    }

    internal override unsafe void SetData(int width, int height, void* data)
    {
        (this.width, this.height) = (width, height);
        Interop.MsdgSetTextureData(winHandle, texHandle, width, height, data);
    }

    internal override void Dispose()
    {
        winHandle = IntPtr.Zero;
        texHandle = IntPtr.Zero;
    }
}