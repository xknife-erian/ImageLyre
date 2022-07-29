using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Emgu.CV;
using Emgu.CV.Util;
using ImageLad.ImageEngine.Analyze;

namespace CalcHistSample
{
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
        [Benchmark]
        public void EmguCV_CalcHist()
        {
            var imgGray = new Mat();
            var hist = new Mat();
            int[] channels = new int[] { 0 };  //初始化数组
            float[] ranges = new float[] { 0, 255 };
            int[] histSize = new int[] { 256 };
            VectorOfMat vMatImgs = new VectorOfMat();
            vMatImgs.Push(imgGray);
            CvInvoke.CalcHist(vMatImgs, channels, new Mat(), hist, histSize, ranges, false);
        }

        [Benchmark]
        public void Lad_CalcHist1()
        {
            GrayHistogram.Compute(null, GrayFormula.Average);
        }

        [Benchmark]
        public void Lad_CalcHist2()
        {
            GrayHistogram.Compute(null, GrayFormula.Weighted);
        }
    }

    class Program1
    {
        private static List<int> employees;
        static void aMain(string[] args)
        {
            employees = new List<int>();
            employees.Add(1);
            employees.Add(2);
            employees.Add(3);
            employees.Add(4);
            employees.Add(5);
            employees.Add(6);
            var histogram = employees.GroupBy(employee => employee.Age)
                .Select(g => new { Key = g.Key.ToString(), Tally = g.Count() })
                .OrderByDescending(chartStat => chartStat.Tally).ToList();
            foreach (var item in histogram)
            {
                Console.WriteLine("Age: {0}, Tally: {1}", item.Key, item.Tally);
            }
            Console.ReadLine();
        }
    }
}