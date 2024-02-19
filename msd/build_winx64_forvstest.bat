@echo off
cmake -B build -G "Visual Studio 17 2022" -A x64 -DMSD_TARGET_OS="win" -DMSD_TARGET_ARCH="x64"