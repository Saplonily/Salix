#pragma once
#ifndef H_KEY_MAP
#define H_KEY_MAP
#include <cstdint>
#include <WinUser.h>

enum class Key : int32_t
{
    Unknown, Esc,
    F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
    F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    Num1, Num2, Num3, Num4, Num5, Num6, Num7, Num8, Num9, Num0,
    Backquote,
    Tilde = Backquote,
    Minus,
    Hyphen = Minus,
    Equals,
    Plus = Equals,
    Backslash,
    VerticalBar = Backslash,
    LeftSquareBracket,
    LeftCurlyBracket = LeftSquareBracket,
    RightSquareBracket,
    RightCurlyBracket = RightSquareBracket,
    Semicolon,
    Colon = Semicolon,
    Apostrophe,
    Quotation = Apostrophe,
    Comma,
    LessThan = Comma,
    Period,
    Dot = Period,
    GreateThan = Period,
    Slash,
    Question = Slash,
    Backspace, Tab, CapsLock,
    LeftShift, RightShift,
    LeftCtrl, RightCtrl,
    LeftAlt, RightAlt,
    Enter,
    Space,
    Menu,
    Application = Menu,
    LeftWin, RightWin,
    LeftSuper = LeftWin, RightSuper = RightWin,
    Left, Up, Right, Down,
    NumLock,
    NumPadMinus,
    NumPadSlash,
    NumPadMultiply,
    NumPadAsterisk = NumPadMultiply,
    NumPadStar = NumPadMultiply,
    NumPadPlus, NumPadEnter,
    NumPadPeriod,
    NumPadDot = NumPadPeriod,
    NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9, NumPad0,
    Insert, Home, PageUp, Delete, End, PageDown,
    ScrollLock, PauseBreak, PrintScreen,

    // mac
    LeftCommand, RightCommand, Control, LeftOption, RightOption
};

inline Key vkCode_to_Key(WORD vkCode)
{
#define make_case(vkCode, key) case vkCode: return key
    switch (vkCode)
    {
        make_case(VK_BACK, Key::Backspace);
        make_case(VK_TAB, Key::Tab);
        make_case(VK_RETURN, Key::Enter);
        make_case(VK_PAUSE, Key::PauseBreak);
        make_case(VK_CAPITAL, Key::CapsLock);
        make_case(VK_ESCAPE, Key::Esc);
        make_case(VK_SPACE, Key::Space);
        make_case(VK_PRIOR, Key::PageUp);
        make_case(VK_NEXT, Key::PageDown);
        make_case(VK_END, Key::End);
        make_case(VK_HOME, Key::Home);
        make_case(VK_LEFT, Key::Left);
        make_case(VK_UP, Key::Up);
        make_case(VK_RIGHT, Key::Right);
        make_case(VK_DOWN, Key::Down);
        make_case(VK_INSERT, Key::Insert);
        make_case(VK_DELETE, Key::Delete);
        make_case(0x30, Key::Num0);
        make_case(0x31, Key::Num1);
        make_case(0x32, Key::Num2);
        make_case(0x33, Key::Num3);
        make_case(0x34, Key::Num4);
        make_case(0x35, Key::Num5);
        make_case(0x36, Key::Num6);
        make_case(0x37, Key::Num7);
        make_case(0x38, Key::Num8);
        make_case(0x39, Key::Num9);
        make_case(0x41, Key::A);
        make_case(0x42, Key::B);
        make_case(0x43, Key::C);
        make_case(0x44, Key::D);
        make_case(0x45, Key::E);
        make_case(0x46, Key::F);
        make_case(0x47, Key::G);
        make_case(0x48, Key::H);
        make_case(0x49, Key::I);
        make_case(0x4A, Key::J);
        make_case(0x4B, Key::K);
        make_case(0x4C, Key::L);
        make_case(0x4D, Key::M);
        make_case(0x4E, Key::N);
        make_case(0x4F, Key::O);
        make_case(0x50, Key::P);
        make_case(0x51, Key::Q);
        make_case(0x52, Key::R);
        make_case(0x53, Key::S);
        make_case(0x54, Key::T);
        make_case(0x55, Key::U);
        make_case(0x56, Key::V);
        make_case(0x57, Key::W);
        make_case(0x58, Key::X);
        make_case(0x59, Key::Y);
        make_case(0x5A, Key::Z);
        make_case(VK_LWIN, Key::LeftWin);
        make_case(VK_RWIN, Key::RightWin);
        make_case(VK_APPS, Key::Application);
        make_case(VK_NUMPAD0, Key::NumPad0);
        make_case(VK_NUMPAD1, Key::NumPad1);
        make_case(VK_NUMPAD2, Key::NumPad2);
        make_case(VK_NUMPAD3, Key::NumPad3);
        make_case(VK_NUMPAD4, Key::NumPad4);
        make_case(VK_NUMPAD5, Key::NumPad5);
        make_case(VK_NUMPAD6, Key::NumPad6);
        make_case(VK_NUMPAD7, Key::NumPad7);
        make_case(VK_NUMPAD8, Key::NumPad8);
        make_case(VK_NUMPAD9, Key::NumPad9);
        make_case(VK_MULTIPLY, Key::NumPadMultiply);
        make_case(VK_ADD, Key::NumPadPlus);
        make_case(VK_SEPARATOR, Key::NumPadEnter);
        make_case(VK_SUBTRACT, Key::NumPadMinus);
        make_case(VK_DECIMAL, Key::NumPadDot);
        make_case(VK_DIVIDE, Key::Slash);
        make_case(VK_F1, Key::F1);
        make_case(VK_F2, Key::F2);
        make_case(VK_F3, Key::F3);
        make_case(VK_F4, Key::F4);
        make_case(VK_F5, Key::F5);
        make_case(VK_F6, Key::F6);
        make_case(VK_F7, Key::F7);
        make_case(VK_F8, Key::F8);
        make_case(VK_F9, Key::F9);
        make_case(VK_F10, Key::F10);
        make_case(VK_F11, Key::F11);
        make_case(VK_F12, Key::F12);
        make_case(VK_F13, Key::F13);
        make_case(VK_F14, Key::F14);
        make_case(VK_F15, Key::F15);
        make_case(VK_F16, Key::F16);
        make_case(VK_F17, Key::F17);
        make_case(VK_F18, Key::F18);
        make_case(VK_F19, Key::F19);
        make_case(VK_F20, Key::F20);
        make_case(VK_F21, Key::F21);
        make_case(VK_F22, Key::F22);
        make_case(VK_F23, Key::F23);
        make_case(VK_F24, Key::F24);
        make_case(VK_NUMLOCK, Key::NumLock);
        make_case(VK_SCROLL, Key::ScrollLock);
        make_case(VK_LSHIFT, Key::LeftShift);
        make_case(VK_RSHIFT, Key::RightShift);
        make_case(VK_LCONTROL, Key::LeftCtrl);
        make_case(VK_RCONTROL, Key::RightCtrl);
        make_case(VK_LMENU, Key::LeftAlt);
        make_case(VK_RMENU, Key::RightAlt);
        make_case(VK_OEM_1, Key::Colon);
        make_case(VK_OEM_PLUS, Key::Equals);
        make_case(VK_OEM_COMMA, Key::Comma);
        make_case(VK_OEM_MINUS, Key::Minus);
        make_case(VK_OEM_PERIOD, Key::Dot);
        make_case(VK_OEM_2, Key::Question);
        make_case(VK_OEM_3, Key::Tilde);
        make_case(VK_OEM_4, Key::LeftCurlyBracket);
        make_case(VK_OEM_5, Key::Backslash);
        make_case(VK_OEM_6, Key::RightCurlyBracket);
        make_case(VK_OEM_7, Key::Quotation);
        make_case(VK_CLEAR, Key::NumPad5);
        make_case(VK_SNAPSHOT, Key::PrintScreen);
    default: return Key::Unknown;
    }
#undef make_case
}

#endif