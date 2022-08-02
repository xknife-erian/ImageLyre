﻿using System;
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
        Bitmap = new Mat(128, 256, MatType.CV_8SC3, Scalar.Red);
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

        Bitmap = new Mat(new[] {height, width}, MatType.CV_8SC1, array);
    });

    public ICommand BuildMockVideoCommand => new RelayCommand(() =>
    {
        var width = 128;
        var height = 256;
        var side = 16;

        var a1 = BuildArray(side, width, height, false);
        var a2 = BuildArray(side, width, height, true);

        Bitmap = new Mat(new[] {height, width}, MatType.CV_8SC1, a1);

        var flag = true;
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                for (int i = 0; i < width * height; i++)
                {
                    Bitmap.Set(i, flag ? a1[i] : a2[i]);
                }

                Thread.Sleep(200);
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