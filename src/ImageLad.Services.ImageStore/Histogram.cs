using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace ImageLad.ImageEngine
{
    /// <summary>
    /// 图像直方图是一种反映图像色调分布的直方图，其绘制每个色调值的像素数。每个色调值的像素数也称为频率(frequency)。
    /// https://juejin.cn/post/7059551594982932511
    /// </summary>
    public class Histogram
    {
        /// <summary>
        /// 总像素数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 均值
        /// </summary>
        public double Mean { get; set; }
        /// <summary>
        /// 标准偏差
        /// </summary>
        public double StdDev { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public int Min { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public int Max { get; set; }
        /// <summary>
        /// 模态灰度值
        /// </summary>
        public int Mode { get; set; }
        /// <summary>
        /// 图像灰度直方图数据
        /// </summary>
        public double[] Array { get; private set; } = new double[256];

        /// <summary>
        /// 计算指定的图像灰度直方图
        /// </summary>
        /// <param name="bmp">指定的图像</param>
        /// <returns>灰度直方图数据数组</returns>
        public static Histogram Compute(Bitmap bmp)
        {
            var his = new Histogram();
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);//PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* ptr = (byte*)data.Scan0;//每个像素的颜色数组
                int remain = data.Stride - data.Width * 3; //data.Stride描述图像的跨距宽度（也称为扫描宽度）。
                for (int i = 0; i < data.Height; i++)
                {
                    for (int j = 0; j < data.Width; j++)
                    {
                        var mean = ptr[0] + ptr[1] + ptr[2];
                        mean /= 3;
                        his.Array[mean]++;
                        ptr += 3;
                    }
                    ptr += remain;
                }
            }

            his.Count = data.Height * data.Width;
            bmp.UnlockBits(data);
            his.StdDev = his.Array.StandardDeviation();
            his.Mean = his.Array.Mean();
            his.Max = (int) his.Array.Max();
            his.Min = (int) his.Array.Min();
            foreach (var d in his.Array)
            {
                his.Count += (int) d;
            }

            return his;
        }

        /// <summary>
        /// 直方图均衡化 Histogram Equalization
        /// 假如图像的灰度分布不均匀，其灰度分布集中在较窄的范围内，使图像的细节不够清晰，对比度较低。通常采用直方图均衡化及直方图规定化两种变换，使图像的灰度范围拉开或使灰度均匀分布，从而增大反差，使图像细节清晰，以达到增强的目的。
        /// 直方图均衡化，对图像进行非线性拉伸，重新分配图像的灰度值，使一定范围内图像的灰度值大致相等。这样，原来直方图中间的峰值部分对比度得到增强，而两侧的谷底部分对比度降低，输出图像的直方图是一个较为平坦的直方图。
        /// 
        /// 均衡化算法:直方图的均衡化实际也是一种灰度的变换过程，将当前的灰度分布通过一个变换函数，变换为范围更宽、灰度分布更均匀的图像。也就是将原图像的直方图修改为在整个灰度区间内大致均匀分布，因此扩大了图像的动态范围，增强图像的对比度。通常均衡化选择的变换函数是灰度的累积概率，直方图均衡化算法的步骤：
        /// 
        /// 计算原图像的灰度直方图 P(Sk)=nknP(Sk)=nkn，其中nn为像素总数，nknk为灰度级SkSk的像素个数
        ///     计算原始图像的累积直方图 CDF(Sk)=∑i=0knin=∑i=0kPs(Si)CDF(Sk)=∑i=0knin=∑i=0kPs(Si)
        /// Dj=L⋅CDF(Si)Dj=L⋅CDF(Si)，其中 DjDj是目的图像的像素，CDF(Si)CDF(Si)是源图像灰度为i的累积分布，L是图像中最大灰度级（灰度图为255）
        /// 灰度直方图均衡化实现的步骤
        /// 1.统计灰度级中每个像素在整幅图像中的个数
        /// 2.计算每个灰度级占图像中的概率分布
        /// 3.计算累计分布概率
        /// 4.计算均衡化之后的灰度值
        /// 5.映射回原来像素的坐标的像素值
        /// https://www.cnblogs.com/-wenli/p/11496620.html
        /// </summary>
        public static void 灰度直方图均衡化实现()
        {
            /*
            //img_size为图像大小
            //Image_Use为图像数组　　　　　　　　　　　　//Use_ROWS为行，Use_Line为列
            float img_size = Use_ROWS * Use_Line * 1.0;
            int count_data[256], huidu_data[256]; //计数统计、均衡化的灰度值
            float midu_data[256], leijimidu_data[256];  //概率密度、累计概率密度
                                                        //数组初始化
            memset(count_data, 0, sizeof(count_data));
            memset(midu_data, 0.0, sizeof(midu_data));
            memset(leijimidu_data, 0.0, sizeof(leijimidu_data));
            memset(huidu_data, 0.0, sizeof(huidu_data));

            //1.统计灰度级中每个像素在整幅图像中的个数
            for (int i = 0; i < Use_ROWS; i++)
            {
                for (int j = 0; j < Use_Line; j++)
                {
                    count_data[Image_Use[i][j]]++;
                }
            }
            //2.计算每个灰度级占图像中的概率分布
            for (int i = 0; i < 256; i++)
            {
                midu_data[i] = count_data[i] / img_size;
            }
            //3.计算累计分布概率
            leijimidu_data[0] = midu_data[0];
            for (int i = 1; i < 256; i++)
            {
                leijimidu_data[i] = midu_data[i] + leijimidu_data[i - 1];
            }
            //4.计算均衡化之后的灰度值
            for (int i = 0; i < 256; i++)
            {
                huidu_data[i] = (int)(255 * leijimidu_data[i]);
            }
            //5.映射回原来像素的坐标的像素值
            for (int i = 0; i < Use_ROWS; i++)
            {
                for (int j = 0; j < Use_Line; j++)
                {
                    Image_Use[i][j] = huidu_data[Image_Use[i][j]];
                }
            }
            */
        }
    }
}
