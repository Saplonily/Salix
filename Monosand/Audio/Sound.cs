namespace Monosand;

public sealed class Sound
{
    private IntPtr nativeHandle;
    private AudioData audioData;

    public unsafe Sound(AudioData audioData)
    {
        this.audioData = audioData;
        nativeHandle = Interop.MsdaCreateSound(audioData.Format, audioData.RawData, audioData.FramesCount);
        if (nativeHandle == IntPtr.Zero)
            throw new OperationFailedException("Sound creation failed.");
    }

    public void Play()
    {
        IntPtr si = Interop.MsdaCreateSoundInstance(nativeHandle);
        Interop.MsdaPlaySoundInstance(si);
    }
}
