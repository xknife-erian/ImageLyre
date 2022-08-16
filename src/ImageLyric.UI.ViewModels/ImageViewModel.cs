using System.Drawing.Imaging;
using System.Windows.Input;
using ImageLyric.ImageEngine;
using ImageLyric.Services.Macros;
using ImageLyric.Services.Macros.Beats;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using OpenCvSharp;
using Point = System.Drawing.Point;

namespace ImageLyric.UI.ViewModels;

public class ImageViewModel : ObservableRecipient
{
    private Macro? _macro;

    #region 属性


    #endregion

    #region 界面属性

    private ImageTarget? _imageTarget;
    private Mat? _bmpMat;
    private int _top;
    private int _left;
    private PixelFormat _pixelFormat;

    public Mat? BmpMat
    {
        get => _bmpMat;
        set => SetProperty(ref _bmpMat, value);
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
        BmpMat = _imageTarget.BmpMat;
    }
    public void ToGray()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToGrayBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void ToRGB()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToRGBBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void ToCMYK()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToCMYKBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void ToHSV()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToHSVBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void ToLab()
    {
        if (_imageTarget == null)
            return;
        var beat = new ToLabBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void To8Bit()
    {
        if(_imageTarget==null)
            return;
        var beat = new To8BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void To16Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To16BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void To24Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To24BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
    }

    public void To32Bit()
    {
        if (_imageTarget == null)
            return;
        var beat = new To32BitBeat(_imageTarget);
        _macro?.DoCurrent(beat);
        BmpMat = _imageTarget.BmpMat;
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
        if(vm._imageTarget != null && vm._imageTarget.FileInfo.FullName.Equals(this._imageTarget?.FileInfo.FullName))
            return true;
        return false;
    }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return _imageTarget.FileInfo.FullName.GetHashCode() + _imageTarget.FileInfo.CreationTime.GetHashCode();
    }

    #endregion



}