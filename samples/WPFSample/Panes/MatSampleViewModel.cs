using System;
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

    public ICommand BuildRandomImageCommand => new RelayCommand(() =>
    {
        Bitmap = new Mat(512,512, MatType.CV_8SC3, Scalar.Red);
    });
}