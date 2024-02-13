using System.Runtime.CompilerServices;

using FreeTypeSharp.Native;

namespace TextAtlasMaker;

internal class Program
{
    // TODO: better packing methods
    // TODO: This tool currently needs to be manually modified source code instead of being a command line tool
    public static void Main()
    {
        int fontSize = 24;

        //string fontFile = @"C:\Windows\Fonts\PERTILI.TTF";
        string fontFile = @"C:\Windows\Fonts\msyhl.ttc";
        var (atlasImage, dic) = TextAtlasMaker.Make(fontFile, fontSize, GetChars());
        atlasImage.SaveAsPng(@"atlas.png");
        atlasImage.Dispose();

        using FileStream fs = new(@"atlas.bin", FileMode.Create, FileAccess.Write);
        using BinaryWriter bw = new(fs);
        bw.Write((short)fontSize);
        foreach (var pair in dic)
        {
            bw.Write((short)pair.Key);
            var info = pair.Value;
            bw.Write(info.X);
            bw.Write(info.Y);
            bw.Write(info.Width);
            bw.Write(info.Height);
            bw.Write(info.BearingX);
            bw.Write(info.BearingY);
            bw.Write(info.Advance);
        }


        //JsonSerializerOptions options = new()
        //{
        //    WriteIndented = true,
        //    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        //};
        //File.WriteAllText(@"D:\User\temp\text_atlas_test\atlas_info.json", JsonSerializer.Serialize(dic, options));
    }

    public static IEnumerable<char> GetChars()
    {
        for (char i = '\0'; i < (char)byte.MaxValue; i++)
            yield return i;
    }
}

public static class TextAtlasMaker
{
    private struct CharEntryPrivate
    {
        public byte[] BitmapData;
        public char Char;
        public int BitmapWidth;
        public int BitmapHeight;
        public int BearingX;
        public int BearingY;
        public int Advance;
    }

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

    public static (Image<A8> atlasImage, Dictionary<char, CharEntry> entriesDictionary) Make(
        string fontPath, int pixelHeight, IEnumerable<char> chars
        )
    {
        FT.FT_Init_FreeType(out nint library);
        FT.FT_New_Face(library, fontPath, 0, out nint aface);

        int availableCharCount = 0;
        int charWidth = 0;
        int charHeight = 0;
        List<CharEntryPrivate> privateEntries = new();

        foreach (var item in chars)
        {
            var charEntryNullable = LoadChar(aface, item, pixelHeight);
            if (!charEntryNullable.HasValue) continue;
            var entry = charEntryNullable.Value;

            privateEntries.Add(entry);
            if (entry.BitmapWidth > charWidth)
                charWidth = entry.BitmapWidth;

            if (entry.BitmapHeight > charHeight)
                charHeight = entry.BitmapHeight;

            if (entry.BitmapWidth != 0 && entry.BitmapHeight != 0)
                availableCharCount++;
        }

        var w = Math.Sqrt(charWidth * charHeight * availableCharCount);
        int countW = (int)Math.Ceiling(w / charWidth);
        int countH = (int)Math.Ceiling(w / charHeight);
        int width = countW * charWidth;//+ countW - 1;
        int height = countH * charHeight;//+ countH - 1;

        Dictionary<char, CharEntry> dic = new();
        Image<A8> ourImage = new(width, height);
        ourImage.Mutate(p =>
        {
            int curW = 0;
            int curH = 0;
            foreach (var item in privateEntries)
            {
                int x = curW * charWidth;// + curW;
                int y = curH * charHeight;// + curH;
                dic.Add(item.Char, new CharEntry
                {
                    Advance = (short)item.Advance,
                    BearingX = (short)item.BearingX,
                    BearingY = (short)item.BearingY,
                    Height = (short)item.BitmapHeight,
                    Width = (short)item.BitmapWidth,
                    X = (short)x,
                    Y = (short)y
                });
                if (item.BitmapHeight == 0 || item.BitmapWidth == 0) continue;
                using var singleTextImg = Image.LoadPixelData<A8>(item.BitmapData, item.BitmapWidth, item.BitmapHeight);
                p.DrawImage(singleTextImg, new Point(x, y), 1f);
                curW++;
                if (curW >= countW)
                {
                    curW = 0;
                    curH++;
                }
            }
        });

        FT.FT_Done_Face(aface);
        FT.FT_Done_FreeType(library);
        return (ourImage, dic);

        unsafe static CharEntryPrivate? LoadChar(nint face, char chr, int pixelHeight = 48)
        {
            FT.FT_Set_Pixel_Sizes(face, 0, (uint)pixelHeight);
            var glyphIndex = FT.FT_Get_Char_Index(face, chr);
            if (glyphIndex == 0) return null;
            FT.FT_Load_Char(face, chr, FT.FT_LOAD_RENDER);
            FT_FaceRec* rec = (FT_FaceRec*)face;
            var bitmap = rec->glyph->bitmap;
            int width = (int)bitmap.width;
            int height = (int)bitmap.rows;
            int bearingX = rec->glyph->bitmap_left;
            int bearingY = rec->glyph->bitmap_top;
            int advance = (int)rec->glyph->advance.x;

            byte[] data = new byte[width * height];
            fixed (byte* ptr = data)
            {
                Unsafe.CopyBlock(ptr, (void*)rec->glyph->bitmap.buffer, (uint)(width * height));
            }
            checked
            {
                return new CharEntryPrivate()
                {
                    BitmapData = data,
                    BearingX = bearingX,
                    BearingY = bearingY,
                    BitmapHeight = (int)bitmap.rows,
                    BitmapWidth = (int)bitmap.width,
                    Advance = advance,
                    Char = chr
                };
            }
        }
    }
}