using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand;

public sealed class KeyboardState
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    private unsafe struct Bits128
    {
        private fixed int value[4];

        public bool this[int index] { get => GetBit(index); set => SetBit(index, value); }

        public void SetBit(int index, bool bit)
        {
            if (index is < 0 or >= 128)
                throw new IndexOutOfRangeException();

            int num = 1 << index;
            ref int ptr = ref value[index >> 5];
            if (bit) ptr |= num; else ptr &= ~num;
        }

        public bool GetBit(int index)
        {
            if (index is < 0 or >= 128)
                throw new IndexOutOfRangeException();
            return (value[index >> 5] & (1 << index)) is not 0;
        }

        public void SetAll(bool bit)
        {
            int i = bit ? -1 : 0;
            value[0] = i;
            value[1] = i;
            value[2] = i;
            value[3] = i;
        }
    }

    private static Window? window;
    private Bits128 bitArrayCurrent;
    private Bits128 bitArrayPrevious;

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
        bitArrayCurrent = new();
        bitArrayPrevious = new();
    }

    internal void SetTrue(Key key) => bitArrayCurrent[(int)key] = true;
    internal void SetFalse(Key key) => bitArrayCurrent[(int)key] = false;
    internal void Update()
    {
        bitArrayPrevious = bitArrayCurrent;
    }

    internal void Clear() => bitArrayCurrent.SetAll(false);

    public bool IsPressing(Key key) => bitArrayCurrent[(int)key];
    public bool IsJustPressed(Key key) => bitArrayCurrent[(int)key] && !bitArrayPrevious[(int)key];
    public bool IsJustReleased(Key key) => !bitArrayCurrent[(int)key] && bitArrayPrevious[(int)key];
}