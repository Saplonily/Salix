using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct UnmanagedMemory
{
    public byte* Pointer;
    public nint Size;
    public static UnmanagedMemory Empty => new(null, 0);
    public readonly bool IsEmpty => Pointer == null;
    public UnmanagedMemory(void* pointer, nint size)
        : this((byte*)pointer, size)
    { }
    public UnmanagedMemory(byte* pointer, nint size)
    {
        ThrowHelper.ThrowIfNull(pointer is null, nameof(pointer));
        ThrowHelper.ThrowIfArgInvalid(size <= 0, nameof(size));
        Pointer = pointer;
        Size = size;
    }
}