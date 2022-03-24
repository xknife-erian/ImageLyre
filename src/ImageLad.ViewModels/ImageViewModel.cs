using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Input;
using ImageLad.ImageEngine;
using ImageLad.Services.Macros;
using ImageLad.Services.Macros.Beats;
using ImageLad.ImageEngine.Enums;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace ImageLad.ViewModels;

public class ImageViewModel : ObservableRecipient
{
    private Macro? _macro;

    #region 属性


    #endregion

    #region 界面属性

    private ImageTarget? _imageTarget;
    private Bitmap? _bitmap;
    private PixelFormat _pixelFormat;
    private int _top;
    private int _left;

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

    public PixelFormat PixelFormat
    {
        get => _pixelFormat;
        set => SetProperty(ref _pixelFormat, value);
    }

    public int Top
    {
        get => _top;
        set => SetProperty(ref _top, value);
    }

    public int Left
    {
        get => _left;
        set => SetProperty(ref _left, value);
    }

    #endregion

    public void SetStartLocation(Point point)
    {
        Top = point.X;
        Left = point.Y;
    }

    public void Read(string path)
    {
        _macro = new Macro();
        _imageTarget = new ImageTarget(path);
        var command = new OpenBeat(_imageTarget);
        _macro.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }
    public void ToGray()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToGrayBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void ToRGB()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToRGBBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void ToCMYK()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToCMYKBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void ToHSV()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToHSVBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void ToLab()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToLabBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void To8Bit()
    {
        if(_imageTarget==null)
            return;
        var beat = new To8BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void To16Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To16BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void To24Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To24BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
    }

    public void To32Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To32BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        Bitmap = _imageTarget.Bitmap;
        PixelFormat = Bitmap.PixelFormat;
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
        var vm = obj as ImageViewModel;
        if (vm == null) return false;
        if(vm._imageTarget != null && vm._imageTarget.File.FullName.Equals(this._imageTarget?.File.FullName))
            return true;
        return false;
    }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return _imageTarget.File.FullName.GetHashCode() + _imageTarget.File.CreationTime.GetHashCode();
    }

    #endregion



}