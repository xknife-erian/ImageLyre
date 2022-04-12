using System.Drawing;
using ImageLad.ImageEngine.Enums;
using ImageMagick;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;

namespace ImageLad.ImageEngine;

public static class ImageTargetExtension
{
    /// <summary>
    /// 将目标图像转为灰度图
    /// </summary>
    public static void ToGray(this ImageTarget target)
    {
        //target.Bitmap = ImageUtil.ImageFormatConverter(target.Bitmap, ImageLadFormat.Gray, BitsPerPixel.Bit8);
        using (Image image = Image.Load(target.File.FullName))
        {
            image.Mutate(x => x
                .Resize(image.Width / 2, image.Height / 2)
                .Grayscale());
            target.Bitmap = image.ToSKBitmap();
        }
    }

    public static SKBitmap ToSKBitmap(this Image image)
    {
        var bitmap = new SKBitmap();

        return bitmap;
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