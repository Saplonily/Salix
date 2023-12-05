namespace Monosand;

public sealed unsafe class AudioData : IDisposable
{
    private readonly Platform platform;
    private UnmanagedMemory pcmBuffer;
    private AudioFormat format;
    private int samplesCount;

    public int SamplesCount { get { EnsureState(); return samplesCount; } }
    public AudioFormat Format { get { EnsureState(); return format; } }
    public byte ChannelsCount => Format.ChannelsCount;
    public int SampleRate => Format.SampleRate;
    public double DurationSeconds => (double)SamplesCount / Format.SampleRate / Format.ChannelsCount;
    public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);
    public bool IsDisposed => pcmBuffer.IsEmpty;
    [CLSCompliant(false)] public byte* RawData => pcmBuffer.Pointer;
    public nint RawDataSize => pcmBuffer.Size;

    public AudioData(Platform platform, ReadOnlySpan<byte> audioData)
    {
        this.platform = platform;
        pcmBuffer = platform.LoadAudio(audioData, out samplesCount, out format);

        bool rightSize = samplesCount == pcmBuffer.Size * 8 / format.BitDepth / format.ChannelsCount;
        if (!rightSize) throw new ResourceLoadFailedException("PCM buffer size not match.", ResourceType.Audio);
    }

    public void Dispose()
    {
        if (!pcmBuffer.IsEmpty)
        {
            platform.FreeAudio(pcmBuffer);
            pcmBuffer = UnmanagedMemory.Empty;
            samplesCount = 0;
            format = AudioFormat.Empty;
            GC.SuppressFinalize(this);
        }
    }

    ~AudioData() => Dispose();

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(pcmBuffer.IsEmpty, this);
}