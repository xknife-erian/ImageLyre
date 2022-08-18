#include "sample.h"

#include <stdio.h>

__declspec(dllexport) int calc(int a, int b, int c, int d) {
    return (a + b) * (c + d);
}
