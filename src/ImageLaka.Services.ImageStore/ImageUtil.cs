using System.Drawing;
using System.Drawing.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageLaka.ImageEngine.Enums;
using ImageFormat = ImageLaka.ImageEngine.Enums.ImageFormat;

namespace ImageLaka.ImageEngine;

public static class ImageUtil
{
    public static Bitmap Read(string path)
    {
        if (File.Exists(path)) 
            return new Bitmap(path);

        return new Bitmap(100, 100);
    }

    /// <summary>
    /// 判断一个图像是否是CMYK颜色模式
    /// </summary>
    /// <param name="image">指定的图像</param>
    public static bool IsCMYK(this Image image)
    {
        var flags = (ImageFlags)image.Flags;
        if (flags.HasFlag(ImageFlags.ColorSpaceCmyk) || flags.HasFlag(ImageFlags.ColorSpaceYcck))
        {
            return true;
        }

        const int pixelFormat32BppCMYK = 15 | (32 << 8);
        return (int)image.PixelFormat == pixelFormat32BppCMYK;
    }

    /// <summary>
    /// 根据指定的图像<see cref="Bitmap"/>的属性判断图像的像素深度
    /// </summary>
    /// <param name="bmp">指定的图像</param>
    /// <returns>像素深度</returns>
    public static BitsPerPixel GetBitsPerPixel(Bitmap? bmp)
    {
        if (bmp == null)
            return BitsPerPixel.Undefined;
        switch (bmp.PixelFormat)
        {
            case PixelFormat.Format16bppArgb1555: //像素格式为每像素16位。该颜色信息指定32,768种色调，其中5位为红色，5位为绿色，5位为蓝色，1位为alpha。
            case PixelFormat.Format16bppGrayScale: //像素格式为每像素 16 位。该颜色信息指定 65536 种灰色调。
            case PixelFormat.Format16bppRgb555: //指定格式为每像素 16 位；红色、绿色和蓝色分量各使用 5 位。剩余的 1 位未使用。
            case PixelFormat.Format16bppRgb565: //指定格式为每像素 16 位；红色分量使用 5 位，绿色分量使用 6 位，蓝色分量使用 5 位。
                return BitsPerPixel.Bit16;
            case PixelFormat.Format24bppRgb: //指定格式为每像素 24 位；红色、绿色和蓝色分量各使用 8 位。
                return BitsPerPixel.Bit24;
            case PixelFormat.Canonical: //默认像素格式，每像素 32 位。此格式指定 24 位颜色深度和一个 8 位 alpha 通道。
            case PixelFormat.Format32bppArgb: //指定格式为每像素 32 位；alpha、红色、绿色和蓝色分量各使用 8 位。
            case PixelFormat.Format32bppPArgb: //指定格式为每像素 32 位；alpha、红色、绿色和蓝色分量各使用 8 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
            case PixelFormat.Format32bppRgb: //指定格式为每像素 32 位；红色、绿色和蓝色分量各使用 8 位。剩余的 8 位未使用。
                return BitsPerPixel.Bit32;
            case PixelFormat.Format64bppArgb: //指定格式为每像素 64 位；alpha、红色、绿色和蓝色分量各使用 16 位。
            case PixelFormat.Format64bppPArgb: //指定格式为每像素 64 位；alpha、红色、绿色和蓝色分量各使用 16 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
                return BitsPerPixel.Bit64;
            case PixelFormat.Format48bppRgb: //指定格式为每像素 48 位；红色、绿色和蓝色分量各使用 16 位。
                return BitsPerPixel.Bit48;
            case PixelFormat.Format8bppIndexed: //指定格式为每像素 8 位而且已创建索引。因此颜色表中有 256 种颜色。
                return BitsPerPixel.Bit8;
            case PixelFormat.Format4bppIndexed: //指定格式为每像素 4 位而且已创建索引。
                return BitsPerPixel.Bit4;
            case PixelFormat.Format1bppIndexed: //指定像素格式为每像素 1 位，并指定它使用索引颜色。因此颜色表中有两种颜色。
                return BitsPerPixel.Bit1;
            case PixelFormat.Alpha: //像素数据包含没有进行过自左乘的 alpha 值。
            case PixelFormat.Gdi: //像素数据包含 GDI 颜色。
            case PixelFormat.Indexed: //该像素数据包含颜色索引值，这意味着这些值是系统颜色表中颜色的索引，而不是单个颜色值。
            case PixelFormat.Max: //此枚举的最大值。
            case PixelFormat.PAlpha: //像素数据包含没有进行过自左乘的 alpha 值。
            case PixelFormat.Extended: //保留。
            case PixelFormat.Undefined: //未定义像素格式。
            default:
                return BitsPerPixel.Undefined;
        }
    }

    /// <summary>
    /// 根据指定的图像<see cref="Bitmap"/>的属性判断图像的格式
    /// </summary>
    /// <param name="bmp">指定的图像</param>
    /// <returns>图像格式</returns>
    public static ImageFormat GetImageFormat(Bitmap? bmp)
    {
        if (bmp == null)
            return ImageFormat.Undefined;
        if ((int) bmp.PixelFormat == 8207)
            return ImageFormat.CMYK;
        switch (bmp.PixelFormat)
        {
            case PixelFormat.Format16bppArgb1555: //像素格式为每像素16位。该颜色信息指定32,768种色调，其中5位为红色，5位为绿色，5位为蓝色，1位为alpha。
            case PixelFormat.Format16bppRgb555: //指定格式为每像素 16 位；红色、绿色和蓝色分量各使用 5 位。剩余的 1 位未使用。
            case PixelFormat.Format16bppRgb565: //指定格式为每像素 16 位；红色分量使用 5 位，绿色分量使用 6 位，蓝色分量使用 5 位。
            case PixelFormat.Format24bppRgb: //指定格式为每像素 24 位；红色、绿色和蓝色分量各使用 8 位。
            case PixelFormat.Canonical: //默认像素格式，每像素 32 位。此格式指定 24 位颜色深度和一个 8 位 alpha 通道。
            case PixelFormat.Format32bppArgb: //指定格式为每像素 32 位；alpha、红色、绿色和蓝色分量各使用 8 位。
            case PixelFormat.Format32bppPArgb: //指定格式为每像素 32 位；alpha、红色、绿色和蓝色分量各使用 8 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
            case PixelFormat.Format32bppRgb: //指定格式为每像素 32 位；红色、绿色和蓝色分量各使用 8 位。剩余的 8 位未使用。
            case PixelFormat.Format64bppArgb: //指定格式为每像素 64 位；alpha、红色、绿色和蓝色分量各使用 16 位。
            case PixelFormat.Format64bppPArgb: //指定格式为每像素 64 位；alpha、红色、绿色和蓝色分量各使用 16 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
            case PixelFormat.Format48bppRgb: //指定格式为每像素 48 位；红色、绿色和蓝色分量各使用 16 位。
                return ImageFormat.RGB;
            case PixelFormat.Format16bppGrayScale: //像素格式为每像素 16 位。该颜色信息指定 65536 种灰色调。
            case PixelFormat.Format8bppIndexed: //指定格式为每像素 8 位而且已创建索引。因此颜色表中有 256 种颜色。
            case PixelFormat.Format4bppIndexed: //指定格式为每像素 4 位而且已创建索引。
            case PixelFormat.Format1bppIndexed: //指定像素格式为每像素 1 位，并指定它使用索引颜色。因此颜色表中有两种颜色。
                return ImageFormat.Gray;
            case PixelFormat.Alpha: //像素数据包含没有进行过自左乘的 alpha 值。
            case PixelFormat.Gdi: //像素数据包含 GDI 颜色。
            case PixelFormat.Indexed: //该像素数据包含颜色索引值，这意味着这些值是系统颜色表中颜色的索引，而不是单个颜色值。
            case PixelFormat.Max: //此枚举的最大值。
            case PixelFormat.PAlpha: //像素数据包含没有进行过自左乘的 alpha 值。
            case PixelFormat.Extended: //保留。
            case PixelFormat.Undefined: //未定义像素格式。
            default:
                return ImageFormat.Undefined;
        }
    }

    /// <summary>
    /// 更新图像到指定的对比度
    /// </summary>
    /// <param name="img">待更新的图像</param>
    /// <param name="wb">指定的对比度</param>
    public static void UpdateWhiteBalance(Image<Bgr, byte> img, WhiteBalance wb)
    {
        int avgR = 0, avgG = 0, avgB = 0;
        int sumR = 0, sumG = 0, sumB = 0;
        for (int h = 0; h < img.Height; ++h)
        {
            for (int w = 0; w < img.Width; ++w)
            {
                sumB += img.Data[h, w, 0];
                sumG += img.Data[h, w, 1];
                sumR += img.Data[h, w, 2];
            }
        }
        int size = img.Height * img.Width;
        avgB = sumB / size;
        avgG = sumG / size;
        avgR = sumR / size;
        //double k = (avgB + avgG + avgR) / 3;
        double k = 0.299 * avgR + 0.587 * avgG + 0.114 * avgB;

        switch (wb)
        {
            case WhiteBalance.AUTO:
                avgR = (sumR / size);
                avgG = (sumG / size);
                avgB = (sumB / size);
                break;
            case WhiteBalance.CLOUDY:
                avgR = (int)(sumR * 1.953125 / size);
                avgG = (int)(sumG * 1.0390625 / size);
                avgB = (int)(sumB / size);
                break;
            case WhiteBalance.DAYLIGHT:
                avgR = (int)(sumR * 1.2734375 / size);
                avgG = (int)(sumG / size);
                avgB = (int)(sumB * 1.0625 / size);
                break;
            case WhiteBalance.INCANDESCENCE:
                avgR = (int)(sumR * 1.2890625 / size);
                avgG = (int)(sumG / size);
                avgB = (int)(sumB * 1.0625 / size);
                break;
            case WhiteBalance.FLUORESCENT:
                avgR = (int)(sumR * 1.1875 / size);
                avgG = (int)(sumG / size);
                avgB = (int)(sumB * 1.3125 / size);
                break;
            case WhiteBalance.TUNGSTEN:
                avgR = (int)(sumR / size);
                avgG = (int)(sumG * 1.0078125 / size);
                avgB = (int)(sumB * 1.28125 / size);
                break;
            default:
                break;
        }

        double kr = k / avgR;
        double kg = k / avgG;
        double kb = k / avgB;
        double newB, newG, newR;
        for (int h = 0; h < img.Height; ++h)
        {
            for (int w = 0; w < img.Width; ++w)
            {
                newB = img.Data[h, w, 0] * kb;
                newG = img.Data[h, w, 1] * kg;
                newR = img.Data[h, w, 2] * kr;

                img.Data[h, w, 0] = (byte)(newB > 255 ? 255 : newB);
                img.Data[h, w, 1] = (byte)(newG > 255 ? 255 : newG);
                img.Data[h, w, 2] = (byte)(newR > 255 ? 255 : newR);
            }
        }

    }

    public static Bitmap? ImageFormatConverter(Bitmap? source, ImageFormat imageFormat, BitsPerPixel bpp)
    {
        if (source == null)
            return null;
        switch (imageFormat)
        {
            case ImageFormat.Gray:
                return ToGray(source, bpp);
            case ImageFormat.CMYK:
                return ToCMYK(source, bpp);
            case ImageFormat.Lab:
                return ToLab(source, bpp);
            case ImageFormat.RGB:
                default:
                return ToRGB(source, bpp);
        }
    }

    private static Bitmap ToCMYK(Bitmap source, BitsPerPixel bpp)
    {
        throw new NotImplementedException();
    }

    private static Bitmap ToLab(Bitmap source, BitsPerPixel bpp)
    {
        throw new NotImplementedException();
    }

    private static Bitmap ToRGB(Bitmap source, BitsPerPixel bpp)
    {
        throw new NotImplementedException();
    }

    private static Bitmap ToGray(Bitmap source, BitsPerPixel bpp)
    {
        switch (bpp)
        {
            case BitsPerPixel.Bit32:
            case BitsPerPixel.Bit24:
            case BitsPerPixel.Bit16:
            default:
                throw new NotSupportedException();
            case BitsPerPixel.Bit8:
            {
                var image = source.ToImage<Gray, byte>();
                return image.ToBitmap();
            }
        }
    }
}