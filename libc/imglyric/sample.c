#include "sample.h"

#include <stdio.h>

__declspec(dllexport) void hello(void) {
    printf("Hello, World!\n");
}
