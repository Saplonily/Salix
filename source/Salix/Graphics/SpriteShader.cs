using System.Numerics;

namespace Saladim.Salix;

public sealed class SpriteShader
{
    private readonly Shader shader;

    private readonly ShaderParameter paramTrans2d;
    private readonly ShaderParameter paramProj2d;
    private readonly ShaderParameter paramTex;

    public Shader Shader => shader;

    public SpriteShader(Shader shader)
    {
        ThrowHelper.ThrowIfNull(shader);
        this.shader = shader;

        paramTex = shader.GetParameter("tex"u8);
        if (paramTex.IsInvalid) throw ShaderParamNotFound("tex");

        paramTrans2d = shader.GetParameter("trans2d"u8);
        if (paramTrans2d.IsInvalid) throw ShaderParamNotFound("trans2d");

        paramProj2d = shader.GetParameter("proj2d"u8);
        if (paramProj2d.IsInvalid) throw ShaderParamNotFound("proj2d");
    }

    public void SetTransform2D(Matrix3x2 transform2d)
        => paramTrans2d.Set(transform2d);

    public void SetProjection2D(Matrix3x2 projection2d)
        => paramProj2d.Set(projection2d);

    public void SetTextureIndex(int value)
        => paramTex.Set(value);

    public void Use()
        => shader.Use();

    internal static ArgumentException ShaderParamNotFound(string paramName)
        => new ArgumentException(string.Format(SR.ShaderParamNotFound, paramName));
}