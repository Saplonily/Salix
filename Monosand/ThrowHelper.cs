using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monosand;

internal static class ThrowHelper
{
    /// <summary>Throws <see cref="ObjectDisposedException"/> when <paramref name="condition"/> is true.</summary>
    public static void ThrowIfDisposed([DoesNotReturnIf(true)] bool condition, object instance)
    {
#if NET7_0_OR_GREATER
        ObjectDisposedException.ThrowIf(condition, instance);
#else
        if (condition) throw new ObjectDisposedException(instance.GetType().FullName);
#endif
    }

    /// <summary>Throws <see cref="ArgumentNullException"/> when <paramref name="argument"/> is null.</summary>
    public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null) throw new ArgumentNullException(paramName);
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="argument"/> is negative.</summary>
    public static void ThrowIfNegative(int argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument < 0) throw new ArgumentOutOfRangeException(paramName, "Can't be negative.");
    }

    /// <summary>Throws <see cref="ArgumentOutOfRangeException"/> when <paramref name="argument"/> is negative.</summary>
    public static void ThrowIfNegative(float argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument < 0) throw new ArgumentOutOfRangeException(paramName, "Can't be negative.");
    }

    /// <summary>Throws <see cref="InvalidOperationException"/> when <paramref name="condition"/> is true.</summary>
    public static void ThrowIfInvalid(
        [DoesNotReturnIf(true)] bool condition,
        string? msg = null,
        [CallerArgumentExpression(nameof(condition))] string? conditionExpression = null
        )
    {
        if (condition) throw new InvalidOperationException($"{msg} {conditionExpression}");
    }

    public static void ThrowIfArgInvalid(
        [DoesNotReturnIf(true)] bool condition,
        string paramName,
        [CallerArgumentExpression(nameof(condition))] string? conditionStr = null
        )
    {
        if (condition) throw new ArgumentException(conditionStr, paramName);
    }
}