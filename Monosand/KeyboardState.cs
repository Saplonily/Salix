using System.Collections;

namespace Monosand;

public sealed class KeyboardState
{
    private static Window? window;
    private readonly BitArray bitArray;

    public static KeyboardState Current
    {
        get
        {
            ThrowHelper.ThrowIfInvalid(window is null, "No window created for getting the KeyboardState.");
            return window.KeyboardState;
        }
    }

    internal KeyboardState(Window window)
    {
        KeyboardState.window = window;
        bitArray = new(128);
    }

    internal void SetTrue(Key key) => bitArray[(int)key] = true;
    internal void SetFalse(Key key) => bitArray[(int)key] = false;

    public bool IsPressing(Key key) => bitArray[(int)key];
}