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

    private vpct[] vertices;
    private ushort[] indices;
    private VertexBuffer<vpct> buffer;
    private List<BatchItem> batchItems;
    private RenderContext context;

    public SpriteBatch(RenderContext context)
    {
        this.context = context;
        batchItems = new();
        vertices = new vpct[16 * 4];
        indices = new ushort[16 * 6];
        buffer = new(vpct.VertexDeclaration, VertexBufferDataUsage.StreamDraw, true);
    }

    public void DrawTexture(Texture2D texture, Vector2 position, Color color)
        => DrawTexture(texture, position, Vector2.One, color);

    public void DrawTexture(Texture2D texture, Vector2 position, Vector2 scale, Color color)
    {
        float w = texture.Width * scale.X;
        float h = texture.Height * scale.Y;
        Vector2 tl = position;
        Vector2 tr = position + new Vector2(w, 0);
        Vector2 br = position + new Vector2(w, h);
        Vector2 bl = position + new Vector2(0, h);
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

    public void Flush()
    {
        int batchItemsCount = batchItems.Count;
        if (batchItemsCount == 0) return;

        Texture2D? currentTex = batchItems[0].tex;
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
        Console.WriteLine($"try to flush: {verticesCount}");
        buffer.SetIndexData(indices.AsSpan(0, verticesCount * 6));
        buffer.SetData(vertices.AsSpan(0, verticesCount * 4));
        context.DrawIndexedPrimitives(buffer, PrimitiveType.TriangleList);
        verticesCount = 0;
    }
}