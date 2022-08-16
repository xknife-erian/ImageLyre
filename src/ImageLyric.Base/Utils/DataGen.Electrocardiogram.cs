namespace ImageLyric.Utils;

public static partial class DataGen
{
    public class Electrocardiogram
    {
        private double _HeartRate;
        private double _Period;

        public Electrocardiogram(double heartRate = 72.0)
        {
            HeartRate = heartRate;
        }

        public double PWaveAmplitude { get; set; } = 0.25;

        public double PWaveDuration { get; set; } = 0.09;

        public double PWavePRInterval { get; set; } = 0.16;

        public double QWaveAmplitude { get; set; } = 0.025;

        public double QwaveDuration { get; set; } = 0.066;

        public double QWaveTime { get; set; } = 0.166;

        public double QRSWaveAmplitude { get; set; } = 1.6;

        public double QRSwaveDuration { get; set; } = 0.11;

        public double SWaveAmplitude { get; set; } = 0.25;

        public double SWaveDuration { get; set; } = 0.066;

        public double SWaveTime { get; set; } = 0.09;

        public double TWaveAmplitude { get; set; } = 0.35;

        public double TWaveDuration { get; set; } = 0.142;

        public double TWaveSTInterval { get; set; } = 0.2;

        public double UWaveAmplitude { get; set; } = 0.035;

        public double UWaveDuration { get; set; } = 0.0476;

        public double UWaveTime { get; set; } = 0.433;

        public double HeartRate
        {
            get => _HeartRate;
            set
            {
                var num1 = value;
                var num2 = 60.0 / value;
                _HeartRate = num1;
                _Period = num2;
            }
        }

        public double Period
        {
            get => _Period;
            set
            {
                var num1 = value;
                var num2 = 60.0 / value;
                _Period = num1;
                _HeartRate = num2;
            }
        }

        public double GetVoltage(double elapsedSeconds)
        {
            elapsedSeconds %= 2.0 * Period;
            return PWave(elapsedSeconds, PWaveAmplitude, PWaveDuration, PWavePRInterval, Period) - 0.9 +
                   QWave(elapsedSeconds, QWaveAmplitude, QwaveDuration, QWaveTime, Period) +
                   QRSwave(elapsedSeconds, QRSWaveAmplitude, QRSwaveDuration, Period) +
                   SWave(elapsedSeconds, SWaveAmplitude, SWaveDuration, SWaveTime, Period) +
                   TWave(elapsedSeconds, TWaveAmplitude, TWaveDuration, TWaveSTInterval, Period) + UWave(elapsedSeconds,
                       UWaveAmplitude, UWaveDuration, UWaveTime, Period);
        }

        private static double QRSwave(double x, double amplitude, double duration, double period)
        {
            var num1 = 0.5 * period;
            var num2 = amplitude;
            var num3 = 2.0 * num1 / duration;
            var num4 = num2 / (2.0 * num3) * (2.0 - num3);
            var num5 = 0.0;
            var num6 = 1;
            for (var index = 100; num6 <= index; ++num6)
            {
                var num7 = 2.0 * num3 * num2 / (num6 * num6 * Math.PI * Math.PI) *
                           (1.0 - Math.Cos(num6 * Math.PI / num3)) * Math.Cos(num6 * Math.PI * x / num1);
                num5 += num7;
            }

            return num4 + num5;
        }

        private static double PWave(
            double x,
            double amplitude,
            double duration,
            double time,
            double period)
        {
            var num1 = 0.5 * period;
            var num2 = amplitude;
            var num3 = 2.0 * num1 / duration;
            x += time;
            var num4 = 1.0 / num1;
            var num5 = 0.0;
            var num6 = 1;
            for (var index = 100; num6 <= index; ++num6)
            {
                var num7 = (Math.Sin(Math.PI / (2.0 * num3) * (num3 - 2 * num6)) / (num3 - 2 * num6) +
                            Math.Sin(Math.PI / (2.0 * num3) * (num3 + 2 * num6)) / (num3 + 2 * num6)) *
                           (2.0 / Math.PI) * Math.Cos(num6 * Math.PI * x / num1);
                num5 += num7;
            }

            return num2 * (num4 + num5);
        }

        private static double QWave(
            double x,
            double amplitude,
            double duration,
            double time,
            double period)
        {
            var num1 = 0.5 * period;
            var num2 = amplitude;
            var num3 = 2.0 * num1 / duration;
            x += time;
            var num4 = num2 / (2.0 * num3) * (2.0 - num3);
            var num5 = 0.0;
            var num6 = 1;
            for (var index = 100; num6 <= index; ++num6)
            {
                var num7 = 2.0 * num3 * num2 / (num6 * num6 * Math.PI * Math.PI) *
                           (1.0 - Math.Cos(num6 * Math.PI / num3)) * Math.Cos(num6 * Math.PI * x / num1);
                num5 += num7;
            }

            return -1.0 * (num4 + num5);
        }

        private static double SWave(
            double x,
            double amplitude,
            double duration,
            double time,
            double period)
        {
            var num1 = 0.5 * period;
            var num2 = amplitude;
            var num3 = 2.0 * num1 / duration;
            x -= time;
            var num4 = num2 / (2.0 * num3) * (2.0 - num3);
            var num5 = 0.0;
            var num6 = 1;
            for (var index = 100; num6 <= index; ++num6)
            {
                var num7 = 2.0 * num3 * num2 / (num6 * num6 * Math.PI * Math.PI) *
                           (1.0 - Math.Cos(num6 * Math.PI / num3)) * Math.Cos(num6 * Math.PI * x / num1);
                num5 += num7;
            }

            return -1.0 * (num4 + num5);
        }

        private static double TWave(
            double x,
            double amplitude,
            double duration,
            double time,
            double period)
        {
            var num1 = 0.5 * period;
            var num2 = amplitude;
            var num3 = 2.0 * num1 / duration;
            x = x - time - 0.045;
            var num4 = 1.0 / num1;
            var num5 = 0.0;
            var num6 = 1;
            for (var index = 100; num6 <= index; ++num6)
            {
                var num7 = (Math.Sin(Math.PI / (2.0 * num3) * (num3 - 2 * num6)) / (num3 - 2 * num6) +
                            Math.Sin(Math.PI / (2.0 * num3) * (num3 + 2 * num6)) / (num3 + 2 * num6)) *
                           (2.0 / Math.PI) * Math.Cos(num6 * Math.PI * x / num1);
                num5 += num7;
            }

            return num2 * (num4 + num5);
        }

        private static double UWave(
            double x,
            double amplitude,
            double duration,
            double time,
            double period)
        {
            var num1 = 0.5 * period;
            var num2 = amplitude;
            var num3 = 2.0 * num1 / duration;
            x -= time;
            var num4 = 1.0 / num1;
            var num5 = 0.0;
            var num6 = 1;
            for (var index = 100; num6 <= index; ++num6)
            {
                var num7 = (Math.Sin(Math.PI / (2.0 * num3) * (num3 - 2 * num6)) / (num3 - 2 * num6) +
                            Math.Sin(Math.PI / (2.0 * num3) * (num3 + 2 * num6)) / (num3 + 2 * num6)) *
                           (2.0 / Math.PI) * Math.Cos(num6 * Math.PI * x / num1);
                num5 += num7;
            }

            return num2 * (num4 + num5);
        }
    }
}