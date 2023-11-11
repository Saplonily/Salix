cmake_minimum_required(VERSION 3.25.3)

project(libmsd)

set_property(GLOBAL PROPERTY USE_FOLDERS ON)
set_property(GLOBAL PROPERTY PREDEFINED_TARGETS_FOLDER "CMakeTargets")

set(LIBRARY_OUTPUT_PATH ${PROJECT_SOURCE_DIR}/bin/win-x64)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/win-x64>)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/win-x64>)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY $<1:${CMAKE_SOURCE_DIR}/bin/win-x64>)

add_library(libmsd SHARED)
set_target_properties(libmsd PROPERTIES PREFIX "")
set_target_properties(libmsd PROPERTIES IMPORT_PREFIX "")
file(GLOB_RECURSE sources_files CONFIGURE_DEPENDS source/*.cpp source/*.c source/*.h)
target_sources(libmsd PUBLIC ${sources_files})
source_group(TREE ${CMAKE_SOURCE_DIR} FILES ${sources_files})

add_definitions("-DUNICODE" "-D_UNICODE")
target_link_libraries(libmsd opengl32.lib)
target_link_libraries(libmsd winmm.lib)
target_link_libraries(libmsd imm32.lib)