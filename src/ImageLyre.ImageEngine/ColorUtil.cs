using System.Drawing;

namespace ImageLyre.ImageEngine
{
    public static class ColorUtil
    {
        /// <summary>
        /// 获取灰度值
        /// </summary>
        /// <param name="red">红</param>
        /// <param name="green">绿</param>
        /// <param name="blue">蓝</param>
        /// <returns></returns>
        public static byte GetGrayValue(byte red, byte green, byte blue)
        {
            return (byte)(red * 19595 + green * 38469 + blue * 7472 >> 16);
        }


        /// <summary>
        /// 获取灰度值
        /// </summary>
        /// <param name="pixel">颜色</param>
        /// <returns></returns>
        public static byte GetGrayValue(Color pixel)
        {
            return GetGrayValue(pixel.R, pixel.G, pixel.B);
        }

        /// <summary>
        /// 获取灰度值
        /// </summary>
        /// <param name="pixel">颜色</param>
        /// <returns></returns>
        public static byte GetGrayValue(System.Windows.Media.Color pixel)
        {
            return GetGrayValue(pixel.R, pixel.G, pixel.B);
        }
    }
}
