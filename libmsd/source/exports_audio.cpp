#include "exports.h"
#include <cstdint>
#include "dr_wav.h"

#pragma pack(1)
struct AudioFormat
{
    int32_t SampleRate;
    int8_t BitDepth;
    int8_t ChannelsCount;
    int8_t IsFloat;
};
#pragma pack()

EXPORT void* CALLCONV MsdLoadAudio(void* memory, size_t length, int* dataLength, AudioFormat* format)
{
    return nullptr;
}

EXPORT void CALLCONV MsdFreeAudio(void* memory)
{

}