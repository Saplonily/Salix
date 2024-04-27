using System.Runtime.InteropServices;

namespace Monosand;

internal unsafe partial class Interop
{
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
    [DllImport(DllPath)] public static extern void MsdgSetTextureData(IntPtr texHandle, int width, int height, void* data, ImageFormat format);

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateTexture(int width, int height);

    [DllImport(DllPath)] public static extern void MsdgDeleteTexture(IntPtr texHandle);

    [DllImport(DllPath)] public static extern void MsdgSetTexture(int index, IntPtr texHandle);

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateSampler(TextureFilterType filter, TextureWrapType wrap);

    [DllImport(DllPath)] public static extern void MsdgSetSampler(int index, IntPtr samplerHandle);

    [DllImport(DllPath)] public static extern void MsdgDeleteSampler(IntPtr samplerHandle);

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

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateRenderTarget(IntPtr texHandle);

    [DllImport(DllPath)] public static extern void MsdgDeleteRenderTarget(IntPtr renderTargetHandle);

    [DllImport(DllPath)] public static extern void MsdgSetRenderTarget(IntPtr renderTargetHandle);

    [DllImport(DllPath)] public static extern void MsdgSetTextureFilter(IntPtr texHandle, TextureFilterType min, TextureFilterType max);

    [DllImport(DllPath)] public static extern void MsdgSetTextureWrap(IntPtr texHandle, TextureWrapType wrap);
}
