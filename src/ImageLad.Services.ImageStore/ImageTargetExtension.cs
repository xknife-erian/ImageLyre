using System.Drawing;
using ImageLad.ImageEngine.Analyze;
using ImageLad.ImageEngine.Enums;
using ImageMagick;
using OpenCvSharp;
using SkiaSharp;

namespace ImageLad.ImageEngine;

public static class ImageTargetExtension
{
    /// <summary>
    /// 将目标图像转为灰度图
    /// </summary>
    public static void ToGray(this ImageTarget target)
    {
        // SKBitmap bitmap = new SKBitmap(target.Bitmap.Width, target.Bitmap.Height);
        //
        // byte[,,] buffer = new byte[target.Bitmap.Width, target.Bitmap.Height, 4];
        // for (int row = 0; row < target.Bitmap.Height; row++)
        // for (int col = 0; col < target.Bitmap.Width; col++)
        // {
        //     buffer[row, col, 0] = (byte) col; // red
        //     buffer[row, col, 1] = 0; // green
        //     buffer[row, col, 2] = (byte) row; // blue
        //     buffer[row, col, 3] = 0xFF; // alpha
        // }
        unsafe
        {
            // fixed (byte* ptr = buffer)
            // {
            //     bitmap.SetPixels((IntPtr) ptr);
            // }
        }
        var stream = File.Open(@"D:\OneDrive\图片\this_is_me.jpg", FileMode.Open);
        target.Bitmap = SKBitmap.Decode(stream);
        //target.Bitmap = bitmap;
    }

    /*
    /// <summary>
    /// 将目标图像转为RGB图
    /// </summary>
    public static void ToRGB(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.ColorSpace = ColorSpace.RGB;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }

    /// <summary>
    /// 将目标图像转为CMYK图
    /// </summary>
    public static void ToCMYK(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.ColorSpace = ColorSpace.CMYK;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }

    /// <summary>
    /// 将目标图像转为Lab格式图
    /// </summary>
    public static void ToLab(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.ColorSpace = ColorSpace.Lab;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }

    /// <summary>
    /// 将目标图像转为Lab格式图
    /// </summary>
    public static void ToHSV(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.ColorSpace = ColorSpace.HSV;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }

    /// <summary>
    /// 将目标图像转为8位灰度图
    /// </summary>
    public static void To8Bit(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.Depth = 8;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }

    /// <summary>
    /// 将目标图像转为16位图
    /// </summary>
    public static void To16Bit(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.Depth = 16;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }

    /// <summary>
    /// 将目标图像转为24位图
    /// </summary>
    public static void To24Bit(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.Depth = 24;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }
    
    /// <summary>
    /// 将目标图像转为32位图
    /// </summary>
    public static void To32Bit(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.Depth = 32;
        using (var memStream = new MemoryStream())
        {
            magickImage.Write(memStream);
            target.Bitmap = new Bitmap(memStream);
        }
    }
    */
}