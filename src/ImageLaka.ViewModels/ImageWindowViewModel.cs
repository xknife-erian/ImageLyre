using System.Drawing;
using System.Drawing.Imaging;
using ImageLaka.ImageEngine;
using ImageLaka.Services.Macros;
using ImageLaka.Services.Macros.Commands;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using MvvmDialogs;

namespace ImageLaka.ViewModels;

public class ImageWindowViewModel : ObservableRecipient
{
    private ImageTarget _imageTarget;
    private Macro _macro;
    private Bitmap _bitmap;
    private int _width;
    private int _height;

    public Bitmap Bitmap
    {
        get => _bitmap;
        set => SetProperty(ref _bitmap, value);
    }

    public int Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    public int Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    public void Read(string path)
    {
        _macro = new Macro();
        _imageTarget = new ImageTarget(path);
        var command = new OpenCommand(_imageTarget);
        _macro.AddAndDoCurrent(command);
        Bitmap = _imageTarget.CurrentData;
        Width = Bitmap.Width;
        Height = Bitmap.Height;
    }
}