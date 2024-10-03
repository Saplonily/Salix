cmake_minimum_required(VERSION 3.25.0)
include(CMakeDependentOption)

project(slx)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

if(NOT SLX_TARGET_OS)
    message(FATAL_ERROR "Please use the scripts to generate project files. Or you can help us improve the CMake files.")
endif()

message(STATUS "Salix Target OS: ${SLX_TARGET_OS}")
message(STATUS "Salix Target Arch: ${SLX_TARGET_ARCH}")

if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
    add_compile_options(-fno-exceptions)
    add_compile_options(-nostdlib++)
elseif(CMAKE_CXX_COMPILER_ID MATCHES "MSVC")
    add_compile_options(/Zc:__cplusplus)
    add_compile_options(/EHs-c-)
endif()

set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set_property(GLOBAL PROPERTY PREDEFINED_TARGETS_FOLDER "CMakeTargets")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${SLX_TARGET_OS}-${SLX_TARGET_ARCH}>)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${SLX_TARGET_OS}-${SLX_TARGET_ARCH}>)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${SLX_TARGET_OS}-${SLX_TARGET_ARCH}>)

add_library(slx SHARED)
file(GLOB_RECURSE sources_files CONFIGURE_DEPENDS 
    source/*.cpp 
    source/*.c
    source/*.h
    thirdparty/*.cpp
    thirdparty/*.c
    thirdparty/*.h
)
target_sources(slx PUBLIC ${sources_files})
source_group(TREE ${CMAKE_SOURCE_DIR} FILES ${sources_files})
set_source_files_properties(${sources_files} PROPERTIES FOLDER "source")

include_directories(thirdparty)
include_directories(thirdparty/stb)

if(SLX_TARGET_OS STREQUAL "win")
    add_definitions("-DSLX_WIN")
    add_definitions("-DUNICODE" "-D_UNICODE")

    set_target_properties(slx PROPERTIES PREFIX "")
    set_target_properties(slx PROPERTIES IMPORT_PREFIX "")

    target_link_libraries(slx opengl32.lib)
    target_link_libraries(slx winmm.lib)
    target_link_libraries(slx imm32.lib)
endif()