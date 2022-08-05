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
    private int _side = 16;
    private Mat? _bitmap;
    public int ImageWidth { get; } = 256;
    public int ImageHeight { get; } = 128;

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
        var array = BuildArray(_side, ImageWidth, ImageHeight, true);
        Bitmap = new Mat(new[] { ImageWidth, ImageHeight }, MatType.CV_8SC1, array);
    });

    public ICommand BuildMockVideoCommand => new RelayCommand(() =>
    {
        var a1 = BuildArray(_side, ImageWidth, ImageHeight, false);
        var a2 = BuildArray(_side, ImageWidth, ImageHeight, true);
       
        var flag = true;
        Task.Factory.StartNew(() =>
        {
            while (true)
            {
                Bitmap = flag 
                    ? new Mat(ImageWidth, ImageHeight, MatType.CV_8UC1, a1) 
                    : new Mat(ImageWidth, ImageHeight, MatType.CV_8UC1, a2);
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