using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Input;
using ImageLaka.ImageEngine;
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

    public BitsPerPixel BitsPerPixel
    {
        get
        {
            if (Bitmap == null)
                return BitsPerPixel.Undefined;
            switch (Bitmap.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555: //像素格式为每像素16位。该颜色信息指定32,768种色调，其中5位为红色，5位为绿色，5位为蓝色，1位为alpha。
                case PixelFormat.Format16bppGrayScale: //像素格式为每像素 16 位。该颜色信息指定 65536 种灰色调。
                case PixelFormat.Format16bppRgb555: //指定格式为每像素 16 位；红色、绿色和蓝色分量各使用 5 位。剩余的 1 位未使用。
                case PixelFormat.Format16bppRgb565: //指定格式为每像素 16 位；红色分量使用 5 位，绿色分量使用 6 位，蓝色分量使用 5 位。
                    return BitsPerPixel.Bit16;
                case PixelFormat.Format24bppRgb: //指定格式为每像素 24 位；红色、绿色和蓝色分量各使用 8 位。
                    return BitsPerPixel.Bit24;
                case PixelFormat.Canonical: //默认像素格式，每像素 32 位。此格式指定 24 位颜色深度和一个 8 位 alpha 通道。
                case PixelFormat.Format32bppArgb: //指定格式为每像素 32 位；alpha、红色、绿色和蓝色分量各使用 8 位。
                case PixelFormat.Format32bppPArgb: //指定格式为每像素 32 位；alpha、红色、绿色和蓝色分量各使用 8 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
                case PixelFormat.Format32bppRgb: //指定格式为每像素 32 位；红色、绿色和蓝色分量各使用 8 位。剩余的 8 位未使用。
                    return BitsPerPixel.Bit32;
                case PixelFormat.Format64bppArgb: //指定格式为每像素 64 位；alpha、红色、绿色和蓝色分量各使用 16 位。
                case PixelFormat.Format64bppPArgb: //指定格式为每像素 64 位；alpha、红色、绿色和蓝色分量各使用 16 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
                    return BitsPerPixel.Bit64;
                case PixelFormat.Format48bppRgb: //指定格式为每像素 48 位；红色、绿色和蓝色分量各使用 16 位。
                    return BitsPerPixel.Bit48;
                case PixelFormat.Format8bppIndexed: //指定格式为每像素 8 位而且已创建索引。因此颜色表中有 256 种颜色。
                    return BitsPerPixel.Bit8;
                case PixelFormat.Format4bppIndexed: //指定格式为每像素 4 位而且已创建索引。
                    return BitsPerPixel.Bit4;
                case PixelFormat.Format1bppIndexed: //指定像素格式为每像素 1 位，并指定它使用索引颜色。因此颜色表中有两种颜色。
                    return BitsPerPixel.Bit1;
                case PixelFormat.Alpha: //像素数据包含没有进行过自左乘的 alpha 值。
                case PixelFormat.Gdi: //像素数据包含 GDI 颜色。
                case PixelFormat.Indexed: //该像素数据包含颜色索引值，这意味着这些值是系统颜色表中颜色的索引，而不是单个颜色值。
                case PixelFormat.Max: //此枚举的最大值。
                case PixelFormat.PAlpha: //像素数据包含没有进行过自左乘的 alpha 值。
                case PixelFormat.Extended: //保留。
                case PixelFormat.Undefined: //未定义像素格式。
                default:
                    return BitsPerPixel.Undefined;
            }
        }
    }

    #endregion

    public void SetParentWindowBound(Rectangle rectangle)
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