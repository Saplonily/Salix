using FreeTypeSharp.Native;

using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace TextAtlasMaker;

internal class Program
{
    // TODO: This tool currently needs to be manually modified source code instead of being a command line tool
    public static void Main()
    {
        var (atlasImage, dic) = TextAtlasMaker.Make(@"C:\Windows\Fonts\PERTILI.TTF", 24, GetChars());
        atlasImage.SaveAsPng(@"atlas.png");
        atlasImage.Dispose();

        using FileStream fs = new(@"atlas_info.bin", FileMode.OpenOrCreate, FileAccess.Write);
        BinaryWriter bw = new(fs);
        foreach (var pair in dic)
        {
            bw.Write(pair.Key);
            var info = pair.Value;
            bw.Write((short)info.X);
            bw.Write((short)info.Y);
            bw.Write((short)info.Width);
            bw.Write((short)info.Height);
            bw.Write((short)info.BearingX);
            bw.Write((short)info.BearingY);
            bw.Write((short)info.Advance);
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
        for (char i = '\0'; i < char.MaxValue; i++)
            yield return i;
    }
}

public static class TextAtlasMaker
{
    private struct CharInfoPrivate
    {
        public byte[] BitmapData;
        public char Char;
        public int BitmapWidth;
        public int BitmapHeight;
        public int BearingX;
        public int BearingY;
        public int Advance;
    }

    public struct CharInfo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BearingX { get; set; }
        public int BearingY { get; set; }
        public int Advance { get; set; }
    }

    public static (Image<A8> atlasImage, Dictionary<char, CharInfo> infoDictionary) Make(
        string fontPath, int pixelHeight, IEnumerable<char> chars
        )
    {
        FT.FT_Init_FreeType(out nint library);
        FT.FT_New_Face(library, fontPath, 0, out nint aface);

        int availableCharCount = 0;
        int charWidth = 0;
        int charHeight = 0;
        List<CharInfoPrivate> privateInfos = new();

        foreach (var item in chars)
        {
            var charInfoNullable = LoadChar(aface, item, pixelHeight);
            if (!charInfoNullable.HasValue) continue;
            var charInfo = charInfoNullable.Value;
            privateInfos.Add(charInfo);
            if (charInfo.BitmapWidth > charWidth)
                charWidth = charInfo.BitmapWidth;

            if (charInfo.BitmapHeight > charHeight)
                charHeight = charInfo.BitmapHeight;

            if (charInfo.BitmapWidth != 0 && charInfo.BitmapHeight != 0)
                availableCharCount++;
        }

        var w = Math.Sqrt(charWidth * charHeight * availableCharCount);
        int countW = (int)Math.Ceiling(w / charWidth);
        int countH = (int)Math.Ceiling(w / charHeight);
        int width = countW * charWidth;//+ countW - 1;
        int height = countH * charHeight;//+ countH - 1;

        Dictionary<char, CharInfo> dic = new();
        Image<A8> ourImage = new(width, height);
        ourImage.Mutate(p =>
        {
            int curW = 0;
            int curH = 0;
            foreach (var item in privateInfos)
            {
                int x = curW * charWidth;// + curW;
                int y = curH * charHeight;// + curH;
                dic.Add(item.Char, new CharInfo
                {
                    Advance = item.Advance,
                    BearingX = item.BearingX,
                    BearingY = item.BearingY,
                    Height = item.BitmapHeight,
                    Width = item.BitmapWidth,
                    X = x,
                    Y = y
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

        unsafe static CharInfoPrivate? LoadChar(nint face, char chr, int pixelHeight = 48)
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
                return new CharInfoPrivate()
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