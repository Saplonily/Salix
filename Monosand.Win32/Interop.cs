﻿using System.Runtime.InteropServices;

namespace Monosand.Win32;

#pragma warning disable SYSLIB1054

internal unsafe class Interop
{
    private const string DllPath = "Monosand.Win32.Native.dll";

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RECT { public int left, top, right, bottom; }

    [DllImport(DllPath)] public static extern IntPtr MsdCreateWindow(int width, int height, char* title, IntPtr gcHandle);
    [DllImport(DllPath)] public static extern int MsdInit();

    // TODO, for some tfms(.net5-) don't support UnmanagedCallbackOnlyAttribute, use a sdl2-like event polling
    [DllImport(DllPath)] public static extern void MsdSetMsgCallback(int index, void* func);
    [DllImport(DllPath)] public static extern void MsdPollEvents(IntPtr handle);
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

    [DllImport(DllPath)] public static extern IntPtr MsdgCreateVertexBuffer(IntPtr handle, IntPtr vertexType);
    [DllImport(DllPath)] public static extern void MsdgDeleteVertexBuffer(IntPtr handle, IntPtr bufferHandle);

    [DllImport(DllPath)]
    public static extern void MsdgSetVertexBufferData(
        IntPtr handle, IntPtr vertexBuffer,
        void* data, int dataSize, VertexBufferDataUsage dataUsage
        );

    [DllImport(DllPath)]
    public static extern void MsdgDrawBufferPrimitives(
        IntPtr handle, IntPtr bufferHandle, PrimitiveType primitiveType,
        int verticesCount
        );

    [DllImport(DllPath)]
    public static extern void* MsdLoadImage(void* memory, int length, out int width, out int height, out int channels);
    [DllImport(DllPath)] public static extern void MsdFreeImage(void* texData);
    [DllImport(DllPath)] public static extern void MsdgSetTextureData(IntPtr handle, IntPtr texHandle, int width, int height, void* data);
    [DllImport(DllPath)] public static extern IntPtr MsdgCreateTexture(IntPtr handle, int width, int height);
    [DllImport(DllPath)] public static extern void MsdgSetTexture(IntPtr handle, int index, IntPtr texHandle);
    [DllImport(DllPath)] public static extern GraphicsBackend MsdgGetGraphicsBackend();
}