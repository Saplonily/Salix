using System.Runtime.InteropServices;

namespace Monosand.Win32;

#pragma warning disable SYSLIB1054

internal unsafe class Interop
{
    private const string DllPath = "Monosand.Win32.Native.dll";

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RECT { public int left, top, right, bottom; }

    [DllImport(DllPath)] public static extern IntPtr MsdCreateWindow(int width, int height, char* title, IntPtr gcHandle);
    [DllImport(DllPath)] public static extern int MsdInit();
    [DllImport(DllPath)] public static extern void* MsdBeginPollEvents(IntPtr handle, out nint count, out int* events);
    [DllImport(DllPath)] public static extern void MsdEndPollEvents(IntPtr handle, void* ehandle);
    [DllImport(DllPath)] public static extern void MsdShowWindow(IntPtr handle);
    [DllImport(DllPath)] public static extern void MsdHideWindow(IntPtr handle);
    [DllImport(DllPath)] public static extern void MsdDestroyWindow(IntPtr handle);
    [DllImport(DllPath)] public static extern RECT MsdGetWindowRect(IntPtr handle);
    [DllImport(DllPath)] public static extern void MsdSetWindowSize(IntPtr handle, int width, int height);
    [DllImport(DllPath)] public static extern void MsdSetWindowPos(IntPtr handle, int x, int y);


    [DllImport(DllPath)] public static extern void MsdgSwapBuffers(IntPtr handle);
    [DllImport(DllPath)] public static extern void MsdgViewport(IntPtr handle, int x, int y, int width, int height);
    [DllImport(DllPath)] public static extern void MsdgClear(IntPtr handle, Color color);

    [DllImport(DllPath)] public static extern IntPtr MsdgRegisterVertexType(IntPtr handle, VertexElementType* vdecl, int len);

    [DllImport(DllPath)]
    public static extern void MsdgDrawPrimitives(
        IntPtr handle,
        IntPtr vertexType,
        PrimitiveType ptype,
        void* data, int dataSize, int verticesCount
        );

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateVertexBuffer(IntPtr handle, IntPtr vertexType, byte indexed);
    [DllImport(DllPath)] public static extern void MsdgDeleteVertexBuffer(IntPtr handle, IntPtr bufferHandle);

    [DllImport(DllPath)]
    public static extern void MsdgSetVertexBufferData(
        IntPtr handle, IntPtr vertexBuffer,
        void* data, int dataSize, VertexBufferDataUsage dataUsage
        );

    [DllImport(DllPath)]
    public static extern void MsdgSetIndexBufferData(
        IntPtr handle, IntPtr vertexBuffer,
        void* data, int dataSize, VertexBufferDataUsage dataUsage
    );

    [DllImport(DllPath)]
    public static extern void MsdgDrawBufferPrimitives(
        IntPtr handle, IntPtr bufferHandle, PrimitiveType primitiveType,
        int verticesCount
        );

    [DllImport(DllPath)]
    public static extern void MsdgDrawIndexedBufferPrimitives(
        IntPtr handle, IntPtr bufferHandle, PrimitiveType primitiveType,
        int verticesCount
    );

    [DllImport(DllPath)]
    public static extern void* MsdLoadImage(
        void* memory, int length, out int width,
        out int height, out int dataLength,
        out ImageFormat textureFormat
        );
    [DllImport(DllPath)] public static extern void MsdFreeImage(void* texData);
    [DllImport(DllPath)]
    public static extern void MsdgSetTextureData(
        IntPtr handle, IntPtr texHandle,
        int width, int height, void* data,
        ImageFormat format
        );

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateTexture(IntPtr handle, int width, int height);
    [DllImport(DllPath)] public static extern void MsdgDeleteTexture(IntPtr handle, IntPtr texHandle);
    [DllImport(DllPath)] public static extern void MsdgSetTexture(IntPtr handle, int index, IntPtr texHandle);
    [DllImport(DllPath)] public static extern GraphicsBackend MsdgGetGraphicsBackend();
    [DllImport(DllPath)] public static extern IntPtr MsdgCreateShaderFromGlsl(IntPtr handle, byte* vertSource, byte* fragSource);
    [DllImport(DllPath)] public static extern void MsdgSetShader(IntPtr handle, IntPtr shaderHandle);
    [DllImport(DllPath)] public static extern void MsdgDeleteShader(IntPtr handle, IntPtr shaderHandle);

    #region Shader Parameter

    [DllImport(DllPath)]
    public static extern int MsdgGetShaderParamLocation(IntPtr handle, IntPtr shaderHandle, byte* nameUtf8);

#if NETSTANDARD2_1_OR_GREATER
    [DllImport(DllPath)]
    public static extern int MsdgGetShaderParamLocation(IntPtr handle, IntPtr shaderHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
#endif

    [DllImport(DllPath)] public static extern void MsdgSetShaderParamInt(IntPtr handle, int location, int value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamFloat(IntPtr handle, int location, float value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamVec4(IntPtr handle, int location, float* value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamMat4(IntPtr handle, int location, float* value, bool transpose);

    #endregion

    [DllImport(DllPath)] public static extern long MsdGetUsecTimeline();
    [DllImport(DllPath)] public static extern void MsdgSetVSyncEnabled(IntPtr handle, byte enable);
    [DllImport(DllPath)] public static extern double MsdgGetVSyncFrameTime(IntPtr handle);
}