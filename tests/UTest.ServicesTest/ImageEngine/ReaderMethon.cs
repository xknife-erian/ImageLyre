using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace UTest.ServicesTest.ImageEngine
{
    public class ReadMethod
    {
        public static void Read0(string path)
        {
            var curBitmap = new Bitmap(path);
            if (curBitmap != null)
            {
                Color curColor;
                int ret;
                for (int i = 0; i < curBitmap.Width; i++)
                {
                    for (int j = 0; j < curBitmap.Height; j++)
                    {
                        curColor = curBitmap.GetPixel(i, j);
                        ret = (int)(curColor.R * 0.299 + curColor.G * 0.587 + curColor.B * 0.114);
                        curBitmap.SetPixel(i, j, Color.FromArgb(ret, ret, ret));
                    }
                }
            }
        }

        public static void Read1(string path)
        {
            var curBitmap = new Bitmap(path);

            if (curBitmap != null)
            {
                Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
                BitmapData bmpData = curBitmap.LockBits(rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                int bytes = curBitmap.Width * curBitmap.Height * 3;
                byte[] rgbValues = new byte[bytes];
                Marshal.Copy(ptr, rgbValues, 0, bytes);
                for (int i = 0; i < rgbValues.Length; i += 3)
                {
                    var colorTemp = rgbValues[i + 2] * 0.299 + rgbValues[i + 1] * 0.587 + rgbValues[i] * 0.114;
                    rgbValues[i] = rgbValues[i + 1] = rgbValues[i + 2] = (byte)colorTemp;
                }
                Marshal.Copy(rgbValues, 0, ptr, bytes);
                curBitmap.UnlockBits(bmpData);
            }
        }

        public static void Read2(string path)
        {
            var curBitmap = new Bitmap(path);
            if (curBitmap != null)
            {
                Rectangle rect = new Rectangle(0, 0, curBitmap.Width, curBitmap.Height);
                BitmapData bmpData = curBitmap.LockBits(rect, ImageLockMode.ReadWrite, curBitmap.PixelFormat);
                byte temp = 0;
                unsafe
                {
                    byte* ptr = (byte*)(bmpData.Scan0);
                    for (int i = 0; i < bmpData.Height; i++)
                    {
                        for (int j = 0; j < bmpData.Width; j++)
                        {
                            temp = (byte)(0.299 * ptr[2] + 0.587 * ptr[1] + 0.114 * ptr[0]);
                            ptr[0] = ptr[1] = ptr[2] = temp;
                            ptr += 3;
                        }

                        ptr += bmpData.Stride - bmpData.Width * 3;
                    }
                }

                curBitmap.UnlockBits(bmpData);
            }
        }
    }
}
