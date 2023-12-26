﻿namespace Monosand;

#pragma warning disable CA1822 // Mark members as static

// TODO maybe we can just remove this class
public unsafe class Platform
{
    private GraphicsBackend graphicsBackend;
    private MonosandPlatform identifier;

    /// <summary>The graphics backend currently using.</summary>
    public GraphicsBackend GraphicsBackend => graphicsBackend;

    /// <summary>Current platform.</summary>
    public MonosandPlatform Identifier => identifier;

    public Platform() { }

    internal void Initialize()
    {
        // TODO error handle
        if (Interop.MsdInitialize() != 0)
            throw new OperationFailedException("MsdInitialize returned non-zero value.");
        graphicsBackend = Interop.MsdgGetGraphicsBackend();
        identifier = MonosandPlatform.Win32;
    }

    internal Stream OpenReadStream(string fileName)
        => new FileStream(fileName, FileMode.Open, FileAccess.Read);

    // TODO remove these resource loading methods
    internal unsafe UnmanagedMemory LoadImage(ReadOnlySpan<byte> source, out int width, out int height, out ImageFormat format)
    {
        void* data;
        fixed (void* ptr = source)
        {
            data = Interop.MsdLoadImage(ptr, source.Length, out width, out height, out int size, out format);
            if (data is null)
                throw new ResourceLoadFailedException(ResourceType.Image);
            return new(data, size);
        }
    }

    internal void FreeImage(UnmanagedMemory imageData)
    {
        ThrowHelper.ThrowIfArgInvalid(imageData.IsEmpty, nameof(imageData));
        Interop.MsdFreeImage(imageData.Pointer);
    }
}