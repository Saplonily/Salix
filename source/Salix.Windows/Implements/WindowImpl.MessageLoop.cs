using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Saladim.Salix.Windows;

unsafe partial class WindowImpl
{
    private enum WindowEventType : int
    {
        Close = 1,
        Move,
        Resize,
        KeyDown,
        KeyUp,
        GotFocus,
        LostFocus,
        Mouse,
        MouseWheel
    };

    [StructLayout(LayoutKind.Explicit)]
    private struct WindowEvent
    {
        [FieldOffset(0)] public WindowEventType type;
        [FieldOffset(4)] public int arg1;
        [FieldOffset(8)] public int arg2;
        [FieldOffset(12)] public int arg3;
        [FieldOffset(12)] public short arg3Left;
        [FieldOffset(14)] public short arg3Right;
    };

    void IWindowImpl.PollEvents(Window window)
    {
        Interop.SLX_PollEvents(nativeHandle);
        WindowEvent* pevent;
        void* handle = Interop.SLX_BeginProcessEvents(nativeHandle, out var ncount, out var vpevent);
        pevent = (WindowEvent*)vpevent;
        if (ncount > int.MaxValue)
            throw new FrameworkException(SR.TooManyWindowEvents);

        int count = (int)ncount;

        for (int i = 0; i < count; i++)
        {
            WindowEvent* e = &pevent[i];
            switch (pevent[i].type)
            {
            case WindowEventType.Close:
                if (window.OnClosing())
                    window.Close();
                break;
            case WindowEventType.Move:
                window.OnMoved(e->arg1, e->arg2);
                break;
            case WindowEventType.Resize:
                window.OnResized(e->arg1, e->arg2);
                break;
            case WindowEventType.KeyDown:
                window.OnKeyPressed((Key)e->arg1);
                break;
            case WindowEventType.KeyUp:
                window.OnKeyReleased((Key)e->arg1);
                break;
            case WindowEventType.GotFocus:
                window.OnGotFocus();
                break;
            case WindowEventType.LostFocus:
                window.OnLostFocus();
                break;
            case WindowEventType.Mouse:
            {
                int x = e->arg1, y = e->arg2;
                short btnType = e->arg3Left;
                MouseButton button = (MouseButton)btnType;
                short downType = e->arg3Right;
                if (downType == 0)
                    window.OnMouseButtonPressed(x, y, button);
                else if (downType == 1)
                    window.OnMouseButtonReleased(x, y, button);
                else if (downType == 2 && button == MouseButton.None)
                    window.OnMouseMoved(x, y);
                break;
            }
            case WindowEventType.MouseWheel:
                window.OnMouseWheelMoved(pevent->arg1);
                break;

            default: throw new FrameworkException(string.Format(SR.UnknownWindowEventType, pevent[i]));
            }
        }

        Interop.SLX_EndProcessEvents(nativeHandle, handle);
    }
}
