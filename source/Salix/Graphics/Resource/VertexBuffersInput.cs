namespace Saladim.Salix;

public sealed class VertexBuffersInput : GraphicsResource
{
    private VertexBufferBinding[]? bindings;

    private IVertexBuffersInputImpl? impl;

    public ReadOnlySpan<VertexBufferBinding> Bindings => bindings.AsSpan();

    public VertexBuffersInput(RenderContext context, params VertexBufferBinding[] bindings)
        : this(context, bindings.AsSpan())
    {
    }

    public VertexBuffersInput(RenderContext context, params ReadOnlySpan<VertexBufferBinding> bindings)
        : base(context)
    {
        this.bindings = bindings.ToArray();
        impl = context.Impl.CreateVertexBuffersInputImpl(bindings);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        impl!.Dispose();
        bindings = null;
        impl = null;
    }

    // REMIND: check dispose when set
}