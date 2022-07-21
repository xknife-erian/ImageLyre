using System.Drawing;
using System.IO;
using ImageMagick;

namespace ImageLad.ImageEngine;

/// <summary>
/// 描述一个被操作的目标
/// </summary>
public class ImageTarget : ITarget
{
    public ImageTarget(string path)
    {
        FileInfo = new FileInfo(path);
    }

    public FileInfo FileInfo { get; private set; }
    public Bitmap? Bitmap { get; set; }

    #region Implementation of ITarget

    /// <summary>
    /// 打开目标，以保证目标可操作。与<see cref="Open"/>相对应。
    /// </summary>
    public void Open()
    {
        using var stream = FileInfo.Open(FileMode.Open);
        Bitmap = new Bitmap(stream);
    }

    /// <summary>
    /// 关闭目标，以保证目标在下次打开<see cref="Open"/>前不再会被操作。
    /// </summary>
    public void Close()
    {
        Bitmap?.Dispose();
        Bitmap = null;
    }

    #endregion
}