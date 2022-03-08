using System.Drawing;
using System.Drawing.Imaging;
using ImageLaka.ImageEngine;
using ImageLaka.Services.Macros;
using ImageLaka.Services.Macros.Commands;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace ImageLaka.ViewModels;

public class ImageWindowViewModel : ObservableRecipient
{
    private ImageTarget _imageTarget;
    private Macro _macro;
    private Bitmap _data;
    private int _width;
    private int _height;

    public Bitmap Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
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
        _macro.DoCurrent(command);
        Data = _imageTarget.CurrentData;
        Width = Data.Width;
        Height = Data.Height+70;
    }
}