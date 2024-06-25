@echo off
cmake -B build -G "Visual Studio 17 2022" -A x64 -DSLX_TARGET_OS="win" -DSLX_TARGET_ARCH="x64"