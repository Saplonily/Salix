namespace Monosand;

// TODO dispose impl
public sealed unsafe class AudioContext
{
    private static readonly int ParamOffset = Interop.MsdaGetAudioContextParamOffset();
    private IntPtr nativeHandle;

    public float MasterVolume
    {
        get => *(float*)((byte*)nativeHandle.ToPointer() + ParamOffset);
        set
        {
            ThrowHelper.ThrowIfNegative(value);
            *(float*)((byte*)nativeHandle.ToPointer() + ParamOffset) = value;
        }
    }

    public AudioContext()
    {
        nativeHandle = Interop.MsdaCreateAudioContext();
        if (nativeHandle == IntPtr.Zero)
            throw new OperationFailedException("AudioContext creation failed.");
    }

    public void Play(SoundInstance soundInstance)
    {
        EnsureState();
        Interop.MsdaPlaySoundInstance(soundInstance.NativeHandle);
    }

    public void MakeCurrent()
    {
        EnsureState();
        Interop.MsdaSetCurrentAudioContext(nativeHandle);
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);
}