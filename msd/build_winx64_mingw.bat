@echo off

if not exist "build_winx64_mingw" (
    echo script: build_winx64_mingw not exists, building...
    cmake -B build_winx64_mingw -G "MinGW Makefiles" -DCMAKE_CXX_FLAGS="-m64" -DMSD_TARGET_OS="win" -DMSD_TARGET_ARCH="x64"
)

echo script: compiling win-x64...
cmake --build build_winx64_mingw --config Release

echo script: finished compiling win-x64
pause