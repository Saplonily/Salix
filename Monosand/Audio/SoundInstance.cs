using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Monosand;

// TODO dispose impl
public sealed unsafe class SoundInstance
{
    [StructLayout(LayoutKind.Sequential)]
    private struct SoundInstanceNative
    {
        public void* sound;
        public void* next;
        public void* src_state;
        public long playedFrames;
        public float playSpeed;
        public int refCount;
    }

    private SoundInstanceNative* nativeHandle;
    private Sound sound;
    private SoundInstance? next;

    internal IntPtr NativeHandle => new IntPtr(nativeHandle);

    public Sound Sound => sound;

    public float PlaySpeed
    {
        get => nativeHandle->playSpeed;
        set
        {
            ThrowHelper.ThrowIfInvalid(PlaySpeed <= 0);
            nativeHandle->playSpeed = value;
        }
    }

    public long PlayedFramesCount => nativeHandle->playedFrames;

    public float PlayedSeconds => (float)PlayedFramesCount / Sound.AudioData.SampleRate;

    public TimeSpan PlayedDuration => TimeSpan.FromSeconds(PlayedSeconds);

    public SoundInstance? Next
    {
        get => next;
        set
        {
            var pnext = this.next;
            this.next = value;
            nativeHandle->next = value is null ? null : value.nativeHandle;
            SoundInstance? next = Next;
            if (next is not null)
                next.nativeHandle->refCount += 1;
            if (pnext != null)
            {
                pnext.nativeHandle->refCount -= 1;
                if (pnext.nativeHandle->refCount == 0)
                    Interop.MsdaDeleteSoundInstance(new IntPtr(pnext.nativeHandle));
            }
        }
    }

    internal SoundInstance(Sound sound, IntPtr nativeHandle)
    {
        this.sound = sound;
        this.nativeHandle = (SoundInstanceNative*)nativeHandle;
        this.nativeHandle->refCount += 1;
    }

    ~SoundInstance()
    {
        // decrease ref count
        nativeHandle->refCount -= 1;
        if (nativeHandle->refCount == 0)
            Interop.MsdaDeleteSoundInstance(new IntPtr(nativeHandle));
    }
}
