﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        var array = new byte[width * height];
        var j = 0;
        var c = true;
        for (int i = 0; i < array.Length; i++)
        {
            if (j >= side)
            {
                j = 0;
                c = !c;
                if (i % width == 0 && i / width % side == 0)
                    c = !c;
            }

            array[i] = c ? (byte) 0xEE : (byte) 0x33;
            j++;
        }

        Bitmap = new Mat(new[] {height, width}, MatType.CV_8SC1, array);
    });
}