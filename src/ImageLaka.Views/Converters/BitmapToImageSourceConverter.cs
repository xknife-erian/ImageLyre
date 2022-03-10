using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace ImageLaka.Views.Converters;

[ValueConversion(typeof(BitmapData), typeof(ImageSource))]
public class BitmapToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return BitmapToWriteableBitmap((Bitmap) value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     将Bitmap 转换成WriteableBitmap
    /// </summary>
    public static WriteableBitmap BitmapToWriteableBitmap(Bitmap src)
    {
        var wb = CreateCompatibleWriteableBitmap(src);
        var format = src.PixelFormat;

        BitmapCopyToWriteableBitmap(src, wb, new Rectangle(0, 0, src.Width, src.Height), 0, 0, format);
        return wb;
    }

    /// <summary>
    ///     创建尺寸和格式与Bitmap兼容的WriteableBitmap
    /// </summary>
    public static WriteableBitmap CreateCompatibleWriteableBitmap(Bitmap src)
    {
        System.Windows.Media.PixelFormat format;
        switch (src.PixelFormat)
        {
            case PixelFormat.Format8bppIndexed:
                format = PixelFormats.Gray8;
                break;
            case PixelFormat.Format16bppRgb555:
                format = PixelFormats.Bgr555;
                break;
            case PixelFormat.Format16bppRgb565:
                format = PixelFormats.Bgr565;
                break;
            case PixelFormat.Format24bppRgb:
                format = PixelFormats.Bgr24;
                break;
            case PixelFormat.Format32bppRgb:
                format = PixelFormats.Bgr32;
                break;
            case PixelFormat.Format32bppPArgb:
                format = PixelFormats.Pbgra32;
                break;
            case PixelFormat.Format32bppArgb:
                format = PixelFormats.Bgra32;
                break;
            default:
                return null;
        }

        return new WriteableBitmap(src.Width, src.Height, 0, 0, format, null);
    }

    /// <summary>
    ///     将Bitmap数据写入WriteableBitmap中
    /// </summary>
    public static void BitmapCopyToWriteableBitmap(Bitmap src, WriteableBitmap dst, Rectangle srcRect, 
        int destinationX, int destinationY, PixelFormat srcPixelFormat)
    {
        var data = src.LockBits(new Rectangle(new Point(0, 0), src.Size),
            ImageLockMode.ReadOnly, srcPixelFormat);
        dst.WritePixels(new Int32Rect(srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height), data.Scan0,
            data.Height * data.Stride, data.Stride, destinationX, destinationY);
        src.UnlockBits(data);
    }
}