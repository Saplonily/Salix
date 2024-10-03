#pragma once
#ifndef H_LIST
#define H_LIST

#include <cstddef>

// simple vector impl without stl
template<typename T>
class List
{
private:
    T* _data;       
    size_t _size; 
    size_t _capacity;

    void grow()
    {
        size_t new_capacity = _capacity * 2;
        T* new_data = new T[new_capacity];
        for (size_t i = 0; i < _size; ++i)
        {
            new_data[i] = _data[i];
        }
        delete[] _data;
        _data = new_data;
        _capacity = new_capacity;
    }

public:
    List() : _data(new T), _size(0), _capacity(1) { }
    List(size_t capacity) : _data(new T[capacity]), _size(0), _capacity(capacity) { }

    ~List()
    {
        delete[] _data;
    }

    size_t size() const
    {
        return _size;
    }

    T* data()
    {
        return _data;
    }

    size_t capacity() const
    {
        return _capacity;
    }

    T& operator[](size_t index)
    {
        return _data[index];
    }

    void add(const T& value)
    {
        if (_size == _capacity)
            grow();
        _data[_size++] = value;
    }

    void clear()
    {
        _size = 0;
    }

    bool empty() const
    {
        return _size == 0;
    }
};

#endif