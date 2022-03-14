using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ImageLaka.ImageEngine;

public class ImageUtil
{
    public static Bitmap Read(string path)
    {
        if (File.Exists(path)) return new Bitmap(path);

        return new Bitmap(100, 100);
    }

    public static Bitmap? ToGray(Bitmap? source, BitsPerPixel ib)
    {
        if (source == null)
            return null;
        switch (ib)
        {
            case BitsPerPixel.Bit32:
            case BitsPerPixel.Bit24:
            {
                var image = source.ToImage<Gray, uint>();
                return image.ToBitmap();
            }
            case BitsPerPixel.Bit16:
            {
                var image = source.ToImage<Gray, ushort>();
                return image.ToBitmap();
            }
            case BitsPerPixel.Bit8:
            default:
            {
                var image = source.ToImage<Gray, byte>();
                return image.ToBitmap();
            }
        }
    }
}