using System.Drawing;
using BenchmarkDotNet.Attributes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageLad.ImageEngine.Analyze;

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
        //  1.加载原图
        var image = new Image<Bgr, byte>(GetImagePath());

        // 2. 原图转灰度
        var imgGray = new Mat();
        CvInvoke.CvtColor(image, imgGray, ColorConversion.Bgr2Gray);

        // 3. 计算直方图
        var hist = new Mat();
        int[] channels = { 0 };  //初始化数组
        float[] ranges = { 0, 255 };
        int[] histSize = { 256 };
        VectorOfMat vMat = new VectorOfMat();
        vMat.Push(imgGray);
        CvInvoke.CalcHist(vMat, channels, new Mat(), hist, histSize, ranges, false);
    }

    [Benchmark]
    public void Lad_CalcHist1()
    {
        using var stream = File.OpenRead(GetImagePath());
        var bitmap = new Bitmap(stream);
        GrayHistogram.Compute(bitmap, GrayFormula.Average);
    }

    [Benchmark]
    public void Lad_CalcHist2()
    {
        using var stream = File.OpenRead(GetImagePath());
        var bitmap = new Bitmap(stream);
        GrayHistogram.Compute(bitmap, GrayFormula.Weighted);
    }
}