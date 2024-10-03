@echo off
cmake -B build -G "Visual Studio 17 2022" -A x64 -D SLX_TARGET_OS="win" -D SLX_TARGET_ARCH="x64" -D CMAKE_CXX_FLAGS="/MP"