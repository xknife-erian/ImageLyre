cmake_minimum_required(VERSION 3.22)
project(imglyre C)

set(CMAKE_C_STANDARD 99)

add_library(imglyre SHARED sample.c)
