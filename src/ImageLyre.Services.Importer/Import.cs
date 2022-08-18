using System.Runtime.InteropServices;

namespace ImageLyric.Services.Importer
{
    public static class Import
    {
        [DllImport("Wrapper\\libimglyric.dll", EntryPoint = "calc")]
        public static extern int Calc(int a1, int a2, int b1, int b2);
    }
}
