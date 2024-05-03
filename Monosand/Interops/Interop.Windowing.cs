using System.Runtime.InteropServices;

namespace Monosand;


internal unsafe partial class Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    internal struct RECT { public int left, top, right, bottom; }

    [DllImport(DllPath)] public static extern MResult<IntPtr> MsdCreateWindow(int width, int height, char* title, IntPtr gcHandle);

    [DllImport(DllPath)] public static extern MResult<IntPtr> MsdCreateRenderContext();

    [DllImport(DllPath)] public static extern MResult MsdAttachRenderContext(IntPtr win, IntPtr hrc);

    [DllImport(DllPath)] public static extern void MsdPollEvents(IntPtr win);

    [DllImport(DllPath)] public static extern void* MsdBeginProcessEvents(IntPtr win, out nint count, out int* events);

    [DllImport(DllPath)] public static extern void MsdEndProcessEvents(IntPtr win, void* ehandle);

    [DllImport(DllPath)] public static extern void MsdShowWindow(IntPtr win);

    [DllImport(DllPath)] public static extern void MsdHideWindow(IntPtr win);

    [DllImport(DllPath)] public static extern void MsdDestroyWindow(IntPtr win);

    [DllImport(DllPath)] public static extern RECT MsdGetWindowRect(IntPtr win);

    [DllImport(DllPath)] public static extern void MsdSetWindowSize(IntPtr win, int width, int height);

    [DllImport(DllPath)] public static extern void MsdSetWindowPos(IntPtr win, int x, int y);

    [DllImport(DllPath)] public static extern void MsdgSwapBuffers(IntPtr win);

    [DllImport(DllPath)] public static extern void MsdSetWindowTitle(IntPtr win, char* title);

    [DllImport(DllPath)] public static extern int MsdGetWindowTitle(IntPtr win, char* title);
}
