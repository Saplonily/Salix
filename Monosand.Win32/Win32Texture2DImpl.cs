namespace Monosand.Win32;

internal sealed class Win32Texture2DImpl : Texture2DImpl
{
    private IntPtr winHandle;
    internal IntPtr texHandle;
    internal int width, height;

    internal Win32Texture2DImpl(Win32WinImpl winImpl, int width, int height)
    {
        (this.width, this.height) = (width, height);
        winHandle = winImpl.Handle;
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
        // TODO delete texture in opengl side
        throw new NotImplementedException();
    }
}