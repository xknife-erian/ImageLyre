using System.Drawing;
using ImageMagick;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;
using OpenCvSharp;
using SkiaSharp;

namespace ImageLad.ImageEngine.Analyze
{
    /// <summary>
    /// 图像的灰度直方图是一种反映图像色调分布的直方图，其绘制每个色调值的像素数。每个色调值的像素数也称为频率(frequency)。
    /// https://juejin.cn/post/7059551594982932511
    /// </summary>
    public class GrayHistogram
    {
        /// <summary>
        /// 总像素数
        /// </summary>
        public long Count { get; set; }
        /// <summary>
        /// 灰度的平均水平
        /// https://zouzhongliang.com/index.php/2019/04/12/阈值-灰度平均值计算方法/
        /// </summary>
        public double Mean { get; set; }
        /// <summary>
        /// 平均方差是衡量一个样本波动大小的量，对图像来说，平均方差反应的是图像高频部分的大小。方差小，则图片看着较暗；方差大，则图片看着较亮。
        /// </summary>
        public double StdDev { get; set; }
        /// <summary>
        /// 选择范围内的最小灰度值。
        /// </summary>
        public int Min { get; set; }
        /// <summary>
        /// 选择范围内的最大灰度值。
        /// </summary>
        public int Max { get; set; }
        /// <summary>
        /// 模式灰度值 - 在选择范围内最经常出现的灰度值。相当于直方图中的最高峰。Item1:灰度值;Item2:该灰度值的像素数。
        /// </summary>
        public (int,double) Mode { get; set; }
        /// <summary>
        /// 图像灰度直方图数据
        /// </summary>
        public double[] Array { get; } = new double[256];

        #region Overrides of Object

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Count: {Count}; Mean: {Math.Round(Mean, 3)}; StdDev: {Math.Round(StdDev, 3)}; Min: {Min}; Max: {Max}; Mode: {Mode};";
        }

        #endregion

        /// <summary>
        /// 计算指定的图像灰度直方图
        /// </summary>
        /// <param name="bmp">指定的图像</param>
        /// <param name="gf">转换灰度的算法</param>
        /// <returns>灰度直方图数据数组</returns>
        public static GrayHistogram Compute(MagickImage bmp, GrayFormula gf)
        {
            double m = 0.0;
            double n = 0.0;
            int j = 0;

            var his = new GrayHistogram();
            var pixels = bmp.GetPixels();
            for (int i = 0; i < pixels.LongCount(); i++)
            {
                int mean = 0;
                var ptr = pixels.GetPixel(0, 0);ptr.
                switch (gf) //计算灰度值
                {
                    case GrayFormula.Average:
                        mean = ptr.Blue + ptr.Green + ptr.Red;
                        mean /= 3;
                        break;
                    case GrayFormula.Weighted:
                        //彩色转灰度，有一个很著名的心理学公式：
                        //Gray = R*0.299 + G*0.587 + B*0.114
                        //为避免低速的浮点运算，放大1000倍来实现整数运算算法
                        mean = (114 * ptr.Blue + 587 * ptr.Green + 299 * ptr.Red + 500) / 1000;
                        break;
                }

                //计算均值
                his.Mean += mean;

                //计算标准方差
                j++;
                double tmpM = m;
                m += (mean - tmpM) / j;
                n += (mean - tmpM) * (mean - m);

                his.Array[mean]++;
            }

            his.Count = pixels.LongCount();
            his.Mean /= pixels.LongCount();//均值
            his.StdDev = Math.Sqrt(n / (j - 1));//标准方差
            his.Mode = CalculateModeValue(his.Array);
            for (int i = 0; i < his.Array.Length; i++)
            {
                if (his.Array[i] != 0)
                {
                    his.Min = i;//最小值
                    break;
                }
            }

            for (int i = his.Array.Length - 1; i >= 0; i--)
            {
                if (his.Array[i] != 0)
                {
                    his.Max = i;//最大值
                    break;
                }
            }

            foreach (var d in his.Array)
            {
                his.Count += (int) d;
            }
            return his;
        }

        private static (int, double) CalculateModeValue(double[] array)
        {
            var value = (0, 0d);
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] > value.Item2)
                    value = (i, array[i]);
            }
            return value;
        }

        /// <summary>
        /// 直方图均衡化 GrayHistogram Equalization
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

    public enum GrayFormula
    {
        Average, Weighted
    }
}
