using System;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLaka.Views.Converters
{
    [ValueConversion(typeof(BitmapData), typeof(ImageSource))]
    public class ImageFileSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BitmapData data = (BitmapData)value;
            WriteableBitmap bmp = new WriteableBitmap(
                data.Width, data.Height,
                96, 96,
                PixelFormats.Gray16,
                null);
            int sourceBufferSize = data.Height * data.Stride;
            bmp.WritePixels(new System.Windows.Int32Rect(0, 0, data.Width, data.Height), 
                data.Scan0, sourceBufferSize, data.Stride, 0, 0);
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
