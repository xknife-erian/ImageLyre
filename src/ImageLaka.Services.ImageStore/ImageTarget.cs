using System.Drawing;

namespace ImageLaka.ImageEngine;

public class ImageTarget : ITarget
{
    private FileInfo _fileInfo;

    public ImageTarget(string path)
    {
        _fileInfo = new FileInfo(path);
    }

    public Bitmap CurrentData { get; set; }

    public void Open()
    {
        CurrentData = ImageUtil.Read(_fileInfo.FullName);
    }

    public void Close()
    {
        _fileInfo = null;
        CurrentData = null;
    }
}