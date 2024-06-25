using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if NET7_0_OR_GREATER
#pragma warning disable CA1513
#endif

namespace Salix;

[StackTraceHidden]
internal static class ThrowHelper
{
    /// <summary>Throws <see cref="ObjectDisposedException"/> when <paramref name="condition"/> is true.</summary>
    public static void ThrowIfDisposed([DoesNotReturnIf(true)] bool condition, object instance)
    {
        if (condition) throw new ObjectDisposedException(instance.GetType().FullName);
    }

    /// <summary>Throws <see cref="ArgumentNullException"/> when <paramref name="argument"/> is null.</summary>
    public static void ThrowIfNull(object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null) throw new ArgumentNullException(paramName);
    }

    /// <summary>Throws <see cref="InvalidOperationException"/> when <paramref name="condition"/> is true.</summary>
    public static void ThrowIfInvalid(
        [DoesNotReturnIf(true)] bool condition,
        string message
        )
    {
        if (condition) throw new InvalidOperationException(message);
    }
}