using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace ImageMagick;

public static class ImageMagickExtensions
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive.")]
    public static Bitmap ToBitmap(this MagickImage src, BitmapDensity bitmapDensity)
    {
        var mapping = "BGR";
        var format = PixelFormat.Format24bppRgb;

        MagickImage image = src;
        try
        {
            if (src.ColorSpace != ColorSpace.sRGB)
            {
                image = (MagickImage) (src.Clone());
                image.ColorSpace = ColorSpace.sRGB;
            }

            if (src.HasAlpha)
            {
                mapping = "BGRA";
                format = PixelFormat.Format32bppArgb;
            }

            using (IPixelCollection<float> pixels = image.GetPixelsUnsafe())
            {
                var bitmap = new Bitmap(image.Width, image.Height, format);
                var rect = new Rectangle(0, 0, image.Width, image.Height);
                var data = bitmap.LockBits(rect, ImageLockMode.ReadWrite, format);
                var destination = data.Scan0;
                for (var y = 0; y < image.Height; y++)
                {
                    var bytes = pixels.ToByteArray(0, y, image.Width, 1, mapping);
                    Marshal.Copy(bytes, 0, destination, bytes.Length);
                    destination = new IntPtr(destination.ToInt64() + data.Stride);
                }

                bitmap.UnlockBits(data);

                SetBitmapDensity(src, bitmap, bitmapDensity);
                return bitmap;
            }
        }
        finally
        {
            if (!ReferenceEquals(image, src))
                image.Dispose();
        }
    }

    public static Bitmap ToBitmap(this MagickImage src)
    {
        return ToBitmap(src, BitmapDensity.Ignore);
    }

    public static Bitmap ToBitmap(this MagickImage src, ImageFormat imageFormat)
    {
        return ToBitmap(src, imageFormat, BitmapDensity.Ignore);
    }

    public static Bitmap ToBitmap(this MagickImage src, ImageFormat imageFormat, BitmapDensity bitmapDensity)
    {
        src.Format = GetMagickFormatFromImageFormat(imageFormat);

        var memStream = new MemoryStream();
        src.Write(memStream);
        memStream.Position = 0;

        /* Do not dispose the memStream, the bitmap owns it. */
        var bitmap = new Bitmap(memStream);

        SetBitmapDensity(src, bitmap, bitmapDensity);

        return bitmap;
    }

    public static void FromBitmap(this MagickImage src, Bitmap bitmap)
    {
        using (var memStream = new MemoryStream())
        {
            if (IsSupportedImageFormat(bitmap.RawFormat))
                bitmap.Save(memStream, bitmap.RawFormat);
            else
                bitmap.Save(memStream, ImageFormat.Bmp);

            memStream.Position = 0;
            src.Read(memStream);
        }
    }

    private static bool IsSupportedImageFormat(ImageFormat format)
    {
        return format.Guid.Equals(ImageFormat.Bmp.Guid) ||
               format.Guid.Equals(ImageFormat.Gif.Guid) ||
               format.Guid.Equals(ImageFormat.Icon.Guid) ||
               format.Guid.Equals(ImageFormat.Jpeg.Guid) ||
               format.Guid.Equals(ImageFormat.Png.Guid) ||
               format.Guid.Equals(ImageFormat.Tiff.Guid);
    }

    public static void SetBitmapDensity(this MagickImage src, Bitmap bitmap, BitmapDensity bitmapDensity)
    {
        if (bitmapDensity == BitmapDensity.Use)
        {
            var dpi = GetDpi(src, bitmapDensity);
            bitmap.SetResolution((float) dpi.X, (float) dpi.Y);
        }
    }

    public static Density GetDpi(this MagickImage src, BitmapDensity bitmapDensity)
    {
        if (bitmapDensity == BitmapDensity.Ignore || src.Density.Units == DensityUnit.Undefined &&
            src.Density.X == 0 && src.Density.Y == 0)
            return new Density(96);

        return src.Density.ChangeUnits(DensityUnit.PixelsPerInch);
    }

    public static MagickFormat GetMagickFormatFromImageFormat(ImageFormat format)
    {
        if (format == ImageFormat.Bmp || format == ImageFormat.MemoryBmp)
            return MagickFormat.Bmp;
        if (format == ImageFormat.Gif)
            return MagickFormat.Gif;
        if (format == ImageFormat.Icon)
            return MagickFormat.Icon;
        if (format == ImageFormat.Jpeg)
            return MagickFormat.Jpeg;
        if (format == ImageFormat.Png)
            return MagickFormat.Png;
        if (format == ImageFormat.Tiff)
            return MagickFormat.Tiff;
        throw new NotSupportedException("Unsupported image format: " + format);
    }
}