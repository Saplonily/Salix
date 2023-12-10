#include "exports.h"
#include <cstdint>
#include <Audioclient.h>
#include <mmdeviceapi.h>
#include <vector>
#include <algorithm>
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
    std::mutex* mutex;
    std::vector<SoundInstance*> instances;
    IAudioClient* client;
    IAudioRenderClient* renderer;
    HANDLE fillEvent;
    UINT32 bufferFrameSize;
} static* currentContext;

struct Sound
{
    AudioFormat format;
    float_t* audioData;
    int64_t framesCount;
};

struct SoundInstance
{
    Sound* sound;
    int64_t playedFrames;
};

// TODO check_hr impl?
#define check_hr(hr) if(FAILED(hr)) { printf("Failed with %d at %d.\n", hr, __LINE__); }
#define check_hr_rt(hr) if(FAILED(hr)) { printf("Failed with %d at %d.\n", hr, __LINE__); return nullptr; }

static DWORD audio_processing_thread(LPVOID param);

EXPORT void* CALLCONV MsdLoadAudio(void* memory, size_t dataSize, size_t* loadedDataSize, int64_t* frames, AudioFormat* format)
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
    ac->mutex = new std::mutex();
    CreateThread(0, 0, audio_processing_thread, ac, 0, nullptr);

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

EXPORT SoundInstance* CALLCONV MsdaCreateSoundInstance(Sound* sound)
{
    SoundInstance* si = new SoundInstance;
    si->sound = sound;
    si->playedFrames = 0;
    return si;
}

EXPORT void CALLCONV MsdaPlaySoundInstance(SoundInstance* si)
{
    AudioContext* ctx = currentContext;
    std::mutex* mutex = ctx->mutex;
    mutex->lock();
    ctx->instances.push_back(si);
    mutex->unlock();
}

static DWORD audio_processing_thread(LPVOID param)
{
    AudioContext* ac = (AudioContext*)param;
    IAudioClient* client = ac->client;
    IAudioRenderClient* renderer = ac->renderer;
    UINT32 bufferFrameSize = ac->bufferFrameSize;
    while (true)
    {
        WaitForSingleObject(ac->fillEvent, INFINITE);

        UINT32 paddingFrames;
        HRESULT hr = client->GetCurrentPadding(&paddingFrames);
        check_hr(hr);
        UINT32 todoFrames = bufferFrameSize - paddingFrames;
        float_t* outputData;
        hr = renderer->GetBuffer(todoFrames, (BYTE**)&outputData);
        check_hr(hr);

        memset(outputData, 0, (size_t)todoFrames * 2 * sizeof(float));
        for (SoundInstance* si : ac->instances)
        {
            // TODO faster memadd
            // TODO resample
            float_t* data = si->sound->audioData;
            int64_t playedFrames = si->playedFrames;
            int64_t siTodoFrames = min(si->sound->framesCount - playedFrames, todoFrames);
            for (int64_t i = 0; i < siTodoFrames; i++)
            {
                outputData[i * 2] += data[(playedFrames + i) * 2];
                outputData[i * 2 + 1] += data[(playedFrames + i) * 2 + 1];
            }
            si->playedFrames += siTodoFrames;
            if (si->playedFrames == si->sound->framesCount)
            {
                ac->mutex->lock();
                ac->instances.erase(std::find(ac->instances.cbegin(), ac->instances.cend(), si));
                ac->mutex->unlock();
            }
        }

        hr = renderer->ReleaseBuffer(todoFrames, NULL);
        check_hr(hr);
    }
}