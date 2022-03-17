using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace ImageLaka.Views.Converters;

[ValueConversion(typeof(Bitmap), typeof(WriteableBitmap))]
public class BitmapToImageSourceConverter : IMultiValueConverter
{
    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
    {
        var bitmap = (Bitmap) value[0];
        // TODO: 2022年3月13日，laka，不是每次都重新创建WriteableBitmap，而是通过WriteableBitmap.WritePixels()替换其缓存的数据，但是未成功。
        // WriteableBitmap srcBitmap;
        // if (value[1] != null && value[1] is WriteableBitmap)
        //     srcBitmap = (WriteableBitmap) value[1];
        return BitmapToWriteableBitmap(bitmap);
    }

    /// <summary>Converts a binding target value to the source binding values.</summary>
    /// <param name="value">The value that the binding target produces.</param>
    /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and types of values that are suggested for the method to return.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>An array of values that have been converted from the target value back to the source values.</returns>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     将Bitmap 转换成WriteableBitmap
    /// </summary>
    public static WriteableBitmap BitmapToWriteableBitmap(Bitmap bmp)
    {
        var wb = CreateCompatibleWriteableBitmap(bmp);
        var format = bmp.PixelFormat;

        BitmapCopyToWriteableBitmap(bmp, wb, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, format);
        return wb;
    }

    /// <summary>
    ///     创建尺寸和格式与Bitmap兼容的WriteableBitmap
    ///     https://gitee.com/xknife/image-laka/wikis/%E7%9F%A5%E8%AF%86%E9%A2%86%E5%9F%9F/WPF/PixelFormats
    /// </summary>
    public static WriteableBitmap CreateCompatibleWriteableBitmap(Bitmap bmp)
    {
        System.Windows.Media.PixelFormat format;
        switch (bmp.PixelFormat)
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
                format = PixelFormats.Cmyk32;
                return new WriteableBitmap(bmp.Width, bmp.Height, 0, 0, format, null);
        }

        return new WriteableBitmap(bmp.Width, bmp.Height, 0, 0, format, null);
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