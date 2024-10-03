@echo off

if not exist "build_winx32" (
    echo script: build_winx32 not exists, building...
    cmake -B build_winx32 -G "Visual Studio 17 2022" -A Win32 -D SLX_TARGET_OS="win" -D SLX_TARGET_ARCH="x32"
)
if not exist "build_winx64" (
    echo script: build_winx64 not exists, building...
    cmake -B build_winx64 -G "Visual Studio 17 2022" -A x64 -D SLX_TARGET_OS="win" -D SLX_TARGET_ARCH="x64"
)

echo script: compiling win-x32...
cmake --build build_winx32 --config Release -- /p:CL_MP=true /p:CL_MPCount=12
echo script: compiling win-x64...
cmake --build build_winx64 --config Release -- /p:CL_MP=true /p:CL_MPCount=12

echo script: finished compiling win-x32 and win-x64
pause