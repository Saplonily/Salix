@echo off

if not exist "build_winx32" (
    echo script: build_winx32 not exists, building...
    cmake -B build_winx32 -G "Visual Studio 17 2022" -A Win32
)
if not exist "build_winx64" (
    echo script: build_winx64 not exists, building...
    cmake -B build_winx64 -G "Visual Studio 17 2022" -A x64
)

echo script: compiling win-x32...
cmake --build build_winx32 --config Release
echo script: compiling win-x64...
cmake --build build_winx64 --config Release

echo script: finished compiling win-x32 and win-x64
pause