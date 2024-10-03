#pragma once
#ifndef H_MEMORY
#define H_MEMORY

#include <malloc.h>
#include <string>

template<class T>
void zeroMemory(T* ptr, int32_t length)
{
    memset((void*)ptr, 0, length);
}

template<class T>
void zeroMemory(T* ptr, size_t length)
{
    memset((void*)ptr, 0, length);
}

template<class T>
T* stackAlloc(int32_t length)
{
    return (T*)alloca(sizeof(T) * length);
}

template<class T>
T* smallAlloc()
{
    return new T;
}

template<class T>
T* smallAllocZero()
{
    T* ptr = new T;
    zeroMemory(ptr, (int32_t)1);
    return ptr;
}

template<class T>
T* smallAlloc(int32_t length)
{
    return new T[length];
}

template<class T>
T* smallAllocZero(int32_t length)
{
    T* ptr = new T[length];
    zeroMemory(ptr, length);
    return ptr;
}

template<class T>
void smallFree(T* ptr)
{
    delete[] ptr;
}

#endif