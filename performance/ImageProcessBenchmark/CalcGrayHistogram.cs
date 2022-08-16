using System.Drawing;
using BenchmarkDotNet.Attributes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageLyric.ImageEngine.Analyze;
using OpenCvSharp;
using ImreadModes = Emgu.CV.CvEnum.ImreadModes;
using Mat = Emgu.CV.Mat;

namespace ImageProcessBenchmark;

[MemoryDiagnoser]
public class CalcGrayHistogram
{
    private const string IMAGE = @".\Assets\25.jpg";

    private static string GetImagePath()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, IMAGE);
        return path;
    }

    [Benchmark]
    public void EmguCV_CalcHist()
    {
        // 2. 原图转灰度
        var imgGray = new Mat(GetImagePath(), ImreadModes.Grayscale);
        VectorOfMat vMat = new VectorOfMat();
        vMat.Push(imgGray);

        // 3. 计算直方图
        int[] channels = { 0 };  //初始化数组
        float[] ranges = { 0, 255 };
        int[] histSize = { 256 };
        CvInvoke.CalcHist(vMat, channels, new Mat(), new Mat(), histSize, ranges, false);
    }

    [Benchmark]
    public void OpenCVSharp_CalcHist()
    {
        OpenCvSharp.Mat panda = new OpenCvSharp.Mat(GetImagePath(), OpenCvSharp.ImreadModes.Grayscale);//读取为灰度图
        OpenCvSharp.Mat[] mats = new OpenCvSharp.Mat[] { panda };//一张图片，初始化为panda

        int[] channels = new int[] { 0 };//一个通道,初始化为通道0
        int[] histsize = new int[] { 256 };//一个通道，初始化为256箱子
        OpenCvSharp.Rangef[] range = new OpenCvSharp.Rangef[1];//一个通道，值范围
        range[0] = new Rangef(0F, 256F);//从0开始（含）到256结束（不含）
        Cv2.CalcHist(mats, channels, new OpenCvSharp.Mat(), new OpenCvSharp.Mat(), 1, histsize, range);//计算灰度图，dim为1 1维
    }

    [Benchmark]
    public void OpenCVNet_CalcHist()
    {
        
    }

    [Benchmark]
    public void Lad_CalcHist1()
    {
        GrayHistogram.Compute(new OpenCvSharp.Mat(GetImagePath(), OpenCvSharp.ImreadModes.Grayscale));
    }

    [Benchmark]
    public void Lad_CalcHist2()
    {
        using var stream = File.OpenRead(GetImagePath());
        var bitmap = new Bitmap(stream);
        GrayHistogram.ComputeBase(bitmap);
    }
}