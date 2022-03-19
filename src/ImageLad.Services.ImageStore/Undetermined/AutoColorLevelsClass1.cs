using System.Drawing.Imaging;

namespace ImageLad.ImageEngine.Undetermined
{
    public class AutoColorLevelsClass1
    {
        /// <summary>
        /// 模拟Photoshop的自动色阶
        /// </summary>
        /// <param name="bmpd"></param>
        /// <param name="pbmpd"></param>
        public static unsafe void Method(BitmapData bmpd, BitmapData pbmpd)
        {
            int i = 0, j = 0, k = 0;
            int height = bmpd.Height, width = bmpd.Width;
            double cutParam = 0.1d; //参照PhotoShop中的自动色阶的裁剪参数，此处设置为为0.1%
            double doubleThreshod = bmpd.Height * bmpd.Width * cutParam * 0.01;  //由于是0.1%，所以再×0.01
            int[,] histBgr = new int[3, 256]; //B, G, R
            byte[,] speedBgr = new byte[3, 256];
            int threshold = Convert.ToInt32(doubleThreshod);
            int newMin = 0;
            int newMax = 0;

            byte* scan0 = (byte*)bmpd.Scan0.ToPointer();
            byte* pscan0 = (byte*)pbmpd.Scan0.ToPointer();
            byte* ptr, pptr;

            //获得直方图数组
            for (i = 0; i < bmpd.Height; i++)
            {
                ptr = scan0 + i * bmpd.Stride;

                for (j = 0; j < bmpd.Width; j++)
                {
                    for (k = 0; k < 3; k++)
                    {
                        histBgr[k, *ptr++] += 1;
                    }
                }
            }

            //逐一比对每个像素RGB三种颜色的值，分别从右向左(取NewMax)和从左向右(取NewMin)获取该图片的RGB的最小值和最大值
            for (k = 0; k < 3; k++)
            {
                var integral = 0;
                for (i = 0; i <= 255; i++)
                {
                    integral += histBgr[k, i];
                    if (integral >= threshold)
                    {
                        newMin = i;
                        break;
                    }
                }
                integral = 0;
                for (i = 255; i >= 0; i--)
                {
                    integral += histBgr[k, i];
                    if (integral > threshold)
                    {
                        newMax = i;
                        break;
                    }
                }
                //将根据上步获得的最大值和最小值，将当前照片的值进行线性映射，以此获得新的直方图数组
                for (i = 0; i <= 255; i++)
                {
                    if (i <= newMin)
                    {
                        speedBgr[k, i] = 0;
                    }
                    else if (i >= newMax)
                    {
                        speedBgr[k, i] = 255;
                    }
                    else
                    {
                        speedBgr[k, i] = (byte)((i - newMin) * 255 / (newMax - newMin));
                    }
                }
            }

            //根据新的直方图对每个像素的RGB的数据进行修改
            for (i = 0; i < bmpd.Height; i++)
            {
                ptr = scan0 + i * bmpd.Stride;
                pptr = pscan0 + i * pbmpd.Stride;

                for (j = 0; j < bmpd.Width; j++)
                {
                    for (k = 0; k < 3; k++)
                    {
                        *pptr++ = speedBgr[k, *ptr++];
                    }
                }
            }
        }
    }
}
