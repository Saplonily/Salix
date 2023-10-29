using System.Drawing;
using System.Numerics;

#pragma warning disable CS8981
using vpct = Monosand.VertexPosition2DColorTexture;

namespace Monosand;

public sealed partial class SpriteBatch
{
    private readonly VertexBuffer<vpct> buffer;
    private readonly RenderContext context;

    private SpriteEffect effect = null!;
    private bool isDrawingText;
    private Matrix4x4 transform;
    private Matrix4x4 projection;

    private Texture2D? lastTexture;
    private vpct[] vertices;
    private ushort[] indices;
    private int verticesIndex;
    private int indicesIndex;

    public RenderContext RenderContext => context;

    public SpriteEffect Effect
    {
        get => effect;
        set
        {
            ThrowHelper.ThrowIfNull(value);
            if (effect != value)
            {
                Flush();
                effect = value;
            }
        }
    }

    public Matrix4x4 Transform { get => transform; set => transform = value; }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> that this <see cref="SpriteBatch"/> belongs to.</param>
    public SpriteBatch(Game game)
        : this(game, null)
    {
    }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> that this <see cref="SpriteBatch"/> belongs to.</param>
    /// <param name="spriteEffect">The default <see cref="SpriteBatch.Effect"/> will be used.
    /// If's set to <see langword="null"/>, then glsl shader "SpriteShader.frag" "SpriteShader.vert" will be loaded and used.</param>
    public SpriteBatch(Game game, SpriteEffect? spriteEffect)
    {
        context = game.RenderContext;
        vertices = new vpct[4 * 16];
        indices = new ushort[6 * 16];
        buffer = new(context, vpct.VertexDeclaration, VertexBufferDataUsage.StreamDraw, true);
        transform = Matrix4x4.Identity;
        if (spriteEffect is null)
        {
            var rl = game.ResourceLoader;
            using var vert = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.SpriteShader.vert");
            using var frag = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.SpriteShader.frag");
            Effect = new(rl.LoadGlslShader(vert, frag));
        }
        else
        {
            Effect = spriteEffect;
        }

        context.ViewportChanged += OnContextViewportChanged;
    }

    private void OnContextViewportChanged(RenderContext renderContext, Rectangle rect)
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat *= Matrix4x4.CreateTranslation(-rect.Width / 2f, -rect.Height / 2f, 0f);
        mat *= Matrix4x4.CreateScale(2f / rect.Width, -2f / rect.Height, 1f);
        projection = mat;
    }

    // TODO a more elegant way to replace these methods?
    #region tons of Drawxxx methods

    public void DrawTexture(Texture2D texture, Vector2 position)
        => DrawTexture(texture, position, Vector2.Zero, Vector2.One, 0f, Color.Known.White);

    public void DrawTexture(Texture2D texture, Vector2 position, in RectangleProp<Color> color)
        => DrawTexture(texture, position, Vector2.Zero, Vector2.One, 0f, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, float radian)
        => DrawTexture(texture, position, origin, Vector2.One, radian, Color.Known.White);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, float radian, in RectangleProp<Color> color)
        => DrawTexture(texture, position, origin, Vector2.One, radian, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 scale)
        => DrawTexture(texture, position, Vector2.Zero, scale, 0f, Color.Known.White);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 scale, in RectangleProp<Color> color)
        => DrawTexture(texture, position, Vector2.Zero, scale, 0f, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, Vector2 scale, float radian)
        => DrawTexture(texture, position, origin, scale, radian, Color.Known.White);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, Vector2 scale, float radian, in RectangleProp<Color> color)
        => DrawTexture(texture, position, origin, scale, radian, color, Vector2.Zero, Vector2.One);

    public void DrawTextureMatrix(Texture2D texture, in Matrix3x2 matrix)
        => DrawTextureMatrix(texture, in matrix, new RectangleProp<Color>(Color.Known.White), Vector2.Zero, Vector2.One);

    public void DrawTextureMatrix(Texture2D texture, in Matrix3x2 matrix, Color color)
        => DrawTextureMatrix(texture, in matrix, new RectangleProp<Color>(color), Vector2.Zero, Vector2.One);

    public void DrawTextureMatrix(Texture2D texture, in Matrix3x2 matrix, Color color, Vector2 textureTopLeft, Vector2 textureBottomRight)
        => DrawTextureMatrix(texture, in matrix, new RectangleProp<Color>(color), textureTopLeft, textureBottomRight);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, Vector2.One, 0f, Color.Known.Black);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position, Color color)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, Vector2.One, 0f, color);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position, Vector2 origin, float radians)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, origin, Vector2.One, radians, Color.Known.Black);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position, Vector2 origin, float radians, Color color)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, origin, Vector2.One, radians, color);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position, Vector2 scale)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, scale, 0f, Color.Known.Black);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position, Vector2 scale, Color color)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, scale, 0f, color);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, in T, Vector2, Vector2, Vector2, float, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, in T text, Vector2 position, Vector2 origin, Vector2 scale, float radians)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, origin, scale, radians, Color.Known.Black);

    #endregion

    /// <summary>Draw lines of text to the <see cref="Monosand.RenderContext"/>.</summary>
    /// <typeparam name="T">The type which implements <see cref="IEnumerable{char}"/>, used to enumerate characters.</typeparam>
    /// <param name="spriteFont">The <see cref="SpriteFont"/> will be used to draw.</param>
    /// <param name="text">Text to draw, allowing '\n' for newlines.</param>
    /// <param name="position">Destination position.</param>
    /// <param name="origin">Origin, as percentage.</param>
    /// <param name="scale">Scaling factor</param>
    /// <param name="radians">Rotation radians.</param>
    /// <param name="color">Color, usually <see cref="Color.Black"/>.</param>
    public void DrawText<T>(SpriteFont spriteFont, in T text,
        Vector2 position, Vector2 origin,
        Vector2 scale, float radians,
        Color color
        ) where T : IEnumerable<char>
    {
        ThrowHelper.ThrowIfNull(spriteFont);
        Flush();
        isDrawingText = true;
        float texWidth = spriteFont.Texture.Width;
        float texHeight = spriteFont.Texture.Height;
#if NETSTANDARD2_0
        float sin = (float)Math.Sin(radians);
        float cos = (float)Math.Cos(radians);
#else
        float sin = MathF.Sin(radians);
        float cos = MathF.Cos(radians);
#endif

        // measure the string
        float totalWidth = 0;
        float totalHeight = 0;

        float curWidth = 0;
        foreach (var chr in text)
        {
            if (chr == '\n')
            {
                curWidth = 0f;
                totalHeight += spriteFont.Size;
                continue;
            }
            if (!spriteFont.Entries.TryGetValue(chr, out var entry))
                continue;
            curWidth += entry.Advance / 64f;
            if (curWidth > totalWidth)
                totalWidth = curWidth;
        }

        float x = 0f;
        float y = 0f;
        foreach (var chr in text)
        {
            if (chr == '\n')
            {
                x = 0f;
                // TODO custom line-height
                y += spriteFont.Size * 1.2f;
            }
            if (!spriteFont.Entries.TryGetValue(chr, out var entry))
                continue;
            Vector2 tl = new(entry.X / texWidth, entry.Y / texHeight);
            Vector2 br = new(tl.X + entry.Width / texWidth, tl.Y + entry.Height / texHeight);
            Vector2 realOrigin =
                (origin * new Vector2(totalWidth, totalHeight) - new Vector2(x + entry.BearingX, y + spriteFont.Size - entry.BearingY))
                / new Vector2(entry.Width, entry.Height);

            DrawTexture(spriteFont.Texture, position, realOrigin, (br - tl) * scale, radians, color, tl, br);
            x += entry.Advance / 64f;
        }
        Flush();
        isDrawingText = false;
    }

    public void DrawTexture(
        Texture2D texture,
        Vector2 position, Vector2 origin,
        Vector2 scale, float radians, in RectangleProp<Color> color,
        Vector2 textureTopLeft, Vector2 textureBottomRight
        )
    {
        float w = texture.Width;
        float h = texture.Height;
        Vector2 texSize = new(w, h);
        Vector2 tl = new Vector2(0, 0) - origin * texSize;
        Vector2 tr = new Vector2(w, 0) - origin * texSize;
        Vector2 br = new Vector2(w, h) - origin * texSize;
        Vector2 bl = new Vector2(0, h) - origin * texSize;
        tl *= scale;
        tr *= scale;
        br *= scale;
        bl *= scale;

        // why there isn't MathF in .net std 2.0?
#if NETSTANDARD2_0
        float sin = (float)Math.Sin(radians);
        float cos = (float)Math.Cos(radians);
#else
        float sin = MathF.Sin(radians);
        float cos = MathF.Cos(radians);
#endif

        // [ cos  -sin ]     [ x ]     [ x * cos - y * sin ]
        // [           ]  *  [   ]  =  [                   ]
        // [ sin   cos ]     [ y ]     [ x * sin + y * cos ]

        tl = new(tl.X * cos - tl.Y * sin, tl.X * sin + tl.Y * cos);
        tr = new(tr.X * cos - tr.Y * sin, tr.X * sin + tr.Y * cos);
        br = new(br.X * cos - br.Y * sin, br.X * sin + br.Y * cos);
        bl = new(bl.X * cos - bl.Y * sin, bl.X * sin + bl.Y * cos);

        tl += position;
        tr += position;
        br += position;
        bl += position;

        DrawTextureRectangle(texture, color, new(tl, tr, bl, br), textureTopLeft, textureBottomRight);
    }

    public void DrawTextureMatrix(
        Texture2D texture, in Matrix3x2 matrix,
        in RectangleProp<Color> color,
        Vector2 textureTopLeft, Vector2 textureBottomRight
        )
    {
        float w = texture.Width;
        float h = texture.Height;

        Vector2 tl = Vector2.Transform(new Vector2(0, 0), matrix);
        Vector2 tr = Vector2.Transform(new Vector2(w, 0), matrix);
        Vector2 br = Vector2.Transform(new Vector2(w, h), matrix);
        Vector2 bl = Vector2.Transform(new Vector2(0, h), matrix);
        DrawTextureRectangle(texture, color, new(tl, tr, bl, br), textureTopLeft, textureBottomRight);
    }

    public unsafe void DrawTextureRectangle(
        Texture2D texture,
        in RectangleProp<Color> color,
        in RectangleProp<Vector2> position,
        Vector2 textureTopLeft, Vector2 textureBottomRight
        )
    {
        ThrowHelper.ThrowIfNull(texture);
        if (lastTexture != texture) Flush();
        if (verticesIndex >= ushort.MaxValue - 8) Flush();
        int vind = verticesIndex;
        int iind = indicesIndex;

        lastTexture = texture;

        if (vertices.Length <= vind)
            Array.Resize(ref vertices, Math.Max(vind + 8, vertices.Length * 2));
        if (indices.Length <= iind)
            Array.Resize(ref indices, Math.Max(iind + 12, indices.Length * 2));

        fixed (vpct* vptr = vertices)
        fixed (ushort* iptr = indices)
        {
            vptr[vind + 0] =
                new(position.TopLeft, color.TopLeft.ToVector4(), new(textureTopLeft.X, 1.0f - textureTopLeft.Y));
            vptr[vind + 1] =
                new(position.TopRight, color.TopRight.ToVector4(), new(textureBottomRight.X, 1.0f - textureTopLeft.Y));
            vptr[vind + 2] =
                new(position.BottomLeft, color.BottomLeft.ToVector4(), new(textureTopLeft.X, 1.0f - textureBottomRight.Y));
            vptr[vind + 3] =
                new(position.BottomRight, color.BottomRight.ToVector4(), new(textureBottomRight.X, 1.0f - textureBottomRight.Y));

            iptr[indicesIndex + 0] = (ushort)(vind + 0);
            iptr[indicesIndex + 1] = (ushort)(vind + 1);
            iptr[indicesIndex + 2] = (ushort)(vind + 2);
            iptr[indicesIndex + 3] = (ushort)(vind + 1);
            iptr[indicesIndex + 4] = (ushort)(vind + 2);
            iptr[indicesIndex + 5] = (ushort)(vind + 3);
        }

        verticesIndex += 4;
        indicesIndex += 6;
    }

    /// <summary>Flush the batched draw actions.</summary>
    public void Flush()
    {
        if (verticesIndex == 0) return;
        context.SetTexture(0, lastTexture!);
        Effect.Use();
        Effect.SetIsDrawingText(isDrawingText);
        Effect.SetProjection(ref projection);
        Effect.SetTransform(ref transform);

        buffer.SetIndexData(indices.AsSpan(0, indicesIndex));
        buffer.SetData(vertices.AsSpan(0, verticesIndex));
        context.DrawIndexedPrimitives(buffer, PrimitiveType.TriangleList);
        verticesIndex = indicesIndex = 0;
    }
}