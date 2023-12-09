#include "exports.h"
#include <cstdint>
#include "dr_wav.h"

struct AudioFormat
{
    int32_t SampleRate;
    int8_t ChannelsCount;
    int8_t BitDepth;
    int8_t IsFloat;
};

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