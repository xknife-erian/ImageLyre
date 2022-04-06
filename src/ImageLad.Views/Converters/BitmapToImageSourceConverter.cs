using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace ImageLad.Views.Converters;

[ValueConversion(typeof(SKBitmap), typeof(WriteableBitmap))]
public class BitmapToImageSourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var bitmap = (SKBitmap) value;
        // TODO: 2022年3月13日，laka，不是每次都重新创建WriteableBitmap，而是通过WriteableBitmap.WritePixels()替换其缓存的数据，但是未成功。
        // WriteableBitmap srcBitmap;
        // if (value[1] != null && value[1] is WriteableBitmap)
        //     srcBitmap = (WriteableBitmap) value[1];
        return bitmap.ToWriteableBitmap();
    }

    /// <summary>Converts a binding target value to the source binding values.</summary>
    /// <param name="value">The value that the binding target produces.</param>
    /// <param name="targetTypes">
    ///     The array of types to convert to. The array length indicates the number and types of values
    ///     that are suggested for the method to return.
    /// </param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>An array of values that have been converted from the target value back to the source values.</returns>
    public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}