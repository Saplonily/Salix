using System.Numerics;

namespace Monosand;

public sealed class SpriteShader
{
    private readonly Shader shader;

    private readonly ShaderParameter paramTrans2d;
    private readonly ShaderParameter paramProj2d;
    private readonly ShaderParameter paramTex;
    private readonly ShaderParameter paramIsDrawingText;

    public Shader Shader => shader;

    public SpriteShader(Shader shader)
    {
        ThrowHelper.ThrowIfNull(shader);
        this.shader = shader;
        var pshader = shader.RenderContext.Shader;
        shader.Use();
        paramTex = shader.GetParameter("tex"u8);
        if (paramTex.IsInvalid) throw SR.ShaderParamNotFound("tex");

        paramTrans2d = shader.GetParameter("trans2d"u8);
        if (paramTrans2d.IsInvalid) throw SR.ShaderParamNotFound("trans2d");

        paramProj2d = shader.GetParameter("proj2d"u8);
        if (paramProj2d.IsInvalid) throw SR.ShaderParamNotFound("proj2d");

        paramIsDrawingText = shader.GetParameter("isDrawingText"u8);
        if (paramIsDrawingText.IsInvalid) throw SR.ShaderParamNotFound("isDrawingText");
        shader.RenderContext.Shader = pshader;
    }

    public void SetTransform2D(Matrix3x2 transform2d)
        => paramTrans2d.Set(transform2d);

    public void SetProjection2D(Matrix3x2 projection2d)
        => paramProj2d.Set(projection2d);

    public void SetIsDrawingText(bool value)
        => paramIsDrawingText.Set(value);

    public void SetTextureIndex(int value)
        => paramTex.Set(value);

    public void Use()
        => shader.Use();
}