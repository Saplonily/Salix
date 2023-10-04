using System.Collections;
using System.Runtime.InteropServices;

namespace Monosand;

public sealed class KeyboardState
{
    private static Window? window;
    private BitArray bitArrayCurrent;
    private BitArray bitArrayPrevious;

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
        bitArrayCurrent = new(128);
        bitArrayPrevious = new(128);
    }

    // TODO optimize with Vector128?
    internal void SetTrue(Key key) => bitArrayCurrent[(int)key] = true;
    internal void SetFalse(Key key) => bitArrayCurrent[(int)key] = false;
    internal void Swap()
    {
        BitArray temp = bitArrayCurrent;
        bitArrayCurrent = bitArrayPrevious;
        bitArrayPrevious = temp;
        bitArrayCurrent = (BitArray)bitArrayPrevious.Clone();
    }
    internal void Clear() => bitArrayCurrent.SetAll(false);

    public bool IsPressing(Key key) => bitArrayCurrent[(int)key];
    public bool IsJustPressed(Key key) => bitArrayCurrent[(int)key] && !bitArrayPrevious[(int)key];
    public bool IsJustReleased(Key key) => !bitArrayCurrent[(int)key] && bitArrayPrevious[(int)key];
}