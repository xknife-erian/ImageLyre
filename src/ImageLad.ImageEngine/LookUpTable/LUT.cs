using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLad.ImageEngine.LookUpTable;

public class LUT
{
    public byte[] Reds { get; set; } = new byte[256];
    public byte[] Greens { get; set; } = new byte[256];
    public byte[] Blues { get; set; } = new byte[256];
    public FileInfo FileInfo { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// 基于LUT获取调色板
    /// </summary>
    /// <returns>调色板</returns>
    public BitmapPalette GetPalette()
    {
        var colors = new List<Color>();
        for (int i = 0; i < Reds.Length; i++)
        {
            colors.Add(Color.FromRgb(Reds[i], Greens[i], Blues[i]));
        }

        var palette = new BitmapPalette(colors);
        return palette;
    }

    public IDictionary<int, Color> GetColorMap()
    {
        var map = new Dictionary<int, Color>();
        for (int i = 0; i < Reds.Length; i++)
        {
            map.Add(i,Color.FromRgb(Reds[i], Greens[i], Blues[i]));
        }
        return map;
    }
}
