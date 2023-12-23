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
        public void* src_state;
        public long playedFrames;
        public float playSpeed;
        public float volume;
        public int refCount;
    }

    private SoundInstanceNative* nativeHandle;
    private Sound sound;

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

    public float Volume
    {
        get => nativeHandle->volume;
        set
        {
            ThrowHelper.ThrowIfNegative(value);
            nativeHandle->volume = value;
        }
    }

    public long PlayedFramesCount => nativeHandle->playedFrames;

    public float PlayedSeconds => (float)PlayedFramesCount / Sound.AudioData.SampleRate;

    public TimeSpan PlayedDuration => TimeSpan.FromSeconds(PlayedSeconds);

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
