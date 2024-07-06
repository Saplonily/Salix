﻿namespace Saladim.Salix;

/// <summary>
/// A type of Buffer used to store vertices data
/// </summary>
/// <typeparam name="T">Vertex type, must be unmanged</typeparam>
public sealed class VertexBuffer<T> : GraphicsResource where T : unmanaged
{
    private readonly VertexDeclaration vertexDeclaration;
    private readonly bool indexed;
    private readonly VertexBufferDataUsage dataUsage;
    private IntPtr nativeHandle;
    private int verticesCount;
    private int indicesCount;

    public VertexDeclaration VertexDeclaration { get { EnsureState(); return vertexDeclaration; } }
    public bool Indexed { get { EnsureState(); return indexed; } }
    public int IndicesCount { get { EnsureState(); return indicesCount; } }
    public int VerticesCount { get { EnsureState(); return verticesCount; } }
    internal IntPtr NativeHandle { get { EnsureState(); return nativeHandle; } }

    public VertexBuffer(
        RenderContext context,
        VertexDeclaration vertexDeclaration,
        VertexBufferDataUsage dataUsage = VertexBufferDataUsage.StaticDraw,
        bool indexed = false
        ) : base(context)
    {
        ThrowHelper.ThrowIfNull(vertexDeclaration);

        this.dataUsage = dataUsage;
        this.indexed = indexed;
        this.vertexDeclaration = vertexDeclaration;
        nativeHandle = Interop.SLX_CreateVertexBuffer(context.SafeGetVertexType(vertexDeclaration), indexed);
        if (nativeHandle == IntPtr.Zero) Interop.Throw();
        indicesCount = -1;
        verticesCount = -1;
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    public unsafe void SetData(T[] array)
    {
        ThrowHelper.ThrowIfNull(array);
        SetData(array.AsSpan());
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    public unsafe void SetData(ReadOnlySpan<T> span)
    {
        ThrowHelper.ThrowIfInvalid(span.IsEmpty, SR.VerticesDataIsNull);
        fixed (T* data = span)
            SetData(data, span.Length);
    }

    /// <summary>Copy and set the data from a pointer <paramref name="data"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetData(T* data, int count)
    {
        EnsureState();
        ThrowHelper.ThrowIfInvalid(data is null, SR.VerticesDataIsNull);
        verticesCount = count;
        if (Interop.SLX_SetVertexBufferData(nativeHandle, data, sizeof(T) * count, dataUsage))
            Interop.Throw();
    }

    /// <summary>Copy and set the data from an <paramref name="array"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort[] array)
    {
        ThrowHelper.ThrowIfNull(array);
        SetIndexData(array.AsSpan());
    }

    /// <summary>Copy and set the data from a <paramref name="span"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ReadOnlySpan<ushort> span)
    {
        ThrowHelper.ThrowIfInvalid(span.IsEmpty, SR.VerticesDataIsNull);
        fixed (ushort* ptr = span)
            SetIndexData(ptr, span.Length);
    }

    /// <summary>Copy and set the data from a pointer <paramref name="ptr"/></summary>
    [CLSCompliant(false)]
    public unsafe void SetIndexData(ushort* data, int count)
    {
        EnsureState();
        ThrowHelper.ThrowIfInvalid(data is null, SR.VerticesDataIsNull);
        indicesCount = count;
        if (Interop.SLX_SetIndexBufferData(nativeHandle, data, sizeof(ushort) * count, dataUsage))
            Interop.Throw();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Interop.SLX_DeleteVertexBuffer(nativeHandle))
            Interop.Throw();
        nativeHandle = IntPtr.Zero;
    }
}