// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using MiscBenchmark;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<TwoDimensionalArray2OneDimensionalArray>();