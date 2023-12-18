namespace Monosand;

// TODO dispose impl
public sealed class Sound
{
    private IntPtr nativeHandle;
    private AudioData audioData;

    public AudioData AudioData => audioData;

    public unsafe Sound(AudioData audioData)
    {
        this.audioData = audioData;
        nativeHandle = Interop.MsdaCreateSound(audioData.Format, audioData.RawData, audioData.FramesCount);
        if (nativeHandle == IntPtr.Zero)
            throw new OperationFailedException("Sound creation failed.");
    }

    public SoundInstance CreateInstance()
    {
        return new(this, Interop.MsdaCreateSoundInstance(nativeHandle));
    }

    public SoundInstance Play()
    {
        SoundInstance si = CreateInstance();
        Interop.MsdaPlaySoundInstance(si.NativeHandle);
        return si;
    }
}
