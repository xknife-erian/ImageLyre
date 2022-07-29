using System.Drawing;
using System.Windows.Media.Imaging;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageLad.ImageEngine.Analyze;

namespace CalcHistSample;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        BenchmarkRunner.Run<CalcHistImpl>();
    }
}

public class CalcHistImpl
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
        var image1 = new Image<Bgr, byte>(GetImagePath());
        var image0 = image1.Mat.Clone();

        // 2. 原图转灰度
        var imgGray = new Mat();
        CvInvoke.CvtColor(image0, imgGray, ColorConversion.Bgr2Gray);

        // 3. 计算直方图
        var hist = new Mat();
        int[] channels = new int[] { 0 };  //初始化数组
        float[] ranges = new float[] { 0, 255 };
        int[] histSize = new int[] { 256 };
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