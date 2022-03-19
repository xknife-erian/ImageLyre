using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using ImageLad.ImageEngine.Enums;

namespace ImageLad.ImageEngine;

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
        if ((int)bmp.PixelFormat == 8207)
            return BitsPerPixel.Bit32;
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
    public static ImageLadFormat GetImageFormat(Bitmap? bmp)
    {
        if (bmp == null)
            return ImageLadFormat.Undefined;
        if ((int) bmp.PixelFormat == 8207)
            return ImageLadFormat.CMYK;
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
                return ImageLadFormat.RGB;
            case PixelFormat.Format16bppGrayScale: //像素格式为每像素 16 位。该颜色信息指定 65536 种灰色调。
            case PixelFormat.Format8bppIndexed: //指定格式为每像素 8 位而且已创建索引。因此颜色表中有 256 种颜色。
            case PixelFormat.Format4bppIndexed: //指定格式为每像素 4 位而且已创建索引。
            case PixelFormat.Format1bppIndexed: //指定像素格式为每像素 1 位，并指定它使用索引颜色。因此颜色表中有两种颜色。
                return ImageLadFormat.Gray;
            case PixelFormat.Alpha: //像素数据包含没有进行过自左乘的 alpha 值。
            case PixelFormat.Gdi: //像素数据包含 GDI 颜色。
            case PixelFormat.Indexed: //该像素数据包含颜色索引值，这意味着这些值是系统颜色表中颜色的索引，而不是单个颜色值。
            case PixelFormat.Max: //此枚举的最大值。
            case PixelFormat.PAlpha: //像素数据包含没有进行过自左乘的 alpha 值。
            case PixelFormat.Extended: //保留。
            case PixelFormat.Undefined: //未定义像素格式。
            default:
                return ImageLadFormat.Undefined;
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
            case WhiteBalance.Auto:
                avgR = (sumR / size);
                avgG = (sumG / size);
                avgB = (sumB / size);
                break;
            case WhiteBalance.Cloudy:
                avgR = (int)(sumR * 1.953125 / size);
                avgG = (int)(sumG * 1.0390625 / size);
                avgB = (int)(sumB / size);
                break;
            case WhiteBalance.Daylight:
                avgR = (int)(sumR * 1.2734375 / size);
                avgG = (int)(sumG / size);
                avgB = (int)(sumB * 1.0625 / size);
                break;
            case WhiteBalance.Incandescence:
                avgR = (int)(sumR * 1.2890625 / size);
                avgG = (int)(sumG / size);
                avgB = (int)(sumB * 1.0625 / size);
                break;
            case WhiteBalance.Fluorescent:
                avgR = (int)(sumR * 1.1875 / size);
                avgG = (int)(sumG / size);
                avgB = (int)(sumB * 1.3125 / size);
                break;
            case WhiteBalance.Tungsten:
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

    public static Bitmap? ImageFormatConverter(Bitmap? source, ImageLadFormat imageLadFormat, BitsPerPixel bpp)
    {
        if (source == null)
            return null;
        switch (imageLadFormat)
        {
            case ImageLadFormat.Gray:
                return ToGray(source, bpp);
            case ImageLadFormat.CMYK:
                return ToCMYK(source, bpp);
            case ImageLadFormat.Lab:
                return ToLab(source, bpp);
            case ImageLadFormat.RGB:
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

    /// <summary>
    /// 计算指定的图像灰度直方图
    /// </summary>
    /// <param name="bmp">指定的图像</param>
    /// <returns>灰度直方图数据数组</returns>
    public static int[] GetHistogram(Bitmap bmp)
    {
        var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);//PixelFormat.Format24bppRgb);
        int[] histogram = new int[256];
        unsafe
        {
            byte* ptr = (byte*)data.Scan0;
            int remain = data.Stride - data.Width * 3;
            for (int i = 0; i < histogram.Length; i++)
                histogram[i] = 0;
            for (int i = 0; i < data.Height; i++)
            {
                for (int j = 0; j < data.Width; j++)
                {
                    int mean = ptr[0] + ptr[1] + ptr[2];
                    mean /= 3;
                    histogram[mean]++;
                    ptr += 3;
                }
                ptr += remain;
            }
        }
        bmp.UnlockBits(data);
        return histogram;
    }

    /// <summary>
    /// 直方图均衡化 Histogram Equalization
    /// 假如图像的灰度分布不均匀，其灰度分布集中在较窄的范围内，使图像的细节不够清晰，对比度较低。通常采用直方图均衡化及直方图规定化两种变换，使图像的灰度范围拉开或使灰度均匀分布，从而增大反差，使图像细节清晰，以达到增强的目的。
    /// 直方图均衡化，对图像进行非线性拉伸，重新分配图像的灰度值，使一定范围内图像的灰度值大致相等。这样，原来直方图中间的峰值部分对比度得到增强，而两侧的谷底部分对比度降低，输出图像的直方图是一个较为平坦的直方图。
    /// 
    /// 均衡化算法
    ///     直方图的均衡化实际也是一种灰度的变换过程，将当前的灰度分布通过一个变换函数，变换为范围更宽、灰度分布更均匀的图像。也就是将原图像的直方图修改为在整个灰度区间内大致均匀分布，因此扩大了图像的动态范围，增强图像的对比度。通常均衡化选择的变换函数是灰度的累积概率，直方图均衡化算法的步骤：
    /// 
    /// 计算原图像的灰度直方图 P(Sk)=nknP(Sk)=nkn，其中nn为像素总数，nknk为灰度级SkSk的像素个数
    ///     计算原始图像的累积直方图 CDF(Sk)=∑i=0knin=∑i=0kPs(Si)CDF(Sk)=∑i=0knin=∑i=0kPs(Si)
    /// Dj=L⋅CDF(Si)Dj=L⋅CDF(Si)，其中 DjDj是目的图像的像素，CDF(Si)CDF(Si)是源图像灰度为i的累积分布，L是图像中最大灰度级（灰度图为255）
    /// 灰度直方图均衡化实现的步骤
    /// 1.统计灰度级中每个像素在整幅图像中的个数
    /// 2.计算每个灰度级占图像中的概率分布
    /// 3.计算累计分布概率
    /// 4.计算均衡化之后的灰度值
    /// 5.映射回原来像素的坐标的像素值
    /// https://www.cnblogs.com/-wenli/p/11496620.html
    /// </summary>
    public static void 灰度直方图均衡化实现()
    {
        /*
        //img_size为图像大小
        //Image_Use为图像数组　　　　　　　　　　　　//Use_ROWS为行，Use_Line为列
        float img_size = Use_ROWS * Use_Line * 1.0;
        int count_data[256], huidu_data[256]; //计数统计、均衡化的灰度值
        float midu_data[256], leijimidu_data[256];  //概率密度、累计概率密度
                                                    //数组初始化
        memset(count_data, 0, sizeof(count_data));
        memset(midu_data, 0.0, sizeof(midu_data));
        memset(leijimidu_data, 0.0, sizeof(leijimidu_data));
        memset(huidu_data, 0.0, sizeof(huidu_data));

        //1.统计灰度级中每个像素在整幅图像中的个数
        for (int i = 0; i < Use_ROWS; i++)
        {
            for (int j = 0; j < Use_Line; j++)
            {
                count_data[Image_Use[i][j]]++;
            }
        }
        //2.计算每个灰度级占图像中的概率分布
        for (int i = 0; i < 256; i++)
        {
            midu_data[i] = count_data[i] / img_size;
        }
        //3.计算累计分布概率
        leijimidu_data[0] = midu_data[0];
        for (int i = 1; i < 256; i++)
        {
            leijimidu_data[i] = midu_data[i] + leijimidu_data[i - 1];
        }
        //4.计算均衡化之后的灰度值
        for (int i = 0; i < 256; i++)
        {
            huidu_data[i] = (int)(255 * leijimidu_data[i]);
        }
        //5.映射回原来像素的坐标的像素值
        for (int i = 0; i < Use_ROWS; i++)
        {
            for (int j = 0; j < Use_Line; j++)
            {
                Image_Use[i][j] = huidu_data[Image_Use[i][j]];
            }
        }
        */
    }
}