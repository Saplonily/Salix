using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
public struct TriangleProperty<TProperty> where TProperty : unmanaged
{
    public TProperty First;
    public TProperty Second;
    public TProperty Third;

    public TriangleProperty(in TProperty first, in TProperty second, in TProperty third)
        => (First, Second, Third) = (first, second, third);

    public TriangleProperty(in TProperty value)
        => First = Second = Third = value;

    public static implicit operator TriangleProperty<TProperty>(in TProperty property)
        => new(property);
}