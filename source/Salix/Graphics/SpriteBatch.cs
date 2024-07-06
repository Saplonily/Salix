﻿using System.Drawing;
using System.Numerics;

using VertexType = Saladim.Salix.VertexPosition2DColorTexture;

namespace Saladim.Salix;

public sealed partial class SpriteBatch
{
    private readonly VertexBuffer<VertexType> buffer;
    private readonly RenderContext context;

    private bool flushing;
    private SpriteShader shader = null!;
    private Matrix3x2 transform2d;
    private Matrix3x2 projection2d;
    private bool projection2dDirty = true;

    private Texture2D? lastTexture;
    private VertexType[] vertices;
    private ushort[] indices;
    private int verticesIndex;
    private int indicesIndex;

    private Matrix3x2 CleanedProjection2D
    {
        get
        {
            if (!projection2dDirty) return projection2d;
            Rectangle rect = RenderContext.Viewport;
            Matrix3x2 mat = Matrix3x2.Identity;
            mat *= Matrix3x2.CreateScale(2f / rect.Width, -2f / rect.Height);
            mat *= Matrix3x2.CreateTranslation(-1f, 1f);
            if (RenderContext.RenderTarget is not null)
                mat *= Matrix3x2.CreateScale(1f, -1f);
            projection2d = mat;
            return projection2d;
        }
    }

    public RenderContext RenderContext => context;
    public Texture2D Texture1x1White { get; }
    public ref Matrix3x2 Transform2D => ref transform2d;

    public SpriteShader SpriteShader { get; set; }
    public SpriteShader TextShader { get; set; }

    private SpriteShader Shader
    {
        get => shader;
        set
        {
            ThrowHelper.ThrowIfNull(value);
            if (shader != value)
            {
                Flush();
                shader = value;
            }
        }
    }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="game">The <see cref="Game"/> that this <see cref="SpriteBatch"/> belongs to.</param>
    public SpriteBatch(Game game)
    {
        context = game.RenderContext;
        vertices = new VertexType[4 * 16];
        indices = new ushort[6 * 16];
        transform2d = Matrix3x2.Identity;
        projection2d = Matrix3x2.Identity;
        buffer = new(context, VertexType.VertexDeclaration, VertexBufferDataUsage.StreamDraw, true);
        ReadOnlySpan<byte> imgData = [255, 255, 255, 255];
        Texture1x1White = new Texture2D(context, 1, 1, imgData, ImageFormat.Rgba32);
        Texture1x1White.Filter = TextureFilterType.Nearest;

        using var vert = ResourceLoader.OpenEmbeddedFileStream("SpriteShader.vert");
        using var frag = ResourceLoader.OpenEmbeddedFileStream("SpriteShader.frag");
        using var vertText = ResourceLoader.OpenEmbeddedFileStream("TextShader.vert");
        using var fragText = ResourceLoader.OpenEmbeddedFileStream("TextShader.frag");

        var loader = game.ResourceLoader;
        SpriteShader = new(loader.LoadGlslShader(vert, frag));
        TextShader = new(loader.LoadGlslShader(vertText, fragText));

        game.Window.PreviewSwapBuffer += _ => Flush();
        context.StateChanged += ContextStateChanged;
        context.PreviewStateChanged += PreviewContextStateChanged;
        Shader = SpriteShader;
    }

    private void ContextStateChanged(RenderContextState state)
    {
        if (state is RenderContextState.Viewport) projection2dDirty = true;
    }

    private void PreviewContextStateChanged(RenderContextState state)
    {
        if (flushing) return;
        if (state is
            RenderContextState.Viewport or
            RenderContextState.RenderTarget or
            RenderContextState.Shader or
            RenderContextState.Sampler)
            Flush();
    }

    public void DrawTexture(Texture2D texture, DrawTransform drawTransform, RectangleProperty<Color> color)
        => DrawTexture(texture, drawTransform, color, Vector2.Zero, Vector2.One);

    public void DrawTexture(Texture2D texture, DrawTransform drawTransform)
        => DrawTexture(texture, drawTransform, Color.Known.White, Vector2.Zero, Vector2.One);

    public void DrawTexture(
        Texture2D texture,
        DrawTransform drawTransform,
        RectangleProperty<Color> color,
        Vector2 textureTopLeft,
        Vector2 textureBottomRight
        )
        => DrawTexture(
            texture,
            drawTransform.Position,
            drawTransform.Origin,
            drawTransform.Scale,
            drawTransform.Radians,
            color,
            textureTopLeft,
            textureBottomRight
            );

    public void DrawTextureMatrix(Texture2D texture, Matrix3x2 matrix)
        => DrawTextureMatrix(texture, matrix, new RectangleProperty<Color>(Color.Known.White), Vector2.Zero, Vector2.One);

    public void DrawTextureMatrix(Texture2D texture, Matrix3x2 matrix, Color color)
        => DrawTextureMatrix(texture, matrix, new RectangleProperty<Color>(color), Vector2.Zero, Vector2.One);

    /// <inheritdoc cref="DrawText{T}(SpriteFont, T, DrawTransform, Color)"/>
    public void DrawText<T>(SpriteFont spriteFont, T text, DrawTransform drawTransform) where T : IEnumerable<char>
        => DrawText(spriteFont, text, drawTransform.Position, drawTransform.Origin, drawTransform.Scale, drawTransform.Radians, Color.Known.Black);

    /// <summary>Draw lines of text to the <see cref="Saladim.Salix.RenderContext"/>.</summary>
    /// <typeparam name="T">The type which implements <see cref="IEnumerable{char}"/>, used to enumerate characters.</typeparam>
    /// <param name="spriteFont">The <see cref="SpriteFont"/> will be used to draw.</param>
    /// <param name="text">Text to draw, allowing '\n' for newlines.</param>
    /// <param name="drawTransform"><see cref="DrawTransform"/> that applys to this draw.</param>
    /// <param name="color">Color, usually and defaulted <see cref="Color.Known.Black"/>.</param>
    public void DrawText<T>(SpriteFont spriteFont, T text, DrawTransform drawTransform, Color color) where T : IEnumerable<char>
        => DrawText(spriteFont, text, drawTransform.Position, drawTransform.Origin, drawTransform.Scale, drawTransform.Radians, color);

    /// <summary>Draw lines of text to the <see cref="Saladim.Salix.RenderContext"/>.</summary>
    /// <typeparam name="T">The type which implements <see cref="IEnumerable{char}"/>, used to enumerate characters.</typeparam>
    /// <param name="spriteFont">The <see cref="SpriteFont"/> will be used to draw.</param>
    /// <param name="text">Text to draw, allowing '\n' for newlines.</param>
    /// <param name="position">Destination position.</param>
    /// <param name="origin">Origin, as percentage.</param>
    /// <param name="scale">Scaling factor</param>
    /// <param name="radians">Rotation radians.</param>
    /// <param name="color">Color, usually and defaulted <see cref="Color.Known.Black"/>.</param>
    public void DrawText<T>(SpriteFont spriteFont, T text,
        Vector2 position, Vector2 origin,
        Vector2 scale, float radians,
        Color color
        ) where T : IEnumerable<char>
    {
        ThrowHelper.ThrowIfNull(spriteFont);
        Shader = TextShader;
        float texWidth = spriteFont.Texture.Width;
        float texHeight = spriteFont.Texture.Height;

        Vector2 rawSize = spriteFont.MeasureText(text);

        float x = 0f;
        float y = 0f;
        foreach (var chr in text)
        {
            if (chr == '\n')
            {
                x = 0f;
                y += spriteFont.Size * SpriteFont.LineHeightMultiplier;
            }
            if (!spriteFont.Entries.TryGetValue(chr, out var entry))
                continue;
            Vector2 tl = new(entry.X / texWidth, entry.Y / texHeight);
            Vector2 br = new(tl.X + entry.Width / texWidth, tl.Y + entry.Height / texHeight);
            Vector2 realOrigin =
                (origin * new Vector2(rawSize.X, rawSize.Y) - new Vector2(x + entry.BearingX, y + spriteFont.Size - entry.BearingY))
                / new Vector2(entry.Width, entry.Height);

            DrawTexture(spriteFont.Texture, position, realOrigin, (br - tl) * scale, radians, color, tl, br);
            x += entry.Advance / 64f;
        }
    }

    public void DrawTexture(
        Texture2D texture,
        Vector2 position, Vector2 origin,
        Vector2 scale, float radians, RectangleProperty<Color> color,
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

        float sin = MathF.Sin(radians);
        float cos = MathF.Cos(radians);

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
        Texture2D texture, Matrix3x2 matrix,
        RectangleProperty<Color> color,
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
        RectangleProperty<Color> color,
        RectangleProperty<Vector2> position,
        Vector2 textureTopLeft, Vector2 textureBottomRight
        )
    {
        ThrowHelper.ThrowIfNull(texture);
        Shader = SpriteShader;
        if (lastTexture != texture) Flush();
        lastTexture = texture;
        EnsureVerticesAndIndices(4, 6);

        int vind = verticesIndex;
        int iind = indicesIndex;

        fixed (VertexType* vptr = vertices)
        fixed (ushort* iptr = indices)
        {
            vptr[vind + 0] =
                new(position.TopLeft, color.TopLeft, new(textureTopLeft.X, textureTopLeft.Y));
            vptr[vind + 1] =
                new(position.TopRight, color.TopRight, new(textureBottomRight.X, textureTopLeft.Y));
            vptr[vind + 2] =
                new(position.BottomLeft, color.BottomLeft, new(textureTopLeft.X, textureBottomRight.Y));
            vptr[vind + 3] =
                new(position.BottomRight, color.BottomRight, new(textureBottomRight.X, textureBottomRight.Y));

            iptr[iind + 0] = (ushort)(vind + 0);
            iptr[iind + 1] = (ushort)(vind + 1);
            iptr[iind + 2] = (ushort)(vind + 2);
            iptr[iind + 3] = (ushort)(vind + 1);
            iptr[iind + 4] = (ushort)(vind + 2);
            iptr[iind + 5] = (ushort)(vind + 3);
        }

        verticesIndex += 4;
        indicesIndex += 6;
    }

    // TODO now we have circle drawing, then we'll need arc drawing
    public unsafe void DrawCircle(Texture2D texture, Matrix3x2 matrix, Color color, int precise = 24)
    {
        ThrowHelper.ThrowIfNull(texture);
        Shader = SpriteShader;
        if (precise > 8192)
            throw new ArgumentOutOfRangeException(nameof(precise), precise, SR.PreciseTooBig);
        if (precise < 3)
            throw new ArgumentOutOfRangeException(nameof(precise), precise, SR.PreciseTooSmall);

        if (lastTexture != texture) Flush();
        lastTexture = texture;
        EnsureVerticesAndIndices(precise, (precise - 2) * 3);

        int vind = verticesIndex;
        int iind = indicesIndex;
        float radianPerSide = MathF.PI * 2f / precise;

        for (int i = 0; i < precise; i++)
        {
            Vector2 pos = new(MathF.Cos(radianPerSide * i), MathF.Sin(radianPerSide * i));
            vertices[vind + i] = new(Vector2.Transform(pos, matrix), color, new(pos.X / 2f + 0.5f, pos.Y / 2f + 0.5f));
        }
        // precise = 4: 0 1 2 / 0 2 3
        // precise = 6: 0 1 2 / 0 2 3 / 0 3 4 / 0 4 5
        // precise = 8: 0 1 2 / 0 2 3 / 0 3 4 / 0 4 5 / 0 5 6 / 0 6 7
        for (int i = 0; i < precise - 2; i++)
        {
            indices[iind + i * 3 + 0] = (ushort)(vind + 0 + 0);
            indices[iind + i * 3 + 1] = (ushort)(vind + i + 1);
            indices[iind + i * 3 + 2] = (ushort)(vind + i + 2);
        }

        verticesIndex += precise;
        indicesIndex += (precise - 2) * 3;
    }

    public void DrawCircle(Texture2D texture, DrawTransform drawTransform, Color color, int precise = 24)
    {
        DrawTransform dt = drawTransform;
        dt.Scale *= texture.Size;
        DrawCircle(texture, dt.BuildMatrix(texture.Size), color, precise);
    }

    public void DrawCircle(Texture2D texture, DrawTransform drawTransform, int precise = 24)
        => DrawCircle(texture, drawTransform, Color.Known.White, precise);

    public void DrawCircle(Color color, float size, DrawTransform drawTransform, int precise = 24)
    {
        DrawTransform dt = drawTransform;
        dt.Scale *= new Vector2(size);
        DrawCircle(Texture1x1White, dt.BuildMatrix(new(size)), color, precise);
    }

    public unsafe void DrawTriangle(
        Texture2D texture,
        TriangleProperty<Vector2> pointPositions,
        TriangleProperty<Vector2> textureCoord,
        TriangleProperty<Color> color,
        Matrix3x2 matrix
        )
    {
        ThrowHelper.ThrowIfNull(texture);
        Shader = SpriteShader;
        if (lastTexture != texture) Flush();
        lastTexture = texture;
        EnsureVerticesAndIndices(3, 3);
        int vind = verticesIndex;
        int iind = indicesIndex;

        fixed (VertexType* vptr = vertices)
        fixed (ushort* iptr = indices)
        {
            vptr[vind + 0] =
                new(Vector2.Transform(pointPositions.First, matrix), color.First, textureCoord.First);
            vptr[vind + 1] =
                new(Vector2.Transform(pointPositions.Second, matrix), color.Second, textureCoord.Second);
            vptr[vind + 2] =
                new(Vector2.Transform(pointPositions.Third, matrix), color.Third, textureCoord.Third);

            iptr[indicesIndex + 0] = (ushort)(vind + 0);
            iptr[indicesIndex + 1] = (ushort)(vind + 1);
            iptr[indicesIndex + 2] = (ushort)(vind + 2);
        }

        verticesIndex += 3;
        indicesIndex += 3;
    }

    public void DrawTriangle(Texture2D texture,
        TriangleProperty<Vector2> pointPositions,
        TriangleProperty<Vector2> textureCoord,
        TriangleProperty<Color> color,
        DrawTransform drawTransform)
        => DrawTriangle(texture, pointPositions, textureCoord, color, drawTransform.BuildMatrix(texture.Size));

    private void EnsureVerticesAndIndices(int newVerticesCount, int newIndicesCount)
    {
        if (verticesIndex >= ushort.MaxValue - newVerticesCount) Flush();
        if (indicesIndex >= ushort.MaxValue - newIndicesCount) Flush();
        int vind = verticesIndex;
        int iind = indicesIndex;
        if (vertices.Length <= vind + newVerticesCount)
            Array.Resize(ref vertices, Math.Max(vind + newVerticesCount, vertices.Length * 2));
        if (indices.Length <= iind + newIndicesCount)
            Array.Resize(ref indices, Math.Max(iind + newIndicesCount, indices.Length * 2));
    }

    public void Flush()
    {
        if (verticesIndex == 0) return;
        flushing = true;
        context.SetTexture(0, lastTexture!);
        Shader.Use();
        Shader.SetTransform2D(transform2d);
        Shader.SetProjection2D(CleanedProjection2D);
        buffer.SetData(vertices.AsSpan(0, verticesIndex));
        buffer.SetIndexData(indices.AsSpan(0, indicesIndex));
        context.DrawIndexedPrimitives(buffer, PrimitiveType.TriangleList);
        verticesIndex = indicesIndex = 0;
        flushing = false;
    }
}