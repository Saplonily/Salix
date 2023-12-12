using System.Runtime.InteropServices;

namespace Monosand;


internal unsafe partial class Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RECT { public int left, top, right, bottom; }

    // on windows, it returns a pointer point to { HWND, HDC }
    [DllImport(DllPath)] public static extern IntPtr MsdCreateWindow(int width, int height, char* title, IntPtr gcHandle);

    // when the graphics backend is opengl33, it returns HGLRC
    [DllImport(DllPath)] public static extern IntPtr MsdCreateRenderContext();

    [DllImport(DllPath)] public static extern IntPtr MsdAttachRenderContext(IntPtr winHandle, IntPtr hrc);

    [DllImport(DllPath)] public static extern void* MsdBeginPullEvents(IntPtr winHandle, out nint count, out int* events);

    [DllImport(DllPath)] public static extern void MsdEndPullEvents(IntPtr winHandle, void* ehandle);

    [DllImport(DllPath)] public static extern void MsdShowWindow(IntPtr winHandle);

    [DllImport(DllPath)] public static extern void MsdHideWindow(IntPtr winHandle);

    [DllImport(DllPath)] public static extern void MsdDestroyWindow(IntPtr winHandle);

    [DllImport(DllPath)] public static extern RECT MsdGetWindowRect(IntPtr winHandle);

    [DllImport(DllPath)] public static extern void MsdSetWindowSize(IntPtr winHandle, int width, int height);

    [DllImport(DllPath)] public static extern void MsdSetWindowPos(IntPtr winHandle, int x, int y);

    [DllImport(DllPath)] public static extern void MsdgSwapBuffers(IntPtr winHandle);

    [DllImport(DllPath)] public static extern void MsdSetWindowTitle(IntPtr winHandle, char* title);

    [DllImport(DllPath)] public static extern void MsdGetWindowTitle(IntPtr winHandle, char* title);
}
