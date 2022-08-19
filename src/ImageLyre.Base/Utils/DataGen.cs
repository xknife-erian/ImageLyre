using System.Drawing;

namespace ImageLyre.Utils;

public static partial class DataGen
{
    /// <summary>Generates an array of numbers with constant spacing.</summary>
    /// <param name="pointCount">The number of points</param>
    /// <param name="spacing">The space between points. Default 1.</param>
    /// <param name="offset">The first point. Default 0</param>
    /// <returns>An array of numbers with constant spacing.</returns>
    public static double[] Consecutive(int pointCount, double spacing = 1.0, double offset = 0.0)
    {
        var numArray = new double[pointCount];
        for (var index = 0; index < numArray.Length; ++index)
            numArray[index] = index * spacing + offset;
        return numArray;
    }

    /// <summary>Generates an array of sine values of an input array.</summary>
    /// <param name="xs">The arguments to the sine function.</param>
    /// <param name="mult">A number to multiply the output by. Default 1.</param>
    /// <returns>An array of sine values</returns>
    public static double[] Sin(double[] xs, double mult = 1.0)
    {
        var numArray = new double[xs.Length];
        for (var index = 0; index < xs.Length; ++index)
            numArray[index] = Math.Sin(xs[index]) * mult;
        return numArray;
    }

    /// <summary>Generates an array of sine values.</summary>
    /// <param name="pointCount">The number of values to generate.</param>
    /// <param name="oscillations">The number of periods. Default 1.</param>
    /// <param name="offset">The number to increment the output by. Default 0.</param>
    /// <param name="mult">The number to multiply the output by. Default 1.</param>
    /// <param name="phase">The fraction of a period to offset by. Default 0.</param>
    /// <returns>An array of sine values</returns>
    public static double[] Sin(
        int pointCount,
        double oscillations = 1.0,
        double offset = 0.0,
        double mult = 1.0,
        double phase = 0.0)
    {
        var num = 2.0 * Math.PI * oscillations / (pointCount - 1);
        var numArray = new double[pointCount];
        for (var index = 0; index < numArray.Length; ++index)
            numArray[index] = Math.Sin(index * num + phase * Math.PI * 2.0) * mult + offset;
        return numArray;
    }

    /// <summary>
    ///     Return data for a sine wave that increases frequency toward the end of an array.
    ///     This function may be useful for inspecting rendering artifacts when data is displayed at different densities.
    /// </summary>
    /// <param name="pointCount">The number of values to generate.</param>
    /// <param name="density">Increasing this value increases maximum frequency.</param>
    /// <returns>An array of values</returns>
    public static double[] SinSweep(int pointCount, double density = 50.0)
    {
        var numArray = new double[pointCount];
        for (var index = 0; index < numArray.Length; ++index)
        {
            var a = Math.Pow(index / (double) pointCount * density, 2.0);
            numArray[index] = Math.Sin(a);
        }

        return numArray;
    }

    /// <summary>
    ///     Generates an array of cosine values of an input array.
    /// </summary>
    /// <param name="xs">The arguments to the cosine function.</param>
    /// <param name="mult">A number to multiply the output by. Default 1.</param>
    /// <returns>An array of cosine values</returns>
    public static double[] Cos(double[] xs, double mult = 1.0)
    {
        var numArray = new double[xs.Length];
        for (var index = 0; index < xs.Length; ++index)
            numArray[index] = Math.Cos(xs[index]) * mult;
        return numArray;
    }

    /// <summary>Generates an array of cosine values.</summary>
    /// <param name="pointCount">The number of values to generate.</param>
    /// <param name="oscillations">The number of periods. Default 1.</param>
    /// <param name="offset">The number to increment the output by. Default 0.</param>
    /// <param name="mult">The number to multiply the output by. Default 1.</param>
    /// <param name="phase">The fraction of a period to offset by. Default 0.</param>
    /// <returns>An array of cosine values</returns>
    public static double[] Cos(
        int pointCount,
        double oscillations = 1.0,
        double offset = 0.0,
        double mult = 1.0,
        double phase = 0.0)
    {
        var num = 2.0 * Math.PI * oscillations / (pointCount - 1);
        var numArray = new double[pointCount];
        for (var index = 0; index < numArray.Length; ++index)
            numArray[index] = Math.Cos(index * num + phase * Math.PI * 2.0) * mult + offset;
        return numArray;
    }

    /// <summary>
    ///     Generates an array of tangent values of an input array.
    /// </summary>
    /// <param name="xs">The arguments to the tangent function.</param>
    /// <param name="mult">A number to multiply the output by. Default 1.</param>
    /// <returns>An array of tangent values</returns>
    public static double[] Tan(double[] xs, double mult = 1.0)
    {
        var numArray = new double[xs.Length];
        for (var index = 0; index < xs.Length; ++index)
            numArray[index] = Math.Tan(xs[index]) * mult;
        return numArray;
    }

    /// <summary>
    ///     Generates an array of random numbers following a uniform distribution on the interval [offset, multiplier].
    /// </summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="pointCount">The number of random points to generate.</param>
    /// <param name="multiplier">The maximum number above offset that may be generated.</param>
    /// <param name="offset">The minimum number that may be generated.</param>
    /// <returns>An array of random numbers.</returns>
    public static double[] Random(Random rand, int pointCount, double multiplier = 1.0, double offset = 0.0)
    {
        if (rand == null)
            rand = new Random();
        var numArray = new double[pointCount];
        for (var index = 0; index < pointCount; ++index)
            numArray[index] = rand.NextDouble() * multiplier + offset;
        return numArray;
    }

    /// <summary>
    ///     Generates a 2D array of random numbers between 0 and 1 (uniform distribution)
    /// </summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="rows">number of rows (dimension 0)</param>
    /// <param name="columns">number of columns (dimension 1)</param>
    /// <param name="multiplier">Multiply values by this number after generation</param>
    /// <param name="offset">Add to values after multiplication</param>
    /// <returns>2D array filled with random numbers</returns>
    public static double[,] Random2D(
        Random rand,
        int rows,
        int columns,
        double multiplier = 1.0,
        double offset = 0.0)
    {
        if (rand == null)
            throw new ArgumentNullException();
        var numArray = new double[rows, columns];
        for (var index1 = 0; index1 < numArray.GetLength(0); ++index1)
        for (var index2 = 0; index2 < numArray.GetLength(1); ++index2)
            numArray[index1, index2] = rand.NextDouble() * multiplier + offset;
        return numArray;
    }

    /// <summary>
    ///     Generates an array of random numbers following a uniform distribution on the interval [offset, multiplier].
    /// </summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="pointCount">The number of random points to generate.</param>
    /// <param name="multiplier">The maximum number above offset that may be generated.</param>
    /// <param name="offset">The minimum number that may be generated.</param>
    /// <returns>An array of random numbers.</returns>
    public static int[] RandomInts(Random rand, int pointCount, double multiplier = 1.0, double offset = 0.0)
    {
        if (rand == null)
            rand = new Random();
        var numArray = new int[pointCount];
        for (var index = 0; index < pointCount; ++index)
            numArray[index] = (int) (rand.NextDouble() * multiplier + offset);
        return numArray;
    }

    /// <summary>Generates a single value from a normal distribution.</summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="stdDev">The standard deviation of the distribution.</param>
    /// <param name="maxSdMultiple">
    ///     The maximum distance from the mean to generate, given as a multiple of the standard
    ///     deviation.
    /// </param>
    /// <returns>A single value from a normal distribution.</returns>
    public static double RandomNormalValue(
        Random rand,
        double mean,
        double stdDev,
        double maxSdMultiple = 10.0)
    {
        double num1;
        do
        {
            var d = 1.0 - rand.NextDouble();
            var num2 = 1.0 - rand.NextDouble();
            num1 = Math.Sqrt(-2.0 * Math.Log(d)) * Math.Sin(2.0 * Math.PI * num2);
        } while (Math.Abs(num1) >= maxSdMultiple);

        return mean + stdDev * num1;
    }

    /// <summary>
    ///     Generates an array of values from a normal distribution.
    /// </summary>
    /// <param name="seed">The number to seed the random number generator with.</param>
    /// <param name="pointCount">The number of points to generate.</param>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="stdDev">The standard deviation of the distribution.</param>
    /// <param name="maxSdMultiple">
    ///     The maximum distance from the mean to generate, given as a multiple of the standard
    ///     deviation.
    /// </param>
    /// <returns>An array of values from a normal distribution.</returns>
    public static double[] RandomNormal(
        int seed,
        int pointCount,
        double mean = 0.5,
        double stdDev = 0.5,
        double maxSdMultiple = 10.0)
    {
        return RandomNormal(new Random(seed), pointCount, mean, stdDev, maxSdMultiple);
    }

    /// <summary>
    ///     Generates an array of values from a normal distribution.
    /// </summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="pointCount">The number of points to generate.</param>
    /// <param name="mean">The mean of the distribution.</param>
    /// <param name="stdDev">The standard deviation of the distribution.</param>
    /// <param name="maxSdMultiple">
    ///     The maximum distance from the mean to generate, given as a multiple of the standard
    ///     deviation.
    /// </param>
    /// <returns>An array of values from a normal distribution.</returns>
    public static double[] RandomNormal(
        Random rand,
        int pointCount,
        double mean = 0.5,
        double stdDev = 0.5,
        double maxSdMultiple = 10.0)
    {
        if (rand == null)
            rand = new Random(0);
        var numArray = new double[pointCount];
        for (var index = 0; index < numArray.Length; ++index)
            numArray[index] = RandomNormalValue(rand, mean, stdDev, maxSdMultiple);
        return numArray;
    }

    /// <summary>
    ///     Generates an array of data with normally distributed residuals about a line.
    /// </summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="pointCount">The number of points to generate. Default 100.</param>
    /// <param name="slope">The slope of the line. Default 1.</param>
    /// <param name="offset">The y-intercept of the line. Default 0.</param>
    /// <param name="noise">The standard deviation of the residuals. Default 0.1</param>
    /// <returns>An array of approximately linear data.</returns>
    public static double[] NoisyLinear(
        Random rand,
        int pointCount = 100,
        double slope = 1.0,
        double offset = 0.0,
        double noise = 0.1)
    {
        if (rand == null)
            rand = new Random(0);
        var numArray = new double[pointCount];
        for (var index = 0; index < numArray.Length; ++index)
            numArray[index] = slope * index + offset + RandomNormalValue(rand, 0.0, noise);
        return numArray;
    }

    /// <summary>
    ///     Generates an array of data with uniformally distributed residuals about a sinusoidal curve.
    /// </summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="pointCount">The number of points to generate.</param>
    /// <param name="oscillations">The number of periods. Default 1.</param>
    /// <param name="noiseLevel">Twice the maximum residual, in units of mult. Default 0.5</param>
    /// <param name="mult">The number to multiply the residuals by. Default 1.</param>
    /// <returns>An array of approximately sinusoidal data.</returns>
    public static double[] NoisySin(
        Random rand,
        int pointCount,
        double oscillations = 1.0,
        double noiseLevel = 0.5,
        double mult = 1.0)
    {
        if (rand == null)
            rand = new Random(0);
        var numArray = Sin(pointCount, oscillations);
        for (var index = 0; index < numArray.Length; ++index)
            numArray[index] += (rand.NextDouble() - 0.5) * noiseLevel * mult;
        return numArray;
    }

    /// <summary>Generates a random color.</summary>
    /// <param name="rand">The Random object to use.</param>
    /// <param name="min">The min of each component. Default 0.</param>
    /// <param name="max">The max of each component. Default 255.</param>
    /// <returns>A random color.</returns>
    public static Color RandomColor(Random rand, int min = 0, int max = 255)
    {
        if (rand == null)
            rand = new Random();
        var red = rand.Next(min, max);
        var num1 = rand.Next(min, max);
        var num2 = rand.Next(min, max);
        var green = num1;
        var blue = num2;
        return Color.FromArgb(red, green, blue);
    }

    /// <summary>
    ///     Return the cumulative sum of a random set of numbers using a fixed seed
    /// </summary>
    /// <param name="pointCount">The number of points to generate.</param>
    /// <param name="mult">The max difference between points in the walk. Default 1.</param>
    /// <param name="offset">The first point in the walk. Default 0.</param>
    /// <returns>The cumulative sum of a random set of numbers.</returns>
    public static double[] RandomWalk(int pointCount, double mult = 1.0, double offset = 0.0)
    {
        return RandomWalk(new Random(0), pointCount, mult, offset);
    }

    /// <summary>Return the cumulative sum of a random set of numbers.</summary>
    /// <param name="rand">The random object to use.</param>
    /// <param name="pointCount">The number of points to generate.</param>
    /// <param name="mult">The max difference between points in the walk. Default 1.</param>
    /// <param name="offset">The first point in the walk. Default 0.</param>
    /// <returns>The cumulative sum of a random set of numbers.</returns>
    public static double[] RandomWalk(Random rand, int pointCount, double mult = 1.0, double offset = 0.0)
    {
        if (rand == null)
            rand = new Random(0);
        var numArray = new double[pointCount];
        numArray[0] = offset;
        for (var index = 1; index < numArray.Length; ++index)
            numArray[index] = numArray[index - 1] + (rand.NextDouble() * 2.0 - 1.0) * mult;
        return numArray;
    }

    /// <summary>
    ///     Generate unevenly-spaced X/Y points.
    ///     X values walk upward (by values from 0 to 1)
    ///     Y values walk randomly (by values from -1 to 1)
    /// </summary>
    public static (double[] xs, double[] ys) RandomWalk2D(Random rand, int pointCount)
    {
        var numArray1 = new double[pointCount];
        var numArray2 = new double[pointCount];
        for (var index = 1; index < numArray1.Length; ++index)
        {
            numArray1[index] = numArray1[index - 1] + (rand.NextDouble() - 0.5) * 2.0;
            numArray2[index] = numArray2[index - 1] + rand.NextDouble();
        }

        return (numArray2, numArray1);
    }

    /// <summary>Generates a random span.</summary>
    /// <param name="rand">The random object to use.</param>
    /// <param name="low">The minimum of the span. Default 0.</param>
    /// <param name="high">Tge naximum of the span. Default 100.</param>
    /// <param name="minimumSpacing">The minimum length of the span. Default 10.</param>
    /// <returns>A random span.</returns>
    public static (double, double) RandomSpan(
        Random rand = null,
        double low = 0.0,
        double high = 100.0,
        double minimumSpacing = 10.0)
    {
        if (rand == null)
            rand = new Random();
        var num1 = Math.Abs(high - low);
        for (var index = 0; index < 10000; ++index)
        {
            var num2 = rand.NextDouble() * num1 + low;
            var num3 = rand.NextDouble() * num1 + low;
            if (Math.Abs(num2 - num3) >= minimumSpacing)
                return num2 < num3 ? (num2, num3) : (num3, num2);
        }

        throw new ArgumentException();
    }

    /// <summary>
    ///     Generates a range of values starting at 0 and separated by 1.
    /// </summary>
    /// <param name="stop">The end of the range.</param>
    /// <returns>A range of values.</returns>
    public static double[] Range(int stop)
    {
        return Range(0.0, stop, 1.0);
    }

    /// <summary>Generates a range of values separated by 1.</summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <returns>A range of values.</returns>
    public static double[] Range(int start, int stop)
    {
        return Range(start, stop, 1.0);
    }

    /// <summary>Generates a range of values.</summary>
    /// <param name="start">The start of the range.</param>
    /// <param name="stop">The end of the range.</param>
    /// <param name="step">The space between values.</param>
    /// <param name="includeStop">Indicates whether to include the stop point in the range. Default false.</param>
    /// <returns>A range of values.</returns>
    public static double[] Range(double start, double stop, double step, bool includeStop = false)
    {
        if (step <= 0.0)
            throw new ArgumentException("step must be >0. To make a descending series make stop < start.");
        var length = (int) (Math.Abs(start - stop) / step);
        var num = stop > start ? step : -step;
        if (includeStop)
            ++length;
        var numArray = new double[length];
        for (var index = 0; index < length; ++index)
            numArray[index] = start + index * num;
        return numArray;
    }

    /// <summary>Generates an array of zeros</summary>
    /// <param name="pointCount">The number of zeroes to generate</param>
    /// <returns>An array of zeros</returns>
    public static double[] Zeros(int pointCount)
    {
        return new double[pointCount];
    }

    /// <summary>Generates an array of ones</summary>
    /// <param name="pointCount">The number of ones to generate</param>
    /// <returns>An array of ones</returns>
    public static double[] Ones(int pointCount)
    {
        var numArray = new double[pointCount];
        for (var index = 0; index < pointCount; ++index)
            numArray[index] = 1.0;
        return numArray;
    }

    /// <summary>
    ///     Recording of a neuronal action potential (100 ms, 20 kHz sample rate, mV units)
    /// </summary>
    /// <returns>Recording of a neuronal action potential</returns>
    private static double[] HanningWindow(int size, bool normalize = false)
    {
        var source = new double[size];
        for (var index = 0; index < size; ++index)
            source[index] = 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * index / size);
        if (normalize)
        {
            var num = source.Sum();
            for (var index = 0; index < source.Length; ++index)
                source[index] /= num;
        }

        return source;
    }

    public static double[] NoisyBellCurve(
        Random rand,
        int count,
        double mult = 1.0,
        double noiseFraction = 0.1)
    {
        var numArray = HanningWindow(count);
        for (var index = 0; index < count; ++index)
        {
            numArray[index] += (rand.NextDouble() - 0.5) * noiseFraction;
            numArray[index] *= mult;
        }

        return numArray;
    }

}