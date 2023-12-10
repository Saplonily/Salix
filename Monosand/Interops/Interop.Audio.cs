using System.Runtime.InteropServices;

namespace Monosand;

internal unsafe partial class Interop
{
    [DllImport(DllPath)] public static extern IntPtr MsdaCreateAudioContext();

    [DllImport(DllPath)] public static extern void MsdaSetCurrentAudioContext(IntPtr context);

    [DllImport(DllPath)] public static extern IntPtr MsdaCreateSound(AudioFormat fmt, void* audioData, long framesCount);

    [DllImport(DllPath)] public static extern IntPtr MsdaCreateSoundInstance(IntPtr soundHandle);

    [DllImport(DllPath)] public static extern void MsdaPlaySoundInstance(IntPtr soundInstanceHandle);
}
