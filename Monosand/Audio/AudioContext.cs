namespace Monosand;

// TODO dispose impl
public sealed class AudioContext
{
    private IntPtr nativeHandle;

    public AudioContext()
    {
        nativeHandle = Interop.MsdaCreateAudioContext();
        if (nativeHandle == IntPtr.Zero)
            throw new OperationFailedException("AudioContext creation failed.");
    }

    public void MakeCurrent()
    {
        EnsureState();
        Interop.MsdaSetCurrentAudioContext(nativeHandle);
    }

    private void EnsureState()
        => ThrowHelper.ThrowIfDisposed(nativeHandle == IntPtr.Zero, this);
}