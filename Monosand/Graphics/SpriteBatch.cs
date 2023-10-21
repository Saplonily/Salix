using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS8981

using vpct = Monosand.VertexPositionColorTexture;

namespace Monosand;

public sealed partial class SpriteBatch
{
    private struct BatchItem
    {
        public Texture2D tex;
        public vpct topLeft;
        public vpct topRight;
        public vpct bottomLeft;
        public vpct bottomRight;
        public BatchItem(Texture2D tex, vpct topLeft, vpct topRight, vpct bottomLeft, vpct bottomRight)
        {
            this.tex = tex;
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomLeft = bottomLeft;
            this.bottomRight = bottomRight;
        }
    }

    private readonly VertexBuffer<vpct> buffer;
    private readonly List<BatchItem> batchItems;
    private readonly RenderContext context;

    private bool isInDrawText = false;
    private Shader? shader;
    private ShaderParameter projectionParam;
    private ShaderParameter transformParam;
    private Matrix4x4 transform;
    private Matrix4x4 projection;
    private vpct[] vertices;
    private ushort[] indices;

    public RenderContext RenderContext => context;

    public Shader SpriteShader { get; set; }
    public Shader TextShader { get; set; }

    public Matrix4x4 Transform
    {
        get => transform;
        set { transform = value; transformParam.Set(ref value); }
    }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="context">The <see cref="RenderContext"/> that this <see cref="SpriteBatch"/> will draw to.</param>
    public SpriteBatch(Game game)
        : this(game, null, null)
    {
    }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="context">The <see cref="RenderContext"/> that this <see cref="SpriteBatch"/> will draw to.</param>
    /// <param name="spriteShader">The default <see cref="SpriteShader"/> will be used.
    /// If set to <see langword="null"/>, then glsl shader "SpriteShader.frag" "SpriteShader.vert" will be loaded and used.</param>
    public SpriteBatch(Game game, Shader? spriteShader)
        : this(game, spriteShader, null)
    {
    }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="context">The <see cref="RenderContext"/> that this <see cref="SpriteBatch"/> will draw to.</param>
    /// <param name="spriteShader">The default <see cref="SpriteShader"/> will be used.
    /// If set to <see langword="null"/>, then glsl shader "SpriteShader.frag" "SpriteShader.vert" will be loaded and used.</param>
    /// <param name="textShader">The default <see cref="TextShader"/> will be used.
    /// If set to <see langword="null"/>, then glsl shader "TextShader.frag" "TextShader.vert" will be loaded and used.</param>
    public SpriteBatch(Game game, Shader? spriteShader, Shader? textShader)
    {
        this.context = game.RenderContext;
        batchItems = new();
        vertices = new vpct[4 * 16];
        indices = new ushort[6 * 16];
        buffer = new(context, vpct.VertexDeclaration, VertexBufferDataUsage.StreamDraw, true);
        if (spriteShader is null)
        {
            var rl = game.ResourceLoader;
            using var vert = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.SpriteShader.vert");
            using var frag = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.SpriteShader.frag");
            SpriteShader = rl.LoadGlslShader(vert, frag);
        }
        else
        {
            SpriteShader = spriteShader;
        }
        if (textShader is null)
        {
            var rl = game.ResourceLoader;
            using var vert = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.TextShader.vert");
            using var frag = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.TextShader.frag");
            TextShader = rl.LoadGlslShader(vert, frag);
        }
        else
        {
            TextShader = textShader;
        }
        transform = Matrix4x4.Identity;
        EnsureShader(SpriteShader);
        context.ViewportChanged += OnContextViewportChanged;
        OnContextViewportChanged(context, context.Viewport);
    }

    private void OnContextViewportChanged(RenderContext renderContext, Rectangle rect)
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat *= Matrix4x4.CreateTranslation(-rect.Width / 2f, -rect.Height / 2f, 0f);
        mat *= Matrix4x4.CreateScale(2f / rect.Width, -2f / rect.Height, 1f);
        projection = mat;
        projectionParam.Set(ref mat);
    }

    // TODO a more elegant way to replace these methods?

    public void DrawTexture(Texture2D texture, Vector2 position)
        => DrawTexture(texture, position, Vector2.Zero, Vector2.One, 0f, Color.White);

    public void DrawRenderTarget(RenderTarget renderTarget, Vector2 position)
        => DrawTexture(renderTarget.Texture, position + new Vector2(0f, renderTarget.Height), new Vector2(1f, -1f));

    public void DrawTexture(Texture2D texture, Vector2 position, Color color)
        => DrawTexture(texture, position, Vector2.Zero, Vector2.One, 0f, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, float radian)
        => DrawTexture(texture, position, origin, Vector2.One, radian, Color.White);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, float radian, Color color)
        => DrawTexture(texture, position, origin, Vector2.One, radian, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 scale)
        => DrawTexture(texture, position, Vector2.Zero, scale, 0f, Color.White);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 scale, Color color)
        => DrawTexture(texture, position, Vector2.Zero, scale, 0f, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, Vector2 scale, float radian)
        => DrawTexture(texture, position, origin, scale, radian, Color.White);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 origin, Vector2 scale, float radian, Color color)
        => DrawTexture(texture, position, origin, scale, radian, color, Vector2.Zero, Vector2.One);

    public void DrawTexture(
        Texture2D texture,
        Vector2 position, Vector2 origin,
        Vector2 scale, float radian,
        Color color,
        Vector2 partRectTL, Vector2 partRectBR
        )
    {
        ThrowHelper.ThrowIfNull(texture);
        if (!isInDrawText) EnsureShader(SpriteShader);
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
        float sin = (float)Math.Sin(radian);
        float cos = (float)Math.Cos(radian);
#else
        float sin = MathF.Sin(radian);
        float cos = MathF.Cos(radian);
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

        Vector4 c = color.ToVector4();
        // TODO need we pooling BatchItem?
        batchItems.Add(new BatchItem
        (
            tex: texture,
            topLeft: new(new(tl, 0f), c, partRectTL),
            topRight: new(new(tr, 0f), c, new(partRectBR.X, partRectTL.Y)),
            bottomLeft: new(new(bl, 0f), c, new(partRectTL.X, partRectBR.Y)),
            bottomRight: new(new(br, 0f), c, partRectBR)
        ));
    }

    public void DrawTextureMatrix(Texture2D texture, ref Matrix3x2 matrix, Color color)
         => DrawTextureMatrix(texture, ref matrix, color, color, color, color, new(0f, 0f), new(1f, 1f));

    public void DrawTextureMatrix(Texture2D texture, ref Matrix3x2 matrix, Color color, Vector2 partRectTL, Vector2 partRectBR)
        => DrawTextureMatrix(texture, ref matrix, color, color, color, color, partRectTL, partRectBR);

    public void DrawTextureMatrix(
        Texture2D texture, ref Matrix3x2 matrix,
        Color ctl, Color ctr, Color cbl, Color cbr,
        Vector2 partRectTL, Vector2 partRectBR
        )
    {
        ThrowHelper.ThrowIfNull(texture);
        if (!isInDrawText) EnsureShader(SpriteShader);
        float w = texture.Width;
        float h = texture.Height;

        Vector2 tl = Vector2.Transform(new Vector2(0, 0), matrix);
        Vector2 tr = Vector2.Transform(new Vector2(w, 0), matrix);
        Vector2 br = Vector2.Transform(new Vector2(w, h), matrix);
        Vector2 bl = Vector2.Transform(new Vector2(0, h), matrix);
        // TODO need we pooling BatchItem?
        batchItems.Add(new BatchItem
        (
            tex: texture,
            topLeft: new(new(tl, 0f), ctl.ToVector4(), partRectTL),
            topRight: new(new(tr, 0f), ctr.ToVector4(), new(partRectBR.X, partRectTL.Y)),
            bottomLeft: new(new(bl, 0f), cbl.ToVector4(), new(partRectTL.X, partRectBR.Y)),
            bottomRight: new(new(br, 0f), cbr.ToVector4(), partRectBR)
        ));
    }

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, Vector2.One, 0f, Color.Black);

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position, Color color)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, Vector2.One, 0f, color);

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position, Vector2 origin, float radian)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, origin, Vector2.One, radian, Color.Black);

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position, Vector2 origin, float radian, Color color)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, origin, Vector2.One, radian, color);

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position, Vector2 scale)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, scale, 0f, Color.Black);

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position, Vector2 scale, Color color)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, Vector2.Zero, scale, 0f, color);

    public void DrawText<T>(SpriteFont spriteFont, T text, Vector2 position, Vector2 origin, Vector2 scale, float radian)
        where T : IEnumerable<char>
        => DrawText(spriteFont, text, position, origin, scale, radian, Color.Black);

    public void DrawText<T>(SpriteFont spriteFont, T text,
        Vector2 position, Vector2 origin,
        Vector2 scale, float radian,
        Color color
    ) where T : IEnumerable<char>
    {
        ThrowHelper.ThrowIfNull(spriteFont);
        EnsureShader(TextShader);
        isInDrawText = true;
        float texWidth = spriteFont.Texture.Width;
        float texHeight = spriteFont.Texture.Height;
#if NETSTANDARD2_0
        float sin = (float)Math.Sin(radian);
        float cos = (float)Math.Cos(radian);
#else
        float sin = MathF.Sin(radian);
        float cos = MathF.Cos(radian);
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

            DrawTexture(spriteFont.Texture, position, realOrigin, (br - tl) * scale, radian, color, tl, br);
            x += entry.Advance / 64f;
        }
        isInDrawText = false;
    }

    public void Flush()
    {
        int batchItemsCount = batchItems.Count;
        if (batchItemsCount == 0) return;

        Texture2D? currentTex = batchItems[0].tex;
        context.SetTexture(0, currentTex);
        int cvid = 0;
        for (int i = 0; i < batchItemsCount; i++)
        {
            BatchItem item = batchItems[i];
            if (item.tex != currentTex)
            {
                FlushBuffer(ref cvid);
                currentTex = item.tex;
                context.SetTexture(0, currentTex);
            }
            if (cvid * 4 + 4 > ushort.MaxValue)
                FlushBuffer(ref cvid);

            if (cvid * 4 + 4 >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(cvid * 4 + 8, vertices.Length * 2));
            if (cvid * 6 + 6 >= indices.Length)
                Array.Resize(ref indices, Math.Max(cvid * 6 + 12, vertices.Length * 2));

            ref vpct vfirst = ref vertices[cvid * 4];
            ref ushort ifirst = ref indices[cvid * 6];

            Unsafe.Add(ref vfirst, 0) = item.topLeft;
            Unsafe.Add(ref vfirst, 1) = item.topRight;
            Unsafe.Add(ref vfirst, 2) = item.bottomLeft;
            Unsafe.Add(ref vfirst, 3) = item.bottomRight;

            Unsafe.Add(ref ifirst, 0) = (ushort)(cvid * 4 + 0);
            Unsafe.Add(ref ifirst, 1) = (ushort)(cvid * 4 + 1);
            Unsafe.Add(ref ifirst, 2) = (ushort)(cvid * 4 + 2);
            Unsafe.Add(ref ifirst, 3) = (ushort)(cvid * 4 + 1);
            Unsafe.Add(ref ifirst, 4) = (ushort)(cvid * 4 + 2);
            Unsafe.Add(ref ifirst, 5) = (ushort)(cvid * 4 + 3);
            cvid += 1;
        }
        batchItems.Clear();
        FlushBuffer(ref cvid);
    }

    private void FlushBuffer(ref int verticesCount)
    {
        if (verticesCount == 0) return;
        //Console.WriteLine($"try to flush: {verticesCount}");
        buffer.SetIndexData(indices.AsSpan(0, verticesCount * 6));
        buffer.SetData(vertices.AsSpan(0, verticesCount * 4));
        context.DrawIndexedPrimitives(buffer, PrimitiveType.TriangleList);
        verticesCount = 0;
    }

    // TODO cache parameter?
    private void EnsureShader(Shader shader)
    {
        ThrowHelper.ThrowIfNull(shader);
        if (this.shader != shader)
        {
            Flush();
            this.shader = shader;
            this.shader.Use();
            projectionParam = shader.GetParameter("projection"u8);
            transformParam = shader.GetParameter("extra"u8);
            shader.GetParameter("tex"u8).Set(0);

            projectionParam.Set(ref projection);
            transformParam.Set(ref transform);
        }
    }
}