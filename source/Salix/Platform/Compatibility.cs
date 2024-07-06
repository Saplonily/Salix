﻿#if NETSTANDARD2_0

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

namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

        public bool ReturnValue { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class DoesNotReturnIfAttribute : Attribute
    {
        public DoesNotReturnIfAttribute(bool parameterValue) => ParameterValue = parameterValue;

        public bool ParameterValue { get; }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue, Inherited = false)]
    internal sealed class NotNullAttribute : Attribute { }
}

#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        public CallerArgumentExpressionAttribute(string parameterName) => ParameterName = parameterName;
        public string ParameterName { get; }
    }
}

#endif

#if !NET6_0_OR_GREATER

namespace System.Diagnostics
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct, Inherited = false)]
    [Conditional("NEVER")]
    internal sealed class StackTraceHiddenAttribute : Attribute
    {
        public StackTraceHiddenAttribute() { }
    }
}

#endif