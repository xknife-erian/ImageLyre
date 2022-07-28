// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using BenchmarkSample;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<TwoDArrayImpl>();