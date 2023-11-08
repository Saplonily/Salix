namespace Monosand;

public interface IGraphicsImpl : IDisposable
{
    RenderContext RenderContext { get; }
}