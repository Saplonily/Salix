#include "exports.h"
#include <cstdint>
#include <Audioclient.h>
#include <mmdeviceapi.h>
#include <vector>
#include <assert.h>
#include <algorithm>
#include <samplerate.h>
#include <mutex>
#include "dr_wav.h"

struct AudioFormat;
struct Sound;
struct SoundInstance;
struct AudioContext;

struct AudioFormat
{
    int32_t SampleRate;
    int8_t ChannelsCount;
    int8_t BitDepth;
    int8_t IsFloat;
};

struct AudioContext
{
    std::mutex* mutex; // for toPlayInstances
    std::vector<SoundInstance*> instances;
    std::vector<SoundInstance*> toPlayInstances;
    IAudioClient* client;
    IAudioRenderClient* renderer;
    HANDLE fillEvent;
    UINT32 bufferFrameSize;
    float_t masterVolume;
} static* currentContext;

// warning: layout sensitive
struct Sound
{
    AudioFormat format;
    float_t* audioData;
    int64_t framesCount;
};

// warning: layout sensitive
struct SoundInstance
{
    Sound* sound;
    SoundInstance* next;
    SRC_STATE* src_state;
    int64_t playedFrames;
    float_t playSpeed;
    float_t volume;
    int32_t refCount;
};

// TODO check_hr impl?
#define check_hr(hr) if(FAILED(hr)) { printf("Failed with %d at %d.\n", hr, __LINE__); }
#define check_hr_rt(hr) if(FAILED(hr)) { printf("Failed with %d at %d.\n", hr, __LINE__); return nullptr; }

static DWORD audio_processing_thread(LPVOID param);

// TODO support other encode formats
EXPORT void* CALLCONV MsdLoadAudio(
    void* memory, size_t dataSize,
    size_t* loadedDataSize, int64_t* frames,
    AudioFormat* format
)
{
    drwav drwav;
    if (!drwav_init_memory(&drwav, memory, dataSize, nullptr))
        goto err1;
    *frames = drwav.totalPCMFrameCount;
    {
        AudioFormat fmt{};
        fmt.BitDepth = drwav.bitsPerSample;
        fmt.ChannelsCount = drwav.channels;
        fmt.SampleRate = drwav.sampleRate;
        if (drwav.translatedFormatTag == DR_WAVE_FORMAT_PCM)
            fmt.IsFloat = 0;
        else if (drwav.translatedFormatTag == DR_WAVE_FORMAT_IEEE_FLOAT)
            fmt.IsFloat = 1;
        else
            goto err1;

        void* memory = malloc(drwav.dataChunkDataSize);
        if (drwav_read_pcm_frames(&drwav, drwav.totalPCMFrameCount, memory) != drwav.totalPCMFrameCount)
        {
            free(memory);
            goto err1;
        }
        *loadedDataSize = drwav.dataChunkDataSize;
        *frames = drwav.totalPCMFrameCount;
        *format = fmt;
        return memory;
    }

err1:
    drwav_uninit(&drwav);
    return nullptr;
}

EXPORT void CALLCONV MsdFreeAudio(void* memory)
{
    free(memory);
}

static const DWORD mixfmt_sampleRate = 44100;
static const WORD mixfmt_channles = 2;

EXPORT AudioContext* CALLCONV MsdaCreateAudioContext()
{
    HRESULT hr = CoInitializeEx(nullptr, COINIT_MULTITHREADED);
    check_hr_rt(hr);

    const GUID IID_IMMDeviceEnumerator = __uuidof(IMMDeviceEnumerator);
    const CLSID CLSID_MMDeviceEnumerator = __uuidof(MMDeviceEnumerator);
    IMMDeviceEnumerator* enumerator = NULL;
    hr = CoCreateInstance(CLSID_MMDeviceEnumerator, NULL, CLSCTX_ALL, IID_IMMDeviceEnumerator, (void**)&enumerator);
    check_hr_rt(hr);

    IMMDevice* device = nullptr;
    hr = enumerator->GetDefaultAudioEndpoint(eRender, eConsole, &device);
    check_hr_rt(hr);
    enumerator->Release();

    const GUID IID_IAudioClient = __uuidof(IAudioClient);
    IAudioClient* aclient = nullptr;
    hr = device->Activate(IID_IAudioClient, CLSCTX_ALL, NULL, (void**)&aclient);

    check_hr_rt(hr);

    WAVEFORMATEX wfmtex{};
    wfmtex.wFormatTag = WAVE_FORMAT_IEEE_FLOAT;
    wfmtex.nChannels = mixfmt_channles;
    wfmtex.nSamplesPerSec = mixfmt_sampleRate;
    wfmtex.nAvgBytesPerSec = mixfmt_sampleRate * mixfmt_channles * 32 / 8;
    wfmtex.nBlockAlign = mixfmt_channles * 32 / 8;
    wfmtex.wBitsPerSample = 32;
    wfmtex.cbSize = 0;
    DWORD flags = AUDCLNT_STREAMFLAGS_EVENTCALLBACK | AUDCLNT_STREAMFLAGS_AUTOCONVERTPCM | AUDCLNT_STREAMFLAGS_SRC_DEFAULT_QUALITY;
    hr = aclient->Initialize(AUDCLNT_SHAREMODE_SHARED, flags, 0, 0, &wfmtex, NULL);
    check_hr_rt(hr);
    HANDLE fillEvent = CreateEventW(NULL, FALSE, FALSE, nullptr);
    UINT32 bufferFrameSize;
    aclient->GetBufferSize(&bufferFrameSize);
    aclient->SetEventHandle(fillEvent);

    IAudioRenderClient* arender = nullptr;
    const GUID IID_IAudioRenderClient = __uuidof(IAudioRenderClient);
    aclient->GetService(IID_IAudioRenderClient, (void**)&arender);

    AudioContext* ac = new AudioContext;
    ac->client = aclient;
    ac->renderer = arender;
    ac->fillEvent = fillEvent;
    ac->bufferFrameSize = bufferFrameSize;
    ac->instances = std::vector<SoundInstance*>();
    ac->toPlayInstances = std::vector<SoundInstance*>();
    ac->mutex = new std::mutex();
    ac->masterVolume = 1.0f;
    HANDLE thr = CreateThread(0, 0, audio_processing_thread, ac, 0, nullptr);
    if (thr)
        SetThreadPriority(thr, THREAD_PRIORITY_ABOVE_NORMAL);

    aclient->Start();

    return ac;
}

EXPORT void CALLCONV MsdaSetCurrentAudioContext(AudioContext* context)
{
    assert(context != nullptr);
    currentContext = context;
}

EXPORT Sound* CALLCONV MsdaCreateSound(AudioFormat fmt, float_t* audioData, int64_t framesCount)
{
    Sound* snd = new Sound;
    snd->audioData = audioData;
    snd->format = fmt;
    snd->framesCount = framesCount;
    return snd;
}

EXPORT int CALLCONV MsdaGetAudioContextParamOffset()
{
    return (int)offsetof(AudioContext, masterVolume);
}

EXPORT SoundInstance* CALLCONV MsdaCreateSoundInstance(Sound* sound)
{
    SoundInstance* si = new SoundInstance;
    si->sound = sound;
    si->playedFrames = 0;
    si->next = nullptr;
    si->playSpeed = 1.0f;
    si->volume = 1.0f;
    si->refCount = 0;
    // FIXME using other quality will lead to be cut off because we need to fill in more frames
    si->src_state = src_new(SRC_LINEAR, sound->format.ChannelsCount, nullptr);
    return si;
}

EXPORT void CALLCONV MsdaDeleteSoundInstance(SoundInstance* si)
{
    assert(si->refCount == 0);
    if (si->next)
    {
        si->next->refCount--;
        if (si->next->refCount == 0)
            MsdaDeleteSoundInstance(si->next);
    }
    src_delete(si->src_state);
    delete si;
    //printf("deleted SoundInstance: %p\n", si);
}

static void MsdaPlaySoundInstance(SoundInstance* si, AudioContext* ctx)
{
    std::mutex* mutex = ctx->mutex;
    mutex->lock();
    ctx->toPlayInstances.push_back(si);
    si->refCount++;
    mutex->unlock();
}

EXPORT void CALLCONV MsdaPlaySoundInstance(SoundInstance* si)
{
    AudioContext* ctx = currentContext;
    MsdaPlaySoundInstance(si, ctx);
}

static DWORD audio_processing_thread(LPVOID param)
{
    AudioContext* ctx = (AudioContext*)param;
    IAudioClient* client = ctx->client;
    IAudioRenderClient* renderer = ctx->renderer;
    UINT32 bufferFrameSize = ctx->bufferFrameSize;
    float_t* resampledBuffer = new float_t[(size_t)bufferFrameSize * mixfmt_sampleRate * mixfmt_channles];
    std::vector<SoundInstance*>* instances = &ctx->instances;
    std::vector<SoundInstance*>* toPlayInstances = &ctx->toPlayInstances;
    std::mutex* mutex = ctx->mutex;
    while (true)
    {
        WaitForSingleObject(ctx->fillEvent, INFINITE);

        if (toPlayInstances->size())
        {
            mutex->lock();
            for (SoundInstance* si : *toPlayInstances)
                ctx->instances.push_back(si);
            toPlayInstances->clear();
            mutex->unlock();
        }

        UINT32 paddingFrames;
        HRESULT hr = client->GetCurrentPadding(&paddingFrames);
        check_hr(hr);
        UINT32 todoFrames = bufferFrameSize - paddingFrames;
        float_t* outputData = nullptr;
        hr = renderer->GetBuffer(todoFrames, (BYTE**)&outputData);
        check_hr(hr);

        // FIXME seems like .net side would modify the params of SoundInstance at the same time it loops
        // maybe it'll cause werid effects
        memset(outputData, 0, (size_t)todoFrames * mixfmt_channles * sizeof(float));
        for (SoundInstance* si : ctx->instances)
        {
            float_t* data = si->sound->audioData;
            int64_t playedFrames = si->playedFrames;
            int64_t siTodoFrames = min(si->sound->framesCount - playedFrames, todoFrames);
            SRC_DATA src_data{};
            src_data.data_in = data + playedFrames * mixfmt_channles;
            src_data.data_out = resampledBuffer;
            src_data.input_frames = si->sound->framesCount - playedFrames;
            src_data.output_frames = todoFrames;
            src_data.src_ratio = (double_t)mixfmt_sampleRate / si->sound->format.SampleRate / si->playSpeed;
            src_data.end_of_input = 0; // TODO set it
            int err = src_process(si->src_state, &src_data);
            // TODO faster memadd
            float_t volume = si->volume;
            float_t masterVolume = ctx->masterVolume;
            for (int64_t i = 0; i < siTodoFrames; i++)
            {
                for (byte c = 0; c < mixfmt_channles; c++)
                    outputData[i * mixfmt_channles + c] += resampledBuffer[i * mixfmt_channles + c] * volume * masterVolume;
            }
            si->playedFrames += src_data.input_frames_used;
        }

        using it_t = std::vector<SoundInstance*>::iterator;
        for (it_t it = ctx->instances.begin(); it != instances->end();)
        {
            SoundInstance* si = *it;
            if (si->playedFrames == si->sound->framesCount)
            {
                // TODO seamlessly?
                it = instances->erase(it);
                if (si->next)
                    MsdaPlaySoundInstance(si->next, ctx);
                si->refCount--;
                if (si->refCount == 0)
                    MsdaDeleteSoundInstance(si);

            }
            else
            {
                it++;
            }
        }

        hr = renderer->ReleaseBuffer(todoFrames, NULL);
        check_hr(hr);
    }
    delete[] resampledBuffer;
}