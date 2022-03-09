using System.Drawing;

namespace ImageLaka.ImageEngine;

/// <summary>
/// 描述一个被操作的目标
/// </summary>
public class ImageTarget : ITarget
{
    private FileInfo _fileInfo;

    public ImageTarget(string path)
    {
        _fileInfo = new FileInfo(path);
    }

    public Bitmap Bitmap { get; set; }

    #region Implementation of ITarget

    /// <summary>
    /// 打开目标，以保证目标可操作。与<see cref="Open"/>相对应。
    /// </summary>
    public void Open()
    {
        Bitmap = ImageUtil.Read(_fileInfo.FullName);
    }

    /// <summary>
    /// 关闭目标，以保证目标在下次打开<see cref="Open"/>前不再会被操作。
    /// </summary>
    public void Close()
    {
        _fileInfo = null;
        Bitmap = null;
    }

    /// <summary>
    /// 将目标图像转为8位图
    /// </summary>
    public void To8Bit()
    {
        Bitmap = ImageUtil.Gray(Bitmap);
    }

    /// <summary>
    /// 将目标图像转为16位图
    /// </summary>
    public void To16Bit()
    {
        throw new NotImplementedException();
    }

    #endregion
}