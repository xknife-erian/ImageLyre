using System;
using System.Runtime.InteropServices;

namespace ImageLad.Services.Importer
{
    public static class Import
    {
        [DllImport("Wrapper\\libuntitled2.dll", EntryPoint = "hello")]
        public static extern int hello(int v1, int v2, int v3);
    }
}
