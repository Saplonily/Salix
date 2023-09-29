using System.Diagnostics.CodeAnalysis;

namespace Monosand;

internal static class SR
{
    internal static InvalidOperationException GameNotNewed
        => new InvalidOperationException("No 'Game' instance exists.");

    internal static InvalidOperationException GameHasBeenNewed
        => new InvalidOperationException("A 'Game' instance has already existed.");

    internal static InvalidOperationException PropSet(string propName)
        => new InvalidOperationException($"The '{propName}' property has been set.");

    internal static InvalidOperationException PropNotSet(string propName)
        => new InvalidOperationException($"The '{propName}' property has NOT been set or inited.");

    internal static void EnsureNotDisposed([NotNull] object? obj, string objName)
    {
        if (obj is null)
            throw new ObjectDisposedException(objName);
    }
}