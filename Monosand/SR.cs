namespace Monosand;

internal static class SR
{
    internal static InvalidOperationException GameNotNewed
        => new("No 'Game' instance exists.");

    internal static InvalidOperationException GameHasBeenNewed
        => new("A 'Game' instance has already existed.");

    internal static InvalidOperationException PropSet(string propName)
        => new($"The '{propName}' property has been set.");

    internal static InvalidOperationException PropNotSet(string propName)
        => new($"The '{propName}' property has NOT been set or inited.");

    internal static ArgumentException ShaderParamNotFound(string paramName)
        => new ArgumentException($"Shader parameter '{paramName}' does not exist.");
}