using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Monosand;

public static class ThrowHelper
{
    public static void ThrowIfDisposed([DoesNotReturnIf(true)] bool condition, object? instance)
    {
        if (condition) throw new ObjectDisposedException(instance?.GetType().FullName);
    }

    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null) throw new ArgumentNullException(paramName);
    }

    public static void ThrowIfNegative(int argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument < 0)
            throw new ArgumentOutOfRangeException(paramName, "Can't be negative.");
    }
}