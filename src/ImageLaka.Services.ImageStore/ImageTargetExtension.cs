using ImageLaka.ImageEngine.Enums;
using ImageMagick;

namespace ImageLaka.ImageEngine;

public static class ImageTargetExtension
{
    /// <summary>
    /// 将目标图像转为灰度图
    /// </summary>
    public static void ToGray(this ImageTarget target)
    {
    }
    /// <summary>
    /// 将目标图像转为RGB图
    /// </summary>
    public static void ToRGB(this ImageTarget target)
    {
    }
    /// <summary>
    /// 将目标图像转为CMYK图
    /// </summary>
    public static void ToCMYK(this ImageTarget target)
    {
        MagickImage magickImage = new MagickImage(target.File.FullName);
        magickImage.ColorSpace = ColorSpace.CMYK;
        //magickImage.TransformColorSpace(ColorProfile.AppleRGB);
        using (var memStream = new MemoryStream())
        {
            // Write the image to the memorystream
            magickImage.Write(memStream);

            target.Bitmap = new System.Drawing.Bitmap(memStream);
        }
    }
    /// <summary>
    /// 将目标图像转为Lab格式图
    /// </summary>
    public static void ToLab(this ImageTarget target)
    {
    }
    /// <summary>
    /// 将目标图像转为8位灰度图
    /// </summary>
    public static void To8Bit(this ImageTarget target)
    {
        target.Bitmap = ImageUtil.ImageFormatConverter(target.Bitmap, ImageFormat.Gray, BitsPerPixel.Bit8);
    }

    /// <summary>
    /// 将目标图像转为16位灰度图
    /// </summary>
    public static void To16Bit(this ImageTarget target)
    {
    }

    /// <summary>
    /// 将目标图像转为32位灰度图
    /// </summary>
    public static void To32Bit(this ImageTarget target)
    {
    }
}