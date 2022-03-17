using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageLaka.ImageEngine;
using ImageMagick;

namespace ImageLaka.Views.Utils
{
    public static class ImageMagickExtension
    {

        public static BitmapSource ToBitmapSource(this MagickImage src)
        {
            return ToBitmapSource(src, BitmapDensity.Ignore);
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
        public static BitmapSource ToBitmapSource(this MagickImage src, BitmapDensity bitmapDensity)
        {
            IMagickImage image = src;

            var mapping = "RGB";
            var format = PixelFormats.Rgb24;

            try
            {
                if (src.ColorSpace == ColorSpace.CMYK && !image.HasAlpha)
                {
                    mapping = "CMYK";
                    format = PixelFormats.Cmyk32;
                }
                else
                {
                    if (src.ColorSpace != ColorSpace.sRGB)
                    {
                        image = (MagickImage)src.Clone();
                        image.ColorSpace = ColorSpace.sRGB;
                    }

                    if (image.HasAlpha)
                    {
                        mapping = "BGRA";
                        format = PixelFormats.Bgra32;
                    }
                }

                var step = format.BitsPerPixel / 8;
                var stride = src.Width * step;

                using (IPixelCollection<ushort> pixels = src.GetPixelsUnsafe())
                {
                    var bytes = pixels.ToByteArray(mapping);
                    var dpi = src.GetDpi(bitmapDensity);
                    return BitmapSource.Create(src.Width, src.Height, dpi.X, dpi.Y, format, null, bytes, stride);
                }
            }
            finally
            {
                if (!ReferenceEquals(src, image))
                    image.Dispose();
            }
        }

    }
}
