using System.Runtime.InteropServices;

namespace Monosand;

#pragma warning disable SYSLIB1054

internal unsafe class Interop
{
    private const string DllPath = "libmsd";

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RECT { public int left, top, right, bottom; }

    [DllImport(DllPath)] public static extern int MsdInitialize();
    // on windows, it returns a pointer point to { HWND, HDC }
    [DllImport(DllPath)] public static extern IntPtr MsdCreateWindow(int width, int height, char* title, IntPtr gcHandle);
    // when the graphics backend is opengl33, it returns HGLRC
    [DllImport(DllPath)] public static extern IntPtr MsdCreateRenderContext();
    [DllImport(DllPath)] public static extern IntPtr MsdAttachRenderContext(IntPtr winHandle, IntPtr hrc);
    [DllImport(DllPath)] public static extern void* MsdBeginPollEvents(IntPtr winHandle, out nint count, out int* events);
    [DllImport(DllPath)] public static extern void MsdEndPollEvents(IntPtr winHandle, void* ehandle);
    [DllImport(DllPath)] public static extern void MsdShowWindow(IntPtr winHandle);
    [DllImport(DllPath)] public static extern void MsdHideWindow(IntPtr winHandle);
    [DllImport(DllPath)] public static extern void MsdDestroyWindow(IntPtr winHandle);
    [DllImport(DllPath)] public static extern RECT MsdGetWindowRect(IntPtr winHandle);
    [DllImport(DllPath)] public static extern void MsdSetWindowSize(IntPtr winHandle, int width, int height);
    [DllImport(DllPath)] public static extern void MsdSetWindowPos(IntPtr winHandle, int x, int y);


    [DllImport(DllPath)] public static extern void MsdgSwapBuffers(IntPtr winHandle);
    [DllImport(DllPath)] public static extern void MsdgViewport(int x, int y, int width, int height);
    [DllImport(DllPath)] public static extern void MsdgClear(in Color color);

    [DllImport(DllPath)] public static extern IntPtr MsdgRegisterVertexType(VertexElementType* vdecl, int len);

    [DllImport(DllPath)]
    public static extern void MsdgDrawPrimitives(
        IntPtr vertexType,
        PrimitiveType ptype,
        void* data, int dataSize,
        int verticesCount
        );

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateVertexBuffer(IntPtr vertexType, byte indexed);
    [DllImport(DllPath)] public static extern void MsdgDeleteVertexBuffer(IntPtr bufferHandle);

    [DllImport(DllPath)]
    public static extern void MsdgSetVertexBufferData(
        IntPtr vertexBuffer,
        void* data, int dataSize, VertexBufferDataUsage dataUsage
        );

    [DllImport(DllPath)]
    public static extern void MsdgSetIndexBufferData(
        IntPtr vertexBuffer,
        void* data, int dataSize, VertexBufferDataUsage dataUsage
    );

    [DllImport(DllPath)]
    public static extern void MsdgDrawBufferPrimitives(
        IntPtr bufferHandle, PrimitiveType primitiveType,
        int verticesCount
        );

    [DllImport(DllPath)]
    public static extern void MsdgDrawIndexedBufferPrimitives(
        IntPtr bufferHandle, PrimitiveType primitiveType,
        int verticesCount
    );

    // TODO `length` is `int` here because stb_image require it
    [DllImport(DllPath)]
    public static extern void* MsdLoadImage(
        void* memory, int length, out int width,
        out int height, out int dataLength,
        out ImageFormat textureFormat
        );

    [DllImport(DllPath)] public static extern void MsdFreeImage(void* texData);
    [DllImport(DllPath)] public static extern void MsdgSetTextureData(IntPtr texHandle, int width, int height, void* data, ImageFormat format);
    [DllImport(DllPath)] public static extern IntPtr MsdgCreateTexture(int width, int height);
    [DllImport(DllPath)] public static extern void MsdgDeleteTexture(IntPtr texHandle);
    [DllImport(DllPath)] public static extern void MsdgSetTexture(int index, IntPtr texHandle);
    [DllImport(DllPath)] public static extern GraphicsBackend MsdgGetGraphicsBackend();
    [DllImport(DllPath)] public static extern IntPtr MsdgCreateShaderFromGlsl(byte* vertSource, byte* fragSource);
    [DllImport(DllPath)] public static extern void MsdgSetShader(IntPtr shaderHandle);
    [DllImport(DllPath)] public static extern void MsdgDeleteShader(IntPtr shaderHandle);

    #region Shader Parameter

    [DllImport(DllPath)]
    public static extern int MsdgGetShaderParamLocation(IntPtr shaderHandle, byte* nameUtf8);

#if NETSTANDARD2_1_OR_GREATER
    [DllImport(DllPath)]
    public static extern int MsdgGetShaderParamLocation(IntPtr shaderHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
#endif

    [DllImport(DllPath)] public static extern void MsdgSetShaderParamInt(int location, int value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamFloat(int location, float value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamVec4(int location, float* value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamMat4(int location, float* value);
    [DllImport(DllPath)] public static extern void MsdgSetShaderParamMat3x2(int location, float* value);

    #endregion

    [DllImport(DllPath)] public static extern long MsdGetUsecTimeline();
    [DllImport(DllPath)] public static extern void MsdgSetVSyncEnabled(byte enable);
    [DllImport(DllPath)] public static extern double MsdgGetVSyncFrameTime();
    [DllImport(DllPath)] public static extern void MsdSetWindowTitle(IntPtr winHandle, char* title);
    [DllImport(DllPath)] public static extern void MsdGetWindowTitle(IntPtr winHandle, char* title);

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateRenderTarget(IntPtr texHandle);
    [DllImport(DllPath)] public static extern void MsdgDeleteRenderTarget(IntPtr renderTargetHandle);
    [DllImport(DllPath)] public static extern void MsdgSetRenderTarget(IntPtr renderTargetHandle);
    [DllImport(DllPath)] public static extern void MsdgSetTextureFilter(IntPtr texHandle, TextureFilterType min, TextureFilterType max);
    [DllImport(DllPath)] public static extern void MsdgSetTextureWrap(IntPtr texHandle, TextureWrapType wrap);
    [DllImport(DllPath)] public static extern void* MsdLoadAudio(void* memory, nint length, out int frames, out AudioFormat format);
    [DllImport(DllPath)] public static extern void MsdFreeAudio(void* memory);
}