using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Input;
using ImageLaka.ImageEngine;
using ImageLaka.ImageEngine.Enums;
using ImageLaka.Services.Macros;
using ImageLaka.Services.Macros.Commands;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace ImageLaka.ViewModels;

public class ImageWindowViewModel : ObservableRecipient
{
    private Macro? _macro;

    #region 界面属性

    private ImageTarget? _imageTarget;
    private Bitmap? _bitmap;

    public Bitmap? Bitmap
    {
        get => _bitmap;
        set => SetProperty(ref _bitmap, value);
    }

    public ImageTarget? Target
    {
        get => _imageTarget;
        set => SetProperty(ref _imageTarget, value);
    }

    public int Top { get; private set; }
    public int Left { get; private set; }

    public BitsPerPixel BitsPerPixel => ImageUtil.GetBitsPerPixel(Bitmap);

    #endregion

    public void SetParentWindowRectangle(Rectangle rectangle)
    {
        Top = rectangle.Top + rectangle.Height + 50;
        Left = rectangle.Left + 50;
    }

    public void Read(string path)
    {
        _macro = new Macro();
        _imageTarget = new ImageTarget(path);
        var command = new OpenBeat(_imageTarget);
        _macro.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }

    public void To8Bit()
    {
        if(_imageTarget==null)
            return;
        var command = new To8BitBeat(_imageTarget);
        _macro?.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }

    public void To16Bit()
    {
        if (_imageTarget == null)
            return;
        var command = new To16BitBeat(_imageTarget);
        _macro?.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }

    public void To32Bit()
    {
        if (_imageTarget == null)
            return;
        var command = new To32BitBeat(_imageTarget);
        _macro?.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }


    public ICommand WindowActivated => new RelayCommand(OnWindowActivated);

    public event EventHandler? WindowIsActivated;

    private void OnWindowActivated()
    {
        WindowIsActivated?.Invoke(this, EventArgs.Empty);
    }
}