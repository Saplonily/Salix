using System.Runtime.InteropServices;

namespace Monosand;


internal unsafe partial class Interop
{
    // TODO `length` is `int` here because stb_image require it
    [DllImport(DllPath)]
    public static extern void* MsdLoadImage(
        void* memory, int length, out int width,
        out int height, out int dataLength,
        out ImageFormat textureFormat
        );

    [DllImport(DllPath)] public static extern void MsdFreeImage(void* texData);

    [DllImport(DllPath)]
    public static extern void* MsdLoadAudio(
        void* memory, nint dataSize,
        out nint loadedDataSize, out long frames, out AudioFormat format
        );

    [DllImport(DllPath)] public static extern void MsdFreeAudio(void* memory);
}
