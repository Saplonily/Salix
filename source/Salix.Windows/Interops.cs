using System.Runtime.InteropServices;

#pragma warning disable SYSLIB1054

namespace Saladim.Salix.Windows;

unsafe partial class Interop
{
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_Initialize();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateWindow(int width, int height, char* title);
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
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateRenderContext(IntPtr win);
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
	internal static extern IntPtr SLX_CreateVertexBuffersInput(int count, NVertexBufferBinding* bindings);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateSingleVertexBufferInput(int elementsCount, VertexElementType* type);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_ReplaceInputBuffer(IntPtr input, int index, IntPtr buffer, int offset, int stride);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateVertexBuffer(int size);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteVertexBuffer(IntPtr vertexBuffer);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetVertexBufferData(IntPtr vertexBuffer, void* data, int offset, int dataSize);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateIndexBuffer(int size);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteIndexBuffer(IntPtr indexBuffer);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetIndexBufferData(IntPtr indexBuffer, ushort* data, int offset, int dataSize);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DrawPrimitives(IntPtr input, PrimitiveType primitiveType, int verticesCount);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DrawIndexedPrimitives(IntPtr input, IntPtr indexBuffer, PrimitiveType primitiveType, int indicesCount);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateTexture(int width, int height);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteTexture(IntPtr texture);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetTextureData(IntPtr texture, int width, int height, void* data, ImageFormat format);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetTexture(int index, IntPtr texture);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateShaderFromGlsl(byte* vertSource, byte* fragSource);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteShader(IntPtr shader);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShader(IntPtr shader);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern int SLX_GetShaderParamLocation(IntPtr shader, byte* nameUtf8);
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern int SLX_GetShaderParamLocation(IntPtr shader, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
#endif
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamInt(IntPtr shader, int location, int value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamFloat(IntPtr shader, int location, float value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamVec4(IntPtr shader, int location, float* value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamMat4(IntPtr shader, int location, float* value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetShaderParamMat3x2(IntPtr shader, int location, float* value);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateSampler(TextureFilterType filter, TextureWrapType wrap);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteSampler(IntPtr sampler);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetSampler(int index, IntPtr sampler);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern IntPtr SLX_CreateRenderTarget(IntPtr texture);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_DeleteRenderTarget(IntPtr renderTarget);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern NBool SLX_SetRenderTarget(IntPtr renderTarget);
#if NET5_0_OR_GREATER
	[SuppressGCTransition]
#endif
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern ErrorCode SLX_GetError();
#if NET5_0_OR_GREATER
	[SuppressGCTransition]
#endif
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern int SLX_GetPlatformError();
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void* SLX_LoadImage(void* memory, int length, out int width, out int height, out int dataLength, out ImageFormat textureFormat);
	[DllImport(LibName, CallingConvention = CallConv, ExactSpelling = true)]
	internal static extern void SLX_FreeImage(void* texData);
}
