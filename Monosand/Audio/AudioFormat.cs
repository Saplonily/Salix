using System.Runtime.InteropServices;

namespace Monosand;

/// <summary>Represents a PCM audio format.</summary>
[StructLayout(LayoutKind.Sequential)]
public struct AudioFormat : IEquatable<AudioFormat>
{
    public int SampleRate;
    public short ChannelsCount;
    public short BitDepth;

    public static AudioFormat Empty => new(0, 0, 0);

    public readonly bool IsEmpty => SampleRate == 0 || ChannelsCount == 0 || BitDepth == 0;

    public AudioFormat(int sampleRate, short channels, short bitDepth)
        => (SampleRate, ChannelsCount, BitDepth) = (sampleRate, channels, bitDepth);

    public unsafe static AudioFormat FromType<T>(int sampleRate, short channels) where T : unmanaged
        => new(sampleRate, channels, (short)(sizeof(T) * 8));

    public readonly override bool Equals(object? obj)
        => obj is AudioFormat format && Equals(format);

    public readonly bool Equals(AudioFormat other)
        => SampleRate == other.SampleRate &&
           ChannelsCount == other.ChannelsCount &&
           BitDepth == other.BitDepth;

    public readonly override int GetHashCode()
        => HashCode.Combine(SampleRate, ChannelsCount, BitDepth);
    
    public static bool operator ==(AudioFormat left, AudioFormat right)
        => left.Equals(right);

    public static bool operator !=(AudioFormat left, AudioFormat right)
        => !(left == right);
}