using System.Numerics;

namespace Saladim.Salix;

public class SpriteFont
{
    public struct CharEntry
    {
        public short X;
        public short Y;
        public short Width;
        public short Height;
        public short BearingX;
        public short BearingY;
        public short Advance;
    }

    // TODO custom line height
    public const float LineHeightMultiplier = 1.2f;

    public Dictionary<char, CharEntry> Entries { get; }
    public Texture2D Texture { get; }
    public int Size { get; }

    public SpriteFont(Texture2D texture, int size, Dictionary<char, CharEntry> entries)
    {
        Texture = texture;
        Entries = entries;
        Size = size;
    }

    public Vector2 MeasureText<T>(T text) where T : IEnumerable<char>
    {
        float totalWidth = 0;
        float totalHeight = 0;

        float curWidth = 0;
        foreach (var chr in text)
        {
            if (chr == '\n')
            {
                curWidth = 0f;
                totalHeight += Size * LineHeightMultiplier;
                continue;
            }
            if (!Entries.TryGetValue(chr, out var entry))
                continue;
            curWidth += entry.Advance / 64f;
            if (curWidth > totalWidth)
                totalWidth = curWidth;
        }
        return new(totalWidth, totalHeight);
    }
}