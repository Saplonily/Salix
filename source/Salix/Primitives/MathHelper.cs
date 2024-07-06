﻿using System.Numerics;

namespace Saladim.Salix;

public static class MathHelper
{
    public static float Lerp(float from, float to, float weight)
        => from * (1.0f - weight) + to * weight;

    public static double Lerp(double from, double to, double weight)
        => from * (1.0f - weight) + to * weight;

    // TODO remove these tool methods after making our own Vector2

    public static Vector2 Floored(this Vector2 vector)
        => new(MathF.Floor(vector.X), MathF.Floor(vector.Y));
}