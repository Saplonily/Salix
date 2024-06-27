using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Salix;

internal static partial class Interop
{
    private const string LibName = "slx";
    private const CallingConvention CallConv = CallingConvention.StdCall;

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
    internal struct RECT { public int left, top, right, bottom; }

    internal struct NBool
    {
        public byte Value;

        public static implicit operator bool(NBool @bool)
            => @bool.Value != 0;

        public static implicit operator NBool(bool @bool)
            => new NBool() { Value = @bool ? (byte)1 : (byte)0 };

        public static bool operator true(NBool @bool)
            => @bool.Value != 0;

        public static bool operator false(NBool @bool)
            => @bool.Value == 0;
    }

    [DoesNotReturn, DebuggerHidden, StackTraceHidden]
    public static void Throw()
    {
        ErrorCode err = SLX_GetError();
        if (err != ErrorCode.OK)
            throw new FrameworkException(err);
        else
            throw new InvalidOperationException(SR.ThrowOnOK);
    }
}
