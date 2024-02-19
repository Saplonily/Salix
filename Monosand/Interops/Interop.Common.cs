using System.Runtime.InteropServices;

namespace Monosand;

#pragma warning disable SYSLIB1054

internal unsafe partial class Interop
{
    private const string DllPath = "msd";

    [DllImport(DllPath)] public static extern int MsdInitialize();

    [DllImport(DllPath)] public static extern GraphicsBackend MsdgGetGraphicsBackend();

    [DllImport(DllPath)] public static extern long MsdGetUsecTimeline();

    [DllImport(DllPath)] public static extern void MsdgSetVSyncEnabled(byte enable);

    [DllImport(DllPath)] public static extern double MsdgGetVSyncFrameTime();
}