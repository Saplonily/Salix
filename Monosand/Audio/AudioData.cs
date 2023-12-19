namespace Monosand;

public sealed unsafe class AudioData : IDisposable
{
    private readonly Platform platform;
    private UnmanagedMemory pcmBuffer;
    private AudioFormat format;
    private long framesCount;

    public long FramesCount { get { EnsureState(); return framesCount; } }

    public AudioFormat Format { get { EnsureState(); return format; } }

    public byte ChannelsCount => Format.ChannelsCount;

    public int SampleRate => Format.SampleRate;

    public double DurationSeconds => (double)FramesCount / Format.SampleRate;

    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);

    public bool IsDisposed => pcmBuffer.IsEmpty;

    [CLSCompliant(false)] public byte* RawData => pcmBuffer.Pointer;

    public nint RawDataSize => pcmBuffer.Size;

    public AudioData(Platform platform, ReadOnlySpan<byte> audioData)
    {
        this.platform = platform;
        pcmBuffer = platform.LoadAudio(audioData, out framesCount, out format);
    }

    public void Dispose()
    {
        if (!pcmBuffer.IsEmpty)
        {
            platform.FreeAudio(pcmBuffer);
            pcmBuffer = UnmanagedMemory.Empty;
            framesCount = 0;
            format = AudioFormat.Empty;
            GC.SuppressFinalize(this);
        }
    }

    ~AudioData() => Dispose();

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(pcmBuffer.IsEmpty, this);
}