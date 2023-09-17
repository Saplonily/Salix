using System.Runtime.InteropServices;

namespace Monosand.Win32;

#pragma warning disable SYSLIB1054

internal unsafe class Interop
{
    private const string Dll = "Monosand.Win32.Native";

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RECT { public int left, top, right, bottom; }

    [DllImport(Dll)] public static extern IntPtr MsdCreateWindow(int width, int height, char* title, IntPtr gcHandle);
    [DllImport(Dll)] public static extern int MsdInit();
    [DllImport(Dll)] public static extern void MsdSetMsgCallback(int index, void* func);
    [DllImport(Dll)] public static extern void MsdPollEvents(IntPtr handle);
    [DllImport(Dll)] public static extern void MsdShowWindow(IntPtr handle);
    [DllImport(Dll)] public static extern void MsdHideWindow(IntPtr handle);
    [DllImport(Dll)] public static extern void MsdDestroyWindow(IntPtr handle);
    [DllImport(Dll)] public static extern RECT MsdGetWindowRect(IntPtr handle);
    [DllImport(Dll)] public static extern void MsdSetWindowSize(IntPtr handle, int width, int height);
    [DllImport(Dll)] public static extern void MsdSetWindowPos(IntPtr handle, int x, int y);


    [DllImport(Dll)] public static extern void MsdgSwapBuffers(IntPtr handle);
    [DllImport(Dll)] public static extern void MsdgViewport(IntPtr handle, int x, int y, int width, int height);
    [DllImport(Dll)] public static extern void MsdgClear(IntPtr handle, Color color);

    [DllImport(Dll)] public static extern uint MsdgRegisterVertexType(IntPtr handle, VertexElementType* vdecl, int len);
    [DllImport(Dll)]
    public static extern void MsdgDrawPrimitives(
        IntPtr handle,
        uint vertexType,
        PrimitiveType ptype,
        void* data, int dataSize, int verticesToDraw
        );
}