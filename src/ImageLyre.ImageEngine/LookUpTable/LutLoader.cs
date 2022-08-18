using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImageLyric.ImageEngine.LookUpTable;

/// <summary>
///     打开ImageJ look-up tables (LUTs)文件。包括：
///     1. 768 byte binary LUTs(256 Reds, 256 Greens and 256 Blues),
///     2. LUTs in text format,
///     lukan@tvscope.cn
///     2022年7月15日
/// </summary>
public class LUTLoader
{
    /// <summary>
    ///     打开ImageJ的LUT配置文件。包括三种类型：1.二进制文件;2.带头信息的二进制文件;3.文本数据文件。
    /// </summary>
    /// <param name="fileFullPath">文件的完整路径</param>
    /// <returns>LUT</returns>
    /// <exception cref="LUTFileParseException"></exception>
    public LUT OpenLUTFile(string fileFullPath)
    {
        var fi = new FileInfo(fileFullPath);
        if (!fi.Exists)
            throw new ArgumentException($"文件不存在：{fileFullPath}");
        var lut = new LUT {FileInfo = fi};
        var size = 0;
        try
        {
            var length = fi.Length;
            if (length > 768)
                size = OpenBinaryLUT(lut, false);
            if (size == 0 && (length == 0 || length == 768 || length == 970))
                size = OpenBinaryLUT(lut, true);
            if (size == 0 && length > 768)
                size = OpenTextLUT(lut);
            if (size == 0)
                throw new LUTFileParseException();
        }
        catch (IOException e)
        {
            throw new LUTFileParseException();
        }

        return lut;
    }

    /**
     * Opens an NIH Image LUT or a 768 byte binary LUT.
     */
    private int OpenBinaryLUT(LUT fi, bool raw)
    {
        using var f = new FileStream(fi.FileInfo.FullName, FileMode.Open);
        var colorCount = 256;
        if (!raw)
        {
            // 32 byte NIH Image LUT 头部格式信息
            var id = f.ReadInt();
            if (id != 1229147980) // 'ICOL'
            {
                f.Close();
                return 0;
            }

            int version = f.ReadShort();
            colorCount = f.ReadShort();
            int start = f.ReadShort();
            int end = f.ReadShort();
            var fill1 = f.ReadLong();
            var fill2 = f.ReadLong();
            var filler = f.ReadInt();
        }

        f.Read(fi.Reds, 0, colorCount);
        f.Read(fi.Greens, 0, colorCount);
        f.Read(fi.Blues, 0, colorCount);
        if (colorCount < 256)
            Interpolate(fi.Reds, fi.Greens, fi.Blues, colorCount);
        fi.Name = fi.FileInfo.Name;
        f.Close();
        return 256;
    }

    private int OpenTextLUT(LUT fi)
    {
        using var fs = new FileStream(fi.FileInfo.FullName, FileMode.Open);
        fi.Name = fi.FileInfo.Name;
        using var sr = new StreamReader(fs, Encoding.ASCII, true);
        var line = sr.ReadLine();
        if (line == null)
            throw new InvalidDataException($"文件可打开，但是读取内容时，异常。\r\n{fi.FileInfo.FullName}");
        while (string.IsNullOrEmpty(line?.Trim())) //文件起始可能有空行。
            line = sr.ReadLine();
        char vc = ' ';
        if (line.TrimEnd().StartsWith("Index")) //数据文件的起始说明行
        {
            line = sr.ReadLine();
            vc = '\t';
        }

        var i = 0;
        while (sr.Peek() > -1)
        {
            if (!string.IsNullOrEmpty(line))
            {
                var colors = Split(line.TrimStart(' '), vc);
                fi.Reds[i] = colors[0];
                fi.Greens[i] = colors[1];
                fi.Blues[i] = colors[2];
            }

            line = sr.ReadLine();
            i++;
        }
        return i;
    }

    private byte[] Split(string line, char splitChar)
    {
        var bs = new byte[3];
        var strings = line.Split(new[] {splitChar}, StringSplitOptions.RemoveEmptyEntries);
        var i = 0;
        if (strings.Length == 4)
            i = 1;
        for (; i < strings.Length; i++)
        {
            var j = strings.Length==4 ? i - 1 : i;
            bs[j] = byte.Parse(strings[i]);
        }

        return bs;
    }

    /// <summary>
    ///     ImageJ菜单中预置的几种LUT,根据指定的名称直接代码生成。
    /// </summary>
    /// <param name="name">LUT名称</param>
    public LUT GetPresetLUT(string name)
    {
        var fi = new LUT
        {
            Reds = new byte[256],
            Greens = new byte[256],
            Blues = new byte[256]
        };
        if (string.IsNullOrEmpty(name))
            return fi;
        if (name.Equals("3-3-2 RGB")) name = "3-3-2 rgb";
        if (name.Equals("red/green")) name = "redgreen";
        var nColors = 0;
        if (name.Equals("fire"))
            nColors = Fire(fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("grays"))
            nColors = Grays(fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("ice"))
            nColors = Ice(fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("spectrum"))
            nColors = Spectrum(fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("3-3-2 rgb"))
            nColors = Rgb332(fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("red"))
            nColors = PrimaryColor(4, fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("green"))
            nColors = PrimaryColor(2, fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("blue"))
            nColors = PrimaryColor(1, fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("cyan"))
            nColors = PrimaryColor(3, fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("magenta"))
            nColors = PrimaryColor(5, fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("yellow"))
            nColors = PrimaryColor(6, fi.Reds, fi.Greens, fi.Blues);
        else if (name.Equals("redgreen"))
            nColors = RedGreen(fi.Reds, fi.Greens, fi.Blues);
        if (nColors > 0)
            if (nColors < 256)
                Interpolate(fi.Reds, fi.Greens, fi.Blues, nColors);
        fi.Name = name;
        return fi;
    }

    private static int Fire(byte[] reds, byte[] greens, byte[] blues)
    {
        int[] r =
        {
            0, 0, 1, 25, 49, 73, 98, 122, 146, 162, 173, 184, 195, 207, 217, 229, 240, 252, 255, 255, 255, 255, 255,
            255, 255, 255, 255, 255, 255, 255, 255, 255
        };
        int[] g =
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 14, 35, 57, 79, 101, 117, 133, 147, 161, 175, 190, 205, 219, 234,
            248, 255, 255, 255, 255
        };
        int[] b =
        {
            0, 61, 96, 130, 165, 192, 220, 227, 210, 181, 151, 122, 93, 64, 35, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 35,
            98, 160, 223, 255
        };
        for (var i = 0; i < r.Length; i++)
        {
            reds[i] = (byte) r[i];
            greens[i] = (byte) g[i];
            blues[i] = (byte) b[i];
        }

        return r.Length;
    }

    private static int Grays(byte[] reds, byte[] greens, byte[] blues)
    {
        for (var i = 0; i < 256; i++)
        {
            reds[i] = (byte) i;
            greens[i] = (byte) i;
            blues[i] = (byte) i;
        }

        return 256;
    }

    private static int PrimaryColor(int color, byte[] reds, byte[] greens, byte[] blues)
    {
        for (var i = 0; i < 256; i++)
        {
            if ((color & 4) != 0)
                reds[i] = (byte) i;
            if ((color & 2) != 0)
                greens[i] = (byte) i;
            if ((color & 1) != 0)
                blues[i] = (byte) i;
        }

        return 256;
    }

    private static int Ice(byte[] reds, byte[] greens, byte[] blues)
    {
        int[] r =
        {
            0, 0, 0, 0, 0, 0, 19, 29, 50, 48, 79, 112, 134, 158, 186, 201, 217, 229, 242, 250, 250, 250, 250, 251, 250,
            250, 250, 250, 251, 251, 243, 230
        };
        int[] g =
        {
            156, 165, 176, 184, 190, 196, 193, 184, 171, 162, 146, 125, 107, 93, 81, 87, 92, 97, 95, 93, 93, 90, 85, 69,
            64, 54, 47, 35, 19, 0, 4, 0
        };
        int[] b =
        {
            140, 147, 158, 166, 170, 176, 209, 220, 234, 225, 236, 246, 250, 251, 250, 250, 245, 230, 230, 222, 202,
            180, 163, 142, 123, 114, 106, 94, 84, 64, 26, 27
        };
        for (var i = 0; i < r.Length; i++)
        {
            reds[i] = (byte) r[i];
            greens[i] = (byte) g[i];
            blues[i] = (byte) b[i];
        }

        return r.Length;
    }

    private static int Spectrum(byte[] reds, byte[] greens, byte[] blues)
    {
        for (var i = 0; i < 256; i++)
        {
            var argb = ImageUtil.HSBtoRGB(i / 255f, 1f, 1f);
            var c = Color.FromArgb(argb);
            reds[i] = c.R;
            greens[i] = c.G;
            blues[i] = c.B;
        }

        return 256;
    }

    private static int Rgb332(byte[] reds, byte[] greens, byte[] blues)
    {
        Color c;
        for (var i = 0; i < 256; i++)
        {
            reds[i] = (byte) (i & 0xe0);
            greens[i] = (byte) ((i << 3) & 0xe0);
            blues[i] = (byte) ((i << 6) & 0xc0);
        }

        return 256;
    }

    private static int RedGreen(byte[] reds, byte[] greens, byte[] blues)
    {
        for (var i = 0; i < 128; i++)
        {
            reds[i] = (byte) (i * 2);
            greens[i] = 0;
            blues[i] = 0;
        }

        for (var i = 128; i < 256; i++)
        {
            reds[i] = 0;
            greens[i] = (byte) (i * 2);
            blues[i] = 0;
        }

        return 256;
    }

    private static void Interpolate(byte[] reds, byte[] greens, byte[] blues, int nColors)
    {
        var r = new byte[nColors];
        var g = new byte[nColors];
        var b = new byte[nColors];
        Array.Copy(reds, 0, r, 0, nColors);
        Array.Copy(greens, 0, g, 0, nColors);
        Array.Copy(blues, 0, b, 0, nColors);
        var scale = nColors / 256.0;
        int i1, i2;
        double fraction;
        for (var i = 0; i < 256; i++)
        {
            i1 = (int) (i * scale);
            i2 = i1 + 1;
            if (i2 == nColors) i2 = nColors - 1;
            fraction = i * scale - i1;
            reds[i] = (byte) ((1.0 - fraction) * (r[i1] & 255) + fraction * (r[i2] & 255));
            greens[i] = (byte) ((1.0 - fraction) * (g[i1] & 255) + fraction * (g[i2] & 255));
            blues[i] = (byte) ((1.0 - fraction) * (b[i1] & 255) + fraction * (b[i2] & 255));
        }
    }
}