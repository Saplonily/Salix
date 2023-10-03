namespace Monosand;

/// <summary>
/// <para>Represents a Key.</para>
/// <para>Note that for example, the <see cref="Plus"/> and the <see cref="Equals"/>
/// is the same <see cref="Key"/>, because they are the same physical Key.</para>
/// </summary>
public enum Key : int
{
    /**<summary>Unknown key.</summary>*/
    Unknown, Esc,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9, Num0,
    /**<summary>`</summary>*/
    Backquote,
    /**<summary>~</summary>*/
    Tilde = Backquote,
    /**<summary>-</summary>*/
    Minus,
    /**<summary>-</summary>*/
    Hyphen = Minus,
    /**<summary>=</summary>*/
    Equals,
    /**<summary>+</summary>*/
    Plus = Equals,
    /**<summary>\</summary>*/
    Backslash,
    /**<summary>|</summary>*/
    VerticalBar = Backslash,
    /**<summary>[</summary>*/
    LeftSquareBracket,
    /**<summary>{</summary>*/
    LeftCurlyBracket = LeftSquareBracket,
    /**<summary>]</summary>*/
    RightSquareBracket,
    /**<summary>}</summary>*/
    RightCurlyBracket = RightSquareBracket,
    /**<summary>;</summary>*/
    Semicolon,
    /**<summary>:</summary>*/
    Colon = Semicolon,
    /**<summary>'</summary>*/
    Apostrophe,
    /**<summary>"</summary>*/
    Quotation = Apostrophe,
    /**<summary>,</summary>*/
    Comma,
    /**<summary>&lt;</summary>*/
    LessThan = Comma,
    /**<summary>.</summary>*/
    Period,
    /**<summary>.</summary>*/
    Dot = Period,
    /**<summary>&gt;</summary>*/
    GreateThan = Period,
    /**<summary>/</summary>*/
    Slash,
    /**<summary>?</summary>*/
    Question = Slash,
    Backspace, Tab, CapsLock,
    LeftShift, RightShift,
    LeftCtrl, RightCtrl,
    LeftAlt, RightAlt,
    Enter,
    Space,
    /**<summary>Usually the key between the <see cref="RightCtrl"/> and the <see cref="RightWin"/>.</summary>*/
    Menu,
    /**<inheritdoc cref="Menu"/>*/
    Application = Menu,
    LeftWin, RightWin,
    LeftSuper = LeftWin, RightSuper = RightWin,
    Left, Up, Right, Down,
    NumLock,
    NumPadMinus,
    /**<summary>/</summary>*/
    NumPadSlash,
    /**<summary>*</summary>*/
    NumPadMultiply,
    /**<summary>*</summary>*/
    NumPadAsterisk = NumPadMultiply,
    /**<summary>*</summary>*/
    NumPadStar = NumPadMultiply,
    /**<summary>+</summary>*/
    NumPadPlus, NumPadEnter,
    /**<summary>.</summary>*/
    NumPadPeriod,
    /**<summary>.</summary>*/
    NumPadDot = NumPadPeriod,
    NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9, NumPad0,
    Insert, Home, PageUp, Delete, End, PageDown,
    ScrollLock, PauseBreak, PrintScreen,

    // mac
    LeftCommand, RightCommand, Control, LeftOption, RightOption
}