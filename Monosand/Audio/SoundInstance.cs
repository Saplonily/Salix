using System.Runtime.CompilerServices;

namespace Monosand;

// TODO dispose impl
public sealed unsafe class SoundInstance
{
    private IntPtr nativeHandle;
    private Sound sound;
    private SoundInstance? next;

    internal IntPtr NativeHandle => nativeHandle;
    private float* PlaySpeedPtr => (float*)(((long*)(((void**)nativeHandle.ToPointer()) + 3)) + 1);
    private int* RefPtr => (int*)(PlaySpeedPtr + 1);

    public Sound Sound => sound;

    public float PlaySpeed { get => *PlaySpeedPtr; set { ThrowHelper.ThrowIfInvalid(PlaySpeed <= 0); *PlaySpeedPtr = value; } }

    public long PlayedFramesCount => *(long*)(((void**)nativeHandle.ToPointer()) + 3);

    public float PlayedSeconds => (float)PlayedFramesCount / Sound.AudioData.SampleRate / Sound.AudioData.ChannelsCount;

    public TimeSpan PlayedDuration => TimeSpan.FromSeconds(PlayedSeconds);

    public SoundInstance? Next
    {
        get => next;
        set
        {
            var pnext = this.next;
            this.next = value;
            IntPtr* p = ((IntPtr*)nativeHandle.ToPointer() + 1);
            *p = value is null ? IntPtr.Zero : value.nativeHandle;
            SoundInstance? next = Next;
            if (next is not null)
                *next.RefPtr += 1;
            if(pnext != null)
            {
                *pnext.RefPtr -= 1;
                if (*pnext.RefPtr == 0)
                    Interop.MsdaDeleteSoundInstance(pnext.nativeHandle);
            }
        }
    }

    internal SoundInstance(Sound sound, IntPtr nativeHandle)
    {
        this.sound = sound;
        this.nativeHandle = nativeHandle;
        *RefPtr += 1;
    }

    ~SoundInstance()
    {
        // decrease ref count
        *RefPtr -= 1;
        if (*RefPtr == 0)
            Interop.MsdaDeleteSoundInstance(nativeHandle);
    }
}
