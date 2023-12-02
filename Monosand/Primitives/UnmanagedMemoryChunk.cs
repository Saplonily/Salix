using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct UnmanagedMemoryChunk
{
    public byte* Pointer;
    public nint Size;

    public static UnmanagedMemoryChunk Empty => new(null, 0);
    public readonly bool IsEmpty => Pointer == null;

    public UnmanagedMemoryChunk(void* pointer, nint size)
        : this((byte*)pointer, size)
    { }

    public UnmanagedMemoryChunk(byte* pointer, nint size)
    {
        Pointer = pointer;
        Size = size;
    }
}