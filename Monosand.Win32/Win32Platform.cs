using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand.Win32;

public unsafe partial class Win32Platform : Platform
{
    public static bool inited = false;
    public Win32Platform() { }

    internal override void Init()
    {
        if (!inited)
        {
            // TODO error handle
            Win32WinImpl.InitMsgCallbacks();
            inited = true;
            int a = Interop.MsdInit();
            if (a != 0)
                throw new OperationFailedException("Interop.MsdInit() return non-zero value.");
        }
    }

    internal override WinImpl CreateWindowImpl(int width, int height, string title, Window window)
        => new Win32WinImpl(width, height, title, window);
}