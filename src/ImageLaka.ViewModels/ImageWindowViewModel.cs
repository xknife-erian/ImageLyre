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
    private string _pixelFormat;

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

    public string PixelFormat
    {
        get => _pixelFormat;
        set => SetProperty(ref _pixelFormat, value);
    }

    public int Top { get; private set; }
    public int Left { get; private set; }

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
        PixelFormat = Bitmap.PixelFormat.ToString();
    }
    public void ToGray()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToGrayBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void ToRGB()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToRGBBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void ToCMYK()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToCMYKBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void ToHSV()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToHSVBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void ToLab()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToLabBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void To8Bit()
    {
        if(_imageTarget==null)
            return;
        var beat = new To8BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void To16Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To16BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public void To32Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To32BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat.ToString();
    }

    public ICommand WindowActivated => new RelayCommand(OnWindowActivated);

    public event EventHandler? WindowIsActivated;

    private void OnWindowActivated()
    {
        WindowIsActivated?.Invoke(this, EventArgs.Empty);
    }

    #region Overrides of Object

    /// <summary>Determines whether the specified object is equal to the current object.</summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns>
    /// <see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        var vm = obj as ImageWindowViewModel;
        if (vm == null) return false;
        if(vm._imageTarget != null && vm._imageTarget.File.FullName.Equals(this._imageTarget?.File.FullName))
            return true;
        return false;
    }

    #endregion



}