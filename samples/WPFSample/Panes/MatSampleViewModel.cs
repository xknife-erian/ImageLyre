using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;

namespace WPFSample.Panes;

public class MatSampleViewModel : ObservableRecipient
{
    private Mat? _bitmap;
    public int ImageWidth { get; } = 400;
    public int ImageHeight { get; } = 400;

    public Mat? Bitmap
    {
        get => _bitmap;
        set => SetProperty(ref _bitmap, value);
    }

    /// <summary>
    /// 创建一个二维矩阵，并填充指定的颜色
    /// </summary>
    public ICommand BuildRandomImageCommand => new RelayCommand(() =>
    {
        Bitmap = new Mat(128, 256, MatType.CV_8UC3, Scalar.Red);
    });

    /// <summary>
    /// 创建一个二维矩阵，并填充为马赛克
    /// </summary>
    public ICommand BuildMosaicCommand => new RelayCommand(() =>
    {
        var width = 128;
        var height = 256;
        var side = 16;

        var array = BuildArray(side, width, height, true);

        Bitmap = new Mat(new[] {height, width}, MatType.CV_8UC1, array);
        //Bitmap = new Mat(new[] {height, width}, MatType.CV_8SC3, Scalar.Blue);
    });

    public unsafe ICommand BuildMockVideoCommand => new RelayCommand(() =>
    {
        var width = 256;
        var height = 128;
        var side = 16;

        var a1 = BuildArray(side, width, height, false);
        var a2 = BuildArray(side, width, height, true);

        Bitmap = new Mat(width, height, MatType.CV_8UC1, new Scalar(0x00));


        // for (int i = 0; i < mat.Rows; i++)
        // {
        //     IntPtr a = mat.Ptr(i);
        //     byte* b = (byte*)a.ToPointer();
        //     for (int j = 0; j < mat.Cols; j++)
        //     {
        //         b[j] = 0x33;
        //     }
        // }
        //
        // for (int i = 0; i < mat.Rows; i++)
        // {
        //     for (int j = 0; j < mat.Cols; j++)
        //     {
        //         //Bitmap.Set(i, flag ? a1[i] : a2[i]);
        //         // Bitmap.At<byte>(i, j) = 0xFF;
        //         // Bitmap.SetTo(Scalar.IndianRed);
        //         //mat.Set(i, j, 0xdd);
        //     }
        // }


        
        var flag = true;
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                var mat = new Mat(width, height, MatType.CV_8UC1, new Scalar(0xFF));

                // for (int i = 0; i < mat.Rows; i++)
                // {
                //     for (int j = 0; j < mat.Cols; j++)
                //     {
                //         mat.At<byte>(i, j) = flag ? a1[i * side + j] : a2[i * side + j];
                //     }
                // }

                for (int i = 0; i < mat.Rows; i++)
                {
                    IntPtr a = mat.Ptr(i);
                    byte* b = (byte*)a.ToPointer();
                    for (int j = 0; j < mat.Cols; j++)
                    {
                        b[j] = flag ? a1[i * side + j] : a2[i * side + j];
                    }
                }

                Bitmap = mat;

                Thread.Sleep(300);
                flag = !flag;
            }
        });
    });

    private static byte[] BuildArray(int side, int width, int height, bool flag)
    {
        var array = new byte[width * height];
        var j = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (j >= side)
            {
                j = 0;
                flag = !flag;
                if (i % width == 0 && i / width % side == 0)
                    flag = !flag;
            }

            array[i] = flag ? (byte) 0xEE : (byte) 0x33;
            j++;
        }

        return array;
    }

}