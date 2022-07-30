using BenchmarkDotNet.Running;

namespace ImageProcessBenchmark;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        BenchmarkRunner.Run<CalcGrayHistogram>();
    }
}