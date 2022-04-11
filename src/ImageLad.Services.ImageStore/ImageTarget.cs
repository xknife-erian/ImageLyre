﻿using System.Drawing;
using SkiaSharp;

namespace ImageLad.ImageEngine;

/// <summary>
/// 描述一个被操作的目标
/// </summary>
public class ImageTarget : ITarget
{
    public ImageTarget(string path)
    {
        File = new FileInfo(path);
    }

    public FileInfo File { get; private set; }
    public SKBitmap Bitmap { get; set; }

    #region Implementation of ITarget

    /// <summary>
    /// 打开目标，以保证目标可操作。与<see cref="Open"/>相对应。
    /// </summary>
    public void Open()
    {
        var fs = File.Open(FileMode.Open);
        Bitmap = SKBitmap.Decode(fs);
    }

    /// <summary>
    /// 关闭目标，以保证目标在下次打开<see cref="Open"/>前不再会被操作。
    /// </summary>
    public void Close()
    {
        File = null;
        Bitmap = null;
    }

    #endregion
}