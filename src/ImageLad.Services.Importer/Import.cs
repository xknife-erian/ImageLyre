using System;
using System.Runtime.InteropServices;

namespace ImageLad.Services.Importer
{
    public static class Import
    {
        [DllImport("Wrapper\\library.dll", EntryPoint = "sum")]
        public static extern int sum(int v1, int v2);
    }
}
