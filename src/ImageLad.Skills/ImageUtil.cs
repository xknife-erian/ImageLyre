using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageLad.Skills;

public static class ImageUtil
{
    /// <summary>
    ///     从流(<see cref="FileStream" />,<see cref="MemoryStream" />)快速创建<see cref="WriteableBitmap" />
    /// </summary>
    /// <param name="stream">包含图像的流</param>
    /// <returns>WPF界面的<see cref="BitmapSource" /></returns>
    public static WriteableBitmap BuildBitmap(Stream stream)
    {
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return new WriteableBitmap(bitmapImage);
    }

    /// <summary>
    /// 反转图像流
    /// </summary>
    /// <param name="src">源图像流</param>
    /// <param name="target">目标图像流</param>
    /// <param name="bytePerPixel">每个像素的字节数</param>
    public static void ReverseImageStream(byte[] src, byte[] target, ushort bytePerPixel)
    {
        var last = target.Length;
        for (int i = 0; i < src.Length; i += bytePerPixel)
        {
            last -= bytePerPixel;
            for (int j = 0; j < bytePerPixel; j++)
                target[last+j] = src[i+j];
        }
    }

    public static int HSBtoRGB(float hue, float saturation, float brightness)
    {
        int r = 0, g = 0, b = 0;
        if (saturation == 0)
        {
            r = g = b = (int) (brightness * 255.0f + 0.5f);
        }
        else
        {
            var h = (hue - (float) Math.Floor(hue)) * 6.0f;
            var f = h - (float) Math.Floor(h);
            var p = brightness * (1.0f - saturation);
            var q = brightness * (1.0f - saturation * f);
            var t = brightness * (1.0f - saturation * (1.0f - f));
            switch ((int) h)
            {
                case 0:
                    r = (int) (brightness * 255.0f + 0.5f);
                    g = (int) (t * 255.0f + 0.5f);
                    b = (int) (p * 255.0f + 0.5f);
                    break;
                case 1:
                    r = (int) (q * 255.0f + 0.5f);
                    g = (int) (brightness * 255.0f + 0.5f);
                    b = (int) (p * 255.0f + 0.5f);
                    break;
                case 2:
                    r = (int) (p * 255.0f + 0.5f);
                    g = (int) (brightness * 255.0f + 0.5f);
                    b = (int) (t * 255.0f + 0.5f);
                    break;
                case 3:
                    r = (int) (p * 255.0f + 0.5f);
                    g = (int) (q * 255.0f + 0.5f);
                    b = (int) (brightness * 255.0f + 0.5f);
                    break;
                case 4:
                    r = (int) (t * 255.0f + 0.5f);
                    g = (int) (p * 255.0f + 0.5f);
                    b = (int) (brightness * 255.0f + 0.5f);
                    break;
                case 5:
                    r = (int) (brightness * 255.0f + 0.5f);
                    g = (int) (p * 255.0f + 0.5f);
                    b = (int) (q * 255.0f + 0.5f);
                    break;
            }
        }

        return (int) (0xff000000 | (r << 16) | (g << 8) | (b << 0));
    }

    public static float[] RGBtoHSB(int r, int g, int b, float[] hsbvals)
    {
        float hue, saturation, brightness;
        if (hsbvals == null) hsbvals = new float[3];
        var cmax = r > g ? r : g;
        if (b > cmax) cmax = b;
        var cmin = r < g ? r : g;
        if (b < cmin) cmin = b;

        brightness = cmax / 255.0f;
        if (cmax != 0)
            saturation = (cmax - cmin) / (float) cmax;
        else
            saturation = 0;
        if (saturation == 0)
        {
            hue = 0;
        }
        else
        {
            var redc = (cmax - r) / (float) (cmax - cmin);
            var greenc = (cmax - g) / (float) (cmax - cmin);
            var bluec = (cmax - b) / (float) (cmax - cmin);
            if (r == cmax)
                hue = bluec - greenc;
            else if (g == cmax)
                hue = 2.0f + redc - bluec;
            else
                hue = 4.0f + greenc - redc;
            hue = hue / 6.0f;
            if (hue < 0)
                hue = hue + 1.0f;
        }

        hsbvals[0] = hue;
        hsbvals[1] = saturation;
        hsbvals[2] = brightness;
        return hsbvals;
    }
}