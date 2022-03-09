﻿using System.Drawing;
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

    #region 依赖属性

    private Bitmap _bitmap;

    public Bitmap Bitmap
    {
        get => _bitmap;
        set => SetProperty(ref _bitmap, value);
    }

    public int Top { get; set; }
    public int Left { get; set; }

    #endregion

    public void Read(string path)
    {
        _macro = new Macro();
        _imageTarget = new ImageTarget(path);
        var command = new OpenCommand(_imageTarget);
        _macro.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }

    public void To8Bit()
    {
        if (_macro == null) return;
        var command = new To8BitCommand(_imageTarget);
        _macro.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }

    public void To16Bit()
    {
        if (_macro == null) return;
        var command = new To16BitCommand(_imageTarget);
        _macro.DoCurrent(command);
        Bitmap = _imageTarget.Bitmap;
    }

}

