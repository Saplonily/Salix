using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Saladim.Salix.Windows;

internal static partial class Interop
{
    private const string LibName = "slx";
    private const CallingConvention CallConv = CallingConvention.StdCall;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT { public int left, top, right, bottom; }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RenderContextInfo { public int MaxTexturesUnits; }

    [DebuggerStepThrough]
    internal struct NBool
    {
        public byte Value;

        public static implicit operator bool(NBool @bool) => @bool.Value != 0;

        public static implicit operator NBool(bool @bool) => new NBool() { Value = @bool ? (byte)1 : (byte)0 };

        public static bool operator true(NBool @bool) => @bool.Value != 0;

        public static bool operator false(NBool @bool) => @bool.Value == 0;
    }

    [DoesNotReturn, DebuggerStepThrough, StackTraceHidden]
    public static void Throw(string? message = null)
    {
        ErrorCode err = SLX_GetError();
        string msg;
        if (message is null)
            msg = GetMessageFromErrorCode(err);
        else
            msg = $"{message} {GetMessageFromErrorCode(err)}";

        if (err != ErrorCode.OK)
            throw new FrameworkException(msg, GetPlatformException());
        else
            throw new FrameworkException(SR.ThrowOnOK);
    }

    private static string GetMessageFromErrorCode(ErrorCode errorCode)
    {
        // TODO
        return $"(ErrorCode.{errorCode})";
    }

    private static Exception? GetPlatformException()
        => Marshal.GetExceptionForHR(SLX_GetPlatformError());
}
