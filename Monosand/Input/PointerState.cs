namespace Monosand;

public sealed class PointerState
{
    // 1 << 0: left bitmask
    // 1 << 1: right bitmask
    // 1 << 2: middle bitmask
    private int state;
    private int statePrevious;
    private static Window? window;

    public static PointerState Current
    {
        get
        {
            ThrowHelper.ThrowIfInvalid(window is null, "No window created for getting the KeyboardState.");
            return window.PointerState;
        }
    }

    public bool IsLeftButtonPressing => IsPressing(PointerButton.Left);
    public bool IsRightButtonPressing => IsPressing(PointerButton.Right);
    public bool IsMiddleButtonPressing => IsPressing(PointerButton.Middle);

    internal PointerState(Window window)
    {
        PointerState.window = window;
        state = statePrevious = 0;
    }

    internal void SetTrue(int bit)
        => state |= bit;

    internal void SetFalse(int bit)
        => state &= ~bit;

    internal void Update()
        => statePrevious = state;

    internal void Clear()
        => state = statePrevious = 0;

    public bool IsPressing(PointerButton button)
        => (state & (1 << (int)button)) != 0;

    public bool IsJustPressed(PointerButton button)
        => (state & (1 << (int)button)) != 0 && (statePrevious & (1 << (int)button)) == 0;

    public bool IsJustReleased(PointerButton button)
        => (state & (1 << (int)button)) == 0 && (statePrevious & (1 << (int)button)) != 0;
}