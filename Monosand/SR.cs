namespace Monosand;

internal static class SR
{
    internal static Exception GameNotNewed
        => new InvalidOperationException("No 'Game' instance exists.");

    internal static Exception GameHasBeenNewed
        => new InvalidOperationException("A 'Game' instance has already existed.");

    internal static Exception PropSet(string propName)
        => new InvalidOperationException($"The '{propName}' property has been set.");

    internal static Exception PropNotSet(string propName)
        => new InvalidOperationException($"The '{propName}' property has NOT been set or inited.");
}