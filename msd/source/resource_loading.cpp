#include <stb_image.h>
#include "common.h"
#include "graphics_enums.h"

// just temporarily use stb_image
// you can always switch to other libraries you like easily here
static void* load_image(void* mem, int length, int* x, int* y, int* data_length, ImageFormat* format)
{
    int channels;
    byte* data = stbi_load_from_memory((stbi_uc*)mem, length, x, y, &channels, 0);
    switch (channels)
    {
    case 1: *format = ImageFormat::R8; break;
    case 2: *format = ImageFormat::Rg16; break;
    case 3: *format = ImageFormat::Rgb24; break;
    case 4: *format = ImageFormat::Rgba32; break;
    default:
        *format = (ImageFormat)-1;
        stbi_image_free(data);
        return nullptr;
    }
    *data_length = *x * *y * channels;
    return data;
}

static void free_image(void* data)
{
    stbi_image_free(data);
}

EXPORT void* CALLCONV MsdLoadImage(void* mem, int length, int* x, int* y, int* data_length, ImageFormat* format)
{
    return load_image(mem, length, x, y, data_length, format);
}

EXPORT void CALLCONV MsdFreeImage(void* texData)
{
    free_image(texData);
}