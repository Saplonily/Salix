namespace Saladim.Salix;

public sealed class IndexBuffer : GraphicsResource
{
    private IIndexBufferImpl? impl;
    internal IIndexBufferImpl Impl { get { EnsureState(); return impl!; } }

    private readonly int indicesCount;

    public int IndicesCount => indicesCount;

    public IndexBuffer(RenderContext renderContext, int indicesCount, BufferDataUsage dataUsage = BufferDataUsage.StaticDraw) 
        : base(renderContext)
    {
        impl = renderContext.Impl.CreateIndexBufferImpl(indicesCount, dataUsage);
        this.indicesCount = indicesCount;
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort[] array)
    {
        ThrowHelper.ThrowIfNull(array, SR.IndicesDataIsNull);
        fixed (ushort* ptr = array)
            SetIndexData(ptr, array.Length);
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ReadOnlySpan<ushort> span)
    {
        ThrowHelper.ThrowIfNull(span, SR.IndicesDataIsNull);
        fixed (ushort* ptr = span)
            SetIndexData(ptr, span.Length);
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort* data, int count)
    {
        EnsureState();
        ThrowHelper.ThrowIfNull(data, SR.IndicesDataIsNull);
        impl!.SetIndexData(data, 0, count * sizeof(ushort));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        impl?.Dispose();
        impl = null;
    }
}
