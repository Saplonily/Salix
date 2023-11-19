#include "exports.h"
#include <cstdint>

#pragma pack(1)
struct AudioFormat
{
    int32_t SampleRate;
    int16_t ChannelsCount;
    int16_t BitDepth;
};
#pragma pack()

EXPORT void* CALLCONV MsdLoadAudio(void* memory, int* samples, AudioFormat* format)
{
    return nullptr;
}

EXPORT void CALLCONV MsdFreeAudio(void* memory)
{

}