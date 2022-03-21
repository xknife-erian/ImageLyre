using System;
using System.Runtime.InteropServices;

namespace ImageLad.Services.Importer
{
    public static class Import
    {
        [DllImport("TestDLL")]
        public static extern int[] GetHistogram(float v1, float v2, float t);
    }
}
