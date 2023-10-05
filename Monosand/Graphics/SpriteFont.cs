namespace Monosand;

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

    public Dictionary<char, CharEntry> Entries { get; }
    public Texture2D Texture { get; }
    public int Size { get; }

    public SpriteFont(Texture2D texture, int size, Dictionary<char, CharEntry> entries)
    {
        Texture = texture;
        Entries = entries;
        Size = size;
    }
}