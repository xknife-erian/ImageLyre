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

namespace ImageLaka.ImageEngine
{
    public class ImageUtil
    {
        public static Bitmap Read(string path)
        {
            Bitmap bmp = new Bitmap(path);
            return bmp;
        }

        public static Bitmap Gray(Bitmap source)
        {
            if (source == null)
                return null;
            Bitmap bmp = new Bitmap(source.Width, source.Height);
            using Image<Bgr, Byte> img = new Image<Bgr, Byte>(500,500);
            return bmp;
        }

        /// <summary>
        /// 图像灰度化
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static Bitmap ToGray(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    //利用公式计算灰度值
                    int gray = (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);
                    Color newColor = Color.FromArgb(gray, gray, gray);
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }
    }
}
