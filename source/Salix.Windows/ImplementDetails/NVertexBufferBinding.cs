using System.Runtime.InteropServices;

namespace Saladim.Salix.Windows;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NVertexBufferBinding
{
    public IntPtr VertexBuffer;
    public int ElementsCount;
    public VertexElementType* Types;
    public int Offset;
    public int InstanceFrequency;
}