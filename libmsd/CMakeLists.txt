cmake_minimum_required(VERSION 3.25.0)
include(CMakeDependentOption)

project(libmsd)

set(MSD_ARCH_NAME "")
set(MSD_OS_NAME "")
cmake_dependent_option(MSD_BUILD_WIN32 "Build for Win32" ON "WIN32" OFF)

if(MSD_BUILD_WIN32)
    set(MSD_OS_NAME "win")

    if(CMAKE_GENERATOR_PLATFORM STREQUAL "x64")
        set(MSD_ARCH_NAME "x64")
    elseif(CMAKE_GENERATOR_PLATFORM STREQUAL "Win32")
        set(MSD_ARCH_NAME "x32")
    else()
        message(FATAL_ERROR "Other arches (${CMAKE_GENERATOR_PLATFORM}) are not supported currently.")
    endif()

    message("Build for win32, arch: ${MSD_ARCH_NAME}")
else()
    message(FATAL_ERROR "Other platforms (${CMAKE_SYSTEM_NAME}) are not supported currently.")
endif()

set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set_property(GLOBAL PROPERTY PREDEFINED_TARGETS_FOLDER "CMakeTargets")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${MSD_OS_NAME}-${MSD_ARCH_NAME}>)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${MSD_OS_NAME}-${MSD_ARCH_NAME}>)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${MSD_OS_NAME}-${MSD_ARCH_NAME}>)

add_library(libmsd SHARED)
set_target_properties(libmsd PROPERTIES PREFIX "")
set_target_properties(libmsd PROPERTIES IMPORT_PREFIX "")
file(GLOB_RECURSE sources_files CONFIGURE_DEPENDS source/*.cpp source/*.c source/*.h thirdparty/unpackaged/*.cpp thirdparty/unpackaged/*.h thirdparty/unpackaged/*.c)
target_sources(libmsd PUBLIC ${sources_files})
source_group(TREE ${CMAKE_SOURCE_DIR} FILES ${sources_files})

include_directories(thirdparty/unpackaged)

if(MSD_BUILD_WIN32)
    add_definitions("-DUNICODE" "-D_UNICODE")
    target_link_libraries(libmsd opengl32.lib)
    target_link_libraries(libmsd winmm.lib)
    target_link_libraries(libmsd imm32.lib)
endif()