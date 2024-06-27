using System.Runtime.InteropServices;

#pragma warning disable SYSLIB1054

namespace Salix;

unsafe partial class Interop
{
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_Initialize();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern long SLX_GetUsecTimeline();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateRenderContext();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_AttachRenderContext(IntPtr win, IntPtr hrc);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_SwapBuffers(IntPtr win);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_SetVSyncEnabled(NBool enable);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern double SLX_GetVSyncFrameTime();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_Viewport(int x, int y, int width, int height);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_Clear(float r, float g, float b, float a);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_RegisterVertexType(VertexElementType* vdecl, int len);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateVertexBuffer(IntPtr vertexType, NBool indexed);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteVertexBuffer(IntPtr bufferHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DrawPrimitives(IntPtr vertexType, PrimitiveType ptype, void* data, int dataSize, int verticesCount);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetVertexBufferData(IntPtr vertexBuffer, void* data, int dataSize, VertexBufferDataUsage dataUsage);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetIndexBufferData(IntPtr vertexBuffer, void* data, int dataSize, VertexBufferDataUsage dataUsage);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DrawBufferPrimitives(IntPtr bufferHandle, PrimitiveType primitiveType, int verticesCount);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DrawIndexedBufferPrimitives(IntPtr bufferHandle, PrimitiveType primitiveType, int verticesCount);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateTexture(int width, int height);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteTexture(IntPtr texHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetTextureData(IntPtr texHandle, int width, int height, void* data, ImageFormat format);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetTexture(int index, IntPtr texHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetTextureFilter(IntPtr texHandle, TextureFilterType min, TextureFilterType max);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetTextureWrap(IntPtr texHandle, TextureWrapType wrap);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateSampler(TextureFilterType filter, TextureWrapType wrap);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteSampler(IntPtr samplerHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetSampler(int index, IntPtr samplerHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateShaderFromGlsl(byte* vertSource, byte* fragSource);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteShader(IntPtr shaderHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShader(IntPtr shaderHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateRenderTarget(IntPtr texHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteRenderTarget(IntPtr renderTargetHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetRenderTarget(IntPtr renderTargetHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern int SLX_GetShaderParamLocation(IntPtr shaderHandle, byte* nameUtf8);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern int SLX_GetShaderParamLocation(IntPtr shaderHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
#endif
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamInt(int location, int value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamFloat(int location, float value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamVec4(int location, float* value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamMat4(int location, float* value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamMat3x2(int location, float* value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateWindow(int width, int height, char* title, IntPtr gcHandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_DestroyWindow(IntPtr win);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_PollEvents(IntPtr win);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void* SLX_BeginProcessEvents(IntPtr win, out nint count, out void* events);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_EndProcessEvents(IntPtr win, void* ehandle);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_ShowWindow(IntPtr win);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_HideWindow(IntPtr win);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_GetWindowRect(IntPtr win, out RECT rect);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_SetWindowSize(IntPtr win, int width, int height);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_SetWindowPos(IntPtr win, int x, int y);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_SetWindowTitle(IntPtr win, char* title);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern int SLX_GetWindowTitle(IntPtr win, char* title);
#if NET5_0_OR_GREATER
	[SuppressGCTransition]
#endif
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern ErrorCode SLX_GetError();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void* SLX_LoadImage(void* memory, int length, out int width, out int height, out int dataLength, out ImageFormat textureFormat);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_FreeImage(void* texData);
}
