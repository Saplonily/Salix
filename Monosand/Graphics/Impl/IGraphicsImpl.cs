namespace Monosand;

public interface IGraphicsImpl : IDisposable
{
    bool IsDisposed { get; }

    RenderContext RenderContext { get; }
}