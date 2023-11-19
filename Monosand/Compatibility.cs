#if NETSTANDARD2_0

namespace System
{
    internal static class MathF
    {
        public const float PI = 3.14159274F;

        public static float Sin(float x)
            => (float)Math.Sin(x);

        public static float Cos(float x)
            => (float)Math.Cos(x);

        public static float Floor(float x)
            => (int)x;
    }
}

#endif