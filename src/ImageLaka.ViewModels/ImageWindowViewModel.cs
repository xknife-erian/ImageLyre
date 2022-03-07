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
    private BitmapData _data;

    public BitmapData Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    public void Read(string path)
    {
        _macro = new Macro();
        _imageTarget = new ImageTarget(path);
        var command = new OpenCommand(_imageTarget);
        _macro.AddAndDoCurrent(command);
        Data = _imageTarget.CurrentData;
    }
}