cmake_minimum_required(VERSION 3.25.0)
include(CMakeDependentOption)

project(msd)

if(NOT MSD_TARGET_OS)
    message(FATAL_ERROR "Please use scripts to generate project files.")
endif()

message(STATUS "MSD Target OS: ${MSD_TARGET_OS}")
message(STATUS "MSD Target Arch: ${MSD_TARGET_ARCH}")

set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set_property(GLOBAL PROPERTY PREDEFINED_TARGETS_FOLDER "CMakeTargets")

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${MSD_TARGET_OS}-${MSD_TARGET_ARCH}>)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${MSD_TARGET_OS}-${MSD_TARGET_ARCH}>)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/${MSD_TARGET_OS}-${MSD_TARGET_ARCH}>)

add_library(msd SHARED)
file(GLOB_RECURSE sources_files CONFIGURE_DEPENDS source/*.cpp source/*.c source/*.h thirdparty/unpackaged/*.cpp thirdparty/unpackaged/*.h thirdparty/unpackaged/*.c)
target_sources(msd PUBLIC ${sources_files})
source_group(TREE ${CMAKE_SOURCE_DIR} FILES ${sources_files})
set_source_files_properties(${sources_files} PROPERTIES FOLDER "source")

include_directories(thirdparty/unpackaged)

if(MSD_TARGET_OS STREQUAL "win")
    set_target_properties(msd PROPERTIES PREFIX "")
    set_target_properties(msd PROPERTIES IMPORT_PREFIX "")
    add_definitions("-DUNICODE" "-D_UNICODE")
    target_link_libraries(msd opengl32.lib)
    target_link_libraries(msd winmm.lib)
    target_link_libraries(msd imm32.lib)
endif()