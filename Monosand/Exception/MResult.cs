using System.Runtime.InteropServices;

namespace Monosand;

[StructLayout(LayoutKind.Sequential)]
internal struct MResult<T>
{
    public ErrorCode ErrorCode;
    public int PlatformResult;
    public T Value;

    public readonly bool OK => ErrorCode == ErrorCode.None;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MResult
{
    public ErrorCode ErrorCode;
    public int PlatformResult;

    public readonly bool OK => ErrorCode == ErrorCode.None;
}