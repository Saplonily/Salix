using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Monosand;

/// <summary>Represents a PCM audio format.</summary>
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("Channels = {ChannelsCount}, SampleRate = {SampleRate}Hz , BitDepth = {BitDepth}, IsFloatFormat = {IsFloatFormat}")]
public readonly struct AudioFormat : IEquatable<AudioFormat>
{
    private readonly int sampleRate;
    private readonly byte channelsCount;
    private readonly byte bitDepth;
    private readonly byte isFloat;

    public readonly int SampleRate => sampleRate;
    public readonly byte ChannelsCount => channelsCount;
    public readonly byte BitDepth => bitDepth;
    public readonly bool IsFloatFormat => isFloat != 0;

    public static AudioFormat Empty => new(0, 0, 0, false);

    public readonly bool IsEmpty => sampleRate == 0 && bitDepth == 0 && channelsCount == 0 && isFloat == 0;

    public AudioFormat(int sampleRate, byte channelsCount, byte bitDepth, bool isFloat)
        => (this.sampleRate, this.channelsCount, this.bitDepth, this.isFloat) =
        (sampleRate, channelsCount, bitDepth, isFloat ? (byte)1 : (byte)0);

    public unsafe static AudioFormat FromType<T>(int sampleRate, byte channels) where T : unmanaged
    {
        if (
            typeof(T) == typeof(long) ||
            typeof(T) == typeof(int) ||
            typeof(T) == typeof(short) ||
            typeof(T) == typeof(byte)
            )
            return new(sampleRate, channels, (byte)(sizeof(T) * 8), false);
        else if (
            typeof(T) == typeof(float) ||
            typeof(T) == typeof(double)
#if NET5_0_OR_GREATER
            || typeof(T) == typeof(Half)
#endif
            )
            return new(sampleRate, channels, (byte)(sizeof(T) * 8), true);
        else
            throw new NotSupportedException();
    }

    public readonly override bool Equals(object? obj)
        => obj is AudioFormat format && Equals(format);

    public readonly bool Equals(AudioFormat other)
        => sampleRate == other.sampleRate &&
           channelsCount == other.channelsCount &&
           bitDepth == other.bitDepth &&
           isFloat == other.isFloat;

    public readonly override int GetHashCode()
        => HashCode.Combine(sampleRate, channelsCount, bitDepth, isFloat);

    public static bool operator ==(AudioFormat left, AudioFormat right)
        => left.Equals(right);

    public static bool operator !=(AudioFormat left, AudioFormat right)
        => !(left == right);
}