using System;
using System.Runtime.InteropServices;

namespace Monosand;

public partial class Window
{
    private enum event_type : int
    {
        close = 1,
        move,
        resize,
        key_down,
        key_up,
        got_focus,
        lost_focus,
        mouse,
        mouse_wheel
    };

    [StructLayout(LayoutKind.Explicit)]
    private struct win_event
    {
        [FieldOffset(0)] public event_type type;
        [FieldOffset(4)] public int arg1;
        [FieldOffset(8)] public int arg2;
        [FieldOffset(12)] public int arg3;
        [FieldOffset(12)] public short arg3_left;
        [FieldOffset(14)] public short arg3_right;
        [FieldOffset(16)] public IntPtr gc_handle;
    };

    internal unsafe void PollEvents()
    {
        EnsureState();
        Interop.MsdPollEvents(nativeHandle);
        win_event* pevent;
        void* handle = (win_event*)Interop.MsdBeginProcessEvents(nativeHandle, out var ncount, out var vpevent);
        pevent = (win_event*)vpevent;
        if (ncount > int.MaxValue)
            throw new FrameworkException(SR.TooManyWindowEvents);

        int count = (int)ncount;

        for (int i = 0; i < count; i++)
        {
            Window win = (Window)GCHandle.FromIntPtr(pevent[i].gc_handle).Target!;
            win_event* e = &pevent[i];
            switch (pevent[i].type)
            {
            case event_type.close:
                if (win.OnClosing())
                    Close();
                break;
            case event_type.move:
                win.OnMoved(e->arg1, e->arg2);
                break;
            case event_type.resize:
                win.OnResized(e->arg1, e->arg2);
                break;
            case event_type.key_down:
                win.OnKeyPressed((Key)e->arg1);
                break;
            case event_type.key_up:
                win.OnKeyReleased((Key)e->arg1);
                break;
            case event_type.got_focus:
                win.OnGotFocus();
                break;
            case event_type.lost_focus:
                win.OnLostFocus();
                break;
            case event_type.mouse:
            {
                int x = e->arg1, y = e->arg2;
                short btnType = e->arg3_left;
                MouseButton button = (MouseButton)btnType;
                short downType = e->arg3_right;
                if (downType == 0)
                    win.OnMouseButtonPressed(x, y, button);
                else if (downType == 1)
                    win.OnMouseButtonReleased(x, y, button);
                else if (downType == 2 && button == MouseButton.None)
                    win.OnMouseMoved(x, y);
                break;
            }
            case event_type.mouse_wheel:
                win.OnMouseWheelMoved(pevent->arg1);
                break;

            default: throw new FrameworkException(string.Format(SR.UnknownWindowEventType, pevent[i]));
            }
            Console.WriteLine($"{e->type} {e->arg1}, {e->arg2}, {e->arg3}|({e->arg3_left}/{e->arg3_right})");
        }

        Interop.MsdEndProcessEvents(nativeHandle, handle);
    }
}
