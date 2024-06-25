using System.Numerics;

namespace Salix;

public struct DrawTransform
{
    public Vector2 Position;
    public Vector2 Origin;
    public Vector2 Scale = Vector2.One;
    public float Radians;

    public static DrawTransform None => new();

    public DrawTransform()
    { }

    public DrawTransform(Vector2 position)
        => Position = position;

    public DrawTransform(Vector2 position, Vector2 scale)
        => (Position, Scale) = (position, scale);

    public DrawTransform(Vector2 position, float radians)
        => (Position, Radians) = (position, radians);

    public DrawTransform(Vector2 position, Vector2 origin, Vector2 scale)
        => (Position, Origin, Scale) = (position, origin, scale);

    public DrawTransform(Vector2 position, Vector2 origin, float radians)
        => (Position, Origin, Radians) = (position, origin, radians);

    public DrawTransform(Vector2 position, Vector2 origin, Vector2 scale, float radians)
        => (Position, Origin, Scale, Radians) = (position, origin, scale, radians);

    public readonly Matrix3x2 BuildMatrix(Vector2 size)
        => Matrix3x2.CreateTranslation(-size * Origin) *
           Matrix3x2.CreateRotation(Radians) *
           Matrix3x2.CreateScale(Scale) *
           Matrix3x2.CreateTranslation(Position);
}