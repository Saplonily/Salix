using System.Numerics;

namespace Monosand;

public sealed class PointerState
{
    // 1 << 0: left bitmask
    // 1 << 1: right bitmask
    // 1 << 2: middle bitmask
    private int state;
    private int statePrevious;
    private Vector2 position;
    private Vector2 positionPrevious;
    private float wheelOffset;
    private float wheelOffsetPrevious;
    private static Window? window;

    public static PointerState Current
    {
        get
        {
            ThrowHelper.ThrowIfInvalid(window is null, "No window created for getting the KeyboardState.");
            return window.PointerState;
        }
    }

    public Vector2 Position => position;
    public Vector2 PositionPrevious => positionPrevious;
    public Vector2 PositionDelta => position - positionPrevious;
    public float WheelOffset => wheelOffset;
    public float WheelOffsetPrevious => wheelOffsetPrevious;
    public float WheelDelta => wheelOffset - wheelOffsetPrevious;
    public bool IsLeftButtonPressing => IsPressing(PointerButton.Left);
    public bool IsRightButtonPressing => IsPressing(PointerButton.Right);
    public bool IsMiddleButtonPressing => IsPressing(PointerButton.Middle);

    internal PointerState(Window window)
    {
        PointerState.window = window;
        Clear();
    }

    internal void SetTrue(int bit)
        => state |= bit;

    internal void SetFalse(int bit)
        => state &= ~bit;

    internal void SetPosition(Vector2 pos)
        => position = pos;

    internal void AddWheelDelta(float delta)
        => wheelOffset += delta;

    internal void Update()
    {
        statePrevious = state;
        positionPrevious = position;
        wheelOffsetPrevious = wheelOffset;
    }

    internal void Clear()
    {
        state = statePrevious = 0;
        wheelOffset = wheelOffsetPrevious = 0f;
    }

    public bool IsPressing(PointerButton button)
        => (state & (1 << (int)button)) != 0;

    public bool IsJustPressed(PointerButton button)
        => (state & (1 << (int)button)) != 0 && (statePrevious & (1 << (int)button)) == 0;

    public bool IsJustReleased(PointerButton button)
        => (state & (1 << (int)button)) == 0 && (statePrevious & (1 << (int)button)) != 0;
}