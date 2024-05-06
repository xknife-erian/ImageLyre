using System.Windows.Media;
using ImagingPixelFormat = System.Drawing.Imaging.PixelFormat;
using MediaPixelFormat = System.Windows.Media.PixelFormat;

namespace ImageLyre.UI.Views.Utils;

public class Util
{
    public static MediaPixelFormat TranslateBitmapFormat(ImagingPixelFormat pixelFormat)
    {
        MediaPixelFormat format;
        if ((int) pixelFormat == 8207)
            format = PixelFormats.Cmyk32;
        else
            switch (pixelFormat)
            {
                case ImagingPixelFormat.Format8bppIndexed:
                    format = PixelFormats.Gray8;
                    break;
                case ImagingPixelFormat.Format16bppRgb555:
                    format = PixelFormats.Bgr555;
                    break;
                case ImagingPixelFormat.Format16bppRgb565:
                    format = PixelFormats.Bgr565;
                    break;
                case ImagingPixelFormat.Format24bppRgb:
                    format = PixelFormats.Bgr24;
                    break;
                case ImagingPixelFormat.Format32bppRgb:
                    format = PixelFormats.Bgr32;
                    break;
                case ImagingPixelFormat.Format32bppPArgb:
                    format = PixelFormats.Pbgra32;
                    break;
                case ImagingPixelFormat.Format32bppArgb:
                    format = PixelFormats.Bgra32;
                    break;
            }

        return format;
    }
}