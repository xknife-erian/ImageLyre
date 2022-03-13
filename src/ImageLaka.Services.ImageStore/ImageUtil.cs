using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using SixLabors.ImageSharp;
using Color = System.Drawing.Color;
using Size = SixLabors.ImageSharp.Size;

namespace ImageLaka.ImageEngine
{
    public class ImageUtil
    {
        public static Bitmap Read(string path)
        {
            if (File.Exists(path))
            {
                return new Bitmap(path);
            }

            return new Bitmap(100,100);
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
}
