using System;
using System.Drawing;
using System.Numerics;
using System.Reflection;
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

    private Shader shader = null!;
    private ShaderParameter projectionParam;
    private ShaderParameter extraMatParam;
    private Matrix4x4 transform;
    private vpct[] vertices;
    private ushort[] indices;

    public RenderContext RenderContext => context;
    public Shader Shader
    {
        get => shader;
        set
        {
            shader = value;
            shader.Use();
            projectionParam = value.GetParameter("projection"u8);
            extraMatParam = value.GetParameter("extra"u8);
            value.GetParameter("tex"u8).Set(0);
        }
    }

    public Matrix4x4 Transform
    {
        get => transform;
        set { transform = value; extraMatParam.Set(ref value); }
    }

    /// <summary>
    /// Construct a <see cref="SpriteBatch"/>.
    /// </summary>
    /// <param name="context">The <see cref="RenderContext"/> that this <see cref="SpriteBatch"/> will draw to.</param>
    /// <param name="spriteShader">The default <see cref="Shader"/> will be used.
    /// If set to <see langword="null"/>, then glsl shader "SpriteShader.frag" "SpriteShader.vert" will be loaded and used.</param>
    public SpriteBatch(RenderContext context, Shader? spriteShader = null)
    {
        this.context = context;
        batchItems = new();
        vertices = new vpct[16 * 4];
        indices = new ushort[16 * 6];
        buffer = new(vpct.VertexDeclaration, VertexBufferDataUsage.StreamDraw, true);
        if (spriteShader is null)
        {
            var rl = Game.Instance.ResourceLoader;
            using var vert = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.SpriteShader.vert");
            using var frag = rl.OpenEmbeddedStream($"{nameof(Monosand)}.Embedded.SpriteShader.frag");
            Shader = rl.LoadGlslShader(vert, frag);
        }
        else
        {
            Shader = spriteShader;
        }
        Transform = Matrix4x4.Identity;
        context.ViewportChanged += Context_ViewportChanged;
        Rectangle rect = context.GetViewport();
        Context_ViewportChanged(context, rect.X, rect.Y, rect.Width, rect.Height);
    }

    private void Context_ViewportChanged(RenderContext renderContext, int x, int y, int width, int height)
    {
        Matrix4x4 mat = Matrix4x4.Identity;
        mat *= Matrix4x4.CreateTranslation(-width / 2f, -height / 2f, 0f);
        mat *= Matrix4x4.CreateScale(2f / width, -2f / height, 1f);
        projectionParam.Set(ref mat);
    }

    public void DrawTexture(Texture2D texture, Vector2 position)
        => DrawTexture(texture, position, Vector2.Zero, Vector2.One, 0f, Color.White);

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
        float sin = (float)Math.Sin(radian);
        float cos = (float)Math.Cos(radian);
#else
        float sin = MathF.Sin(radian);
        float cos = MathF.Cos(radian);
#endif

        // [ cos  -sin ]     [ x ]     [ x * cos - y * sin ]
        // [           ]  *  [   ]  =  [   ]
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
            topLeft: new(new(tl, 0f), c, new(0f, 0f)),
            topRight: new(new(tr, 0f), c, new(1f, 0f)),
            bottomLeft: new(new(bl, 0f), c, new(0f, 1f)),
            bottomRight: new(new(br, 0f), c, new(1f, 1f))
        ));
    }

    public void DrawTextureMatrix(Texture2D texture, ref Matrix3x2 matrix, Color color)
         => DrawTextureMatrix(texture, ref matrix, color, color, color, color);

    public void DrawTextureMatrix(Texture2D texture, ref Matrix3x2 matrix, Color ctl, Color ctr, Color cbl, Color cbr)
    {
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
            topLeft: new(new(tl, 0f), ctl.ToVector4(), new(0f, 0f)),
            topRight: new(new(tr, 0f), ctr.ToVector4(), new(1f, 0f)),
            bottomLeft: new(new(bl, 0f), cbl.ToVector4(), new(0f, 1f)),
            bottomRight: new(new(br, 0f), cbr.ToVector4(), new(1f, 1f))
        ));
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
                currentTex = item.tex;
                context.SetTexture(0, currentTex);
                FlushBuffer(ref cvid);
            }
            if (cvid * 4 + 4 > ushort.MaxValue)
                FlushBuffer(ref cvid);

            if (cvid * 4 + 4 >= vertices.Length)
                Array.Resize(ref vertices, cvid * 4 + 8);
            if (cvid * 6 + 6 >= indices.Length)
                Array.Resize(ref indices, cvid * 6 + 12);

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
}