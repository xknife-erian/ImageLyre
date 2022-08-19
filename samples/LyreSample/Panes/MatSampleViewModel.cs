using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageLyre.UI.Views.Utils;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;

namespace LyreSample.Panes;

public class MatSampleViewModel : ObservableRecipient
{
    private int _side = 16;
    private Mat? _imageMat;
    public int ImageColumns { get; } = 128;
    public int ImageRows { get; } = 256;

    public Mat? ImageMat
    {
        get => _imageMat;
        set => SetProperty(ref _imageMat, value);
    }

    /// <summary>
    /// 创建一个二维矩阵，并填充指定的颜色
    /// </summary>
    public ICommand BuildRandomImageCommand => new RelayCommand(() =>
    {
        ImageMat = new Mat(ImageRows, ImageColumns, MatType.CV_8UC3, Scalar.Red);
    });

    public ICommand UpdateMatCommand => new RelayCommand(() =>
    {
        if (ImageMat == null)
            return;
        Mat mat = Mat.Zeros(ImageMat.Rows, ImageMat.Cols, MatType.CV_8UC3);
        if (ImageMat.Channels() == 3)
        {
            for (int i = 0; i < ImageMat.Height; i++)
            {
                for (int j = 0; j < ImageMat.Width; j++)
                {
                    mat.Set(i, j, new Vec3b(0, 255, 0));
                }
            }
        }
        else
        {
            for (int i = 0; i < ImageMat.Height; i++)
            {
                for (int j = 0; j < ImageMat.Width; j++)
                {
                    ImageMat.Set(i, j, Scalar.Yellow);
                }
            }
        }

        ImageMat = mat;
    });

    /// <summary>
    /// 创建一个二维矩阵，并填充为马赛克
    /// </summary>
    public ICommand BuildMosaicCommand => new RelayCommand(() =>
    {
        var array = BuildArray(_side, ImageColumns, ImageRows, true);
        ImageMat = new Mat(new[] { ImageRows, ImageColumns }, MatType.CV_8SC1, array);
    });

    private bool _mockFlag = false;

    public ICommand BuildMockVideoCommand => new RelayCommand(() =>
    {
        _mockFlag = !_mockFlag;
        if (!_mockFlag)
            return;

        var a1 = BuildArray(_side, ImageColumns, ImageRows, false);
        var a2 = BuildArray(_side, ImageColumns, ImageRows, true);

        var flag = true;
        Task.Factory.StartNew(() =>
        {
            var timer = new System.Timers.Timer();
            timer.Interval = 120;
            timer.Elapsed += (_, _) =>
            {
                UI.RunAsync(() =>
                {
                    ImageMat = flag
                        ? new Mat(ImageRows, ImageColumns,  MatType.CV_8UC1, a1)
                        : new Mat(ImageRows,ImageColumns,  MatType.CV_8UC1, a2);
                    flag = !flag;
                });
                if (!_mockFlag)
                    timer.Stop();
            };
            timer.Start();
        });
    });

    private static byte[] BuildArray(int side, int cols, int rows, bool flag)
    {
        var array = new byte[cols * rows];
        var j = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (j >= side)
            {
                j = 0;
                flag = !flag;
                if (i % cols == 0 && i / cols % side == 0)
                    flag = !flag;
            }

            array[i] = flag ? (byte) 0xEE : (byte) 0x33;
            j++;
        }

        return array;
    }

}