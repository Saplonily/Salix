using System.Numerics;

namespace Monosand;

public class SpriteEffect : Effect
{
    protected ShaderParameter paramTex;
    protected ShaderParameter paramTrans;
    protected ShaderParameter paramTrans2d;
    protected ShaderParameter paramProjection;
    protected ShaderParameter paramIsDrawingText;

    public SpriteEffect(Shader shader)
        : base(shader)
    {
        var pshader = shader.RenderContext.Shader;
        shader.Use();
        paramTex = shader.GetParameter("tex"u8);
        if (!paramTex.Valid) throw SR.ShaderParamNotFound("tex");

        paramTrans = shader.GetParameter("trans"u8);
        if (!paramTrans.Valid) throw SR.ShaderParamNotFound("trans");

        paramProjection = shader.GetParameter("projection"u8);
        if (!paramProjection.Valid) throw SR.ShaderParamNotFound("projection");

        paramTrans2d = shader.GetParameter("trans2d"u8);
        if (!paramTrans2d.Valid) throw SR.ShaderParamNotFound("trans2d");

        paramIsDrawingText = shader.GetParameter("isDrawingText"u8);
        if (!paramIsDrawingText.Valid) throw SR.ShaderParamNotFound("isDrawingText");
        shader.RenderContext.Shader = pshader;

        
    }

    public void SetTextureIndex(int value)
        => paramTex.Set(value);

    public void SetTransform(ref Matrix4x4 transform)
        => paramTrans.Set(ref transform);

    public void SetTransform2D(ref Matrix3x2 transform2d)
        => paramTrans2d.Set(ref transform2d);

    public void SetProjection(ref Matrix4x4 projection)
        => paramProjection.Set(ref projection);

    public void SetIsDrawingText(bool value)
        => paramIsDrawingText.Set(value);
}