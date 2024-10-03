namespace Saladim.Salix;

public struct WindowConfig
{
    public const int DefaultWidth = 1280;
    public const int DefaultHeight = 720;
    public const string DefaultTitle = nameof(Salix);

    public int Width { get; set; }

    public int Height { get; set; }

    public string Title { get; set; }

    public WindowConfig() : this(DefaultWidth, DefaultHeight, DefaultTitle)
    {
    }

    public WindowConfig(int width, int height, string title)
    {
        Width = width;
        Height = height;
        Title = title;
    }

    internal readonly void Verify()
    {
        if (Width < 1) throw new ArgumentOutOfRangeException(nameof(Width), SR.InvalidWindowSize);
        if (Height < 1) throw new ArgumentOutOfRangeException(nameof(Height), SR.InvalidWindowSize);
        ThrowHelper.ThrowIfNull(Title);
    }
}