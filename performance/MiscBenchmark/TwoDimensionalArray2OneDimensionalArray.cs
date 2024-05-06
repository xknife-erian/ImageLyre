using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace MiscBenchmark;

[MemoryDiagnoser]
public class TwoDimensionalArray2OneDimensionalArray
{
    private double[,] _input;
    public TwoDimensionalArray2OneDimensionalArray()
    {
        _input = new double[2048, 2048];
        for (int i = 0; i < 2048; i++)
        {
            for (int j = 0; j < 2048; j++)
            {
                _input[i, j] = i * j;
            }
        }
    }

    [Benchmark]
    public double[] BufferBlockCopy()
    {
        var input = _input;
        var width = input.GetLength(0);
        var height = input.GetLength(1);
        var size = width * height;
        var result = new double[size];
        Buffer.BlockCopy(input, 0, result, 0, size * sizeof(double));
        return result;
    }

    [Benchmark]
    public unsafe double[] MarshalCopy()
    {
        var input = _input;
        var width = input.GetLength(0);
        var height = input.GetLength(1);
        var size = width * height;
        var result = new double[size];
        fixed (double* pInput = input)
        {
            Marshal.Copy((IntPtr) pInput, result, 0, size);
        }

        return result;
    }

    [Benchmark]
    public double[] ElemCopy()
    {
        var input = _input;
        var width = input.GetLength(0);
        var height = input.GetLength(1);
        var size = width * height;

        var result = new double[size];
        for (var i = 0; i < width; i++)
        for (var j = 0; j < height; j++)
            result[i * height + j] = input[i, j];
        return result;
    }

    [Benchmark]
    public unsafe double[] UnsafeElemCopy()
    {
        var input = _input;
        var width = input.GetLength(0);
        var height = input.GetLength(1);
        var size = width * height;

        var result = new double[size];
        fixed (double* pInput = input, pResult = result)
        {
            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                *(pResult + i * height + j) = *(pInput + i * height + j);
        }

        return result;
    }

    [Benchmark]
    public unsafe double[] MemcpyImpl()
    {
        var input = _input;
        var width = input.GetLength(0);
        var height = input.GetLength(1);
        var size = width * height;

        var result = new double[size];
        fixed (double* pInput = input, pResult = result)
        {
            memcpy((IntPtr) pResult, (IntPtr) pInput, (UIntPtr) (size * sizeof(double)));
        }

        return result;
    }

    [Benchmark]
    public double[] LinqImpl()
    {
        var input = _input;
        return input.OfType<double>().ToArray();
    }

    [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
    public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UIntPtr count);
}