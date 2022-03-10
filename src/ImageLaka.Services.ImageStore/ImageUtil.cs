using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using SixLabors.ImageSharp;
using Color = System.Drawing.Color;

namespace ImageLaka.ImageEngine
{
    public class ImageUtil
    {
        public static Bitmap Read(string path)
        {
            Bitmap bmp = new Bitmap(path);
            return bmp;
        }

        public static Bitmap? ToGray(Bitmap? source, ImageBits ib)
        {
            if (source == null)
                return null;
            switch (ib)
            {
                case ImageBits.Bit32:
                case ImageBits.Bit24:
                {
                    var image = source.ToImage<Gray, uint>();
                    return image.ToBitmap();
                }
                case ImageBits.Bit16:
                {
                    var image = source.ToImage<Gray, ushort>();
                    return image.ToBitmap();
                }
                case ImageBits.Bit8:
                default:
                {
                    var image = source.ToImage<Gray, byte>();
                    return image.ToBitmap();
                }
            }
        }
    }

    public enum ImageBits
    {
        Bit8, Bit16, Bit24, Bit32
    }
}
