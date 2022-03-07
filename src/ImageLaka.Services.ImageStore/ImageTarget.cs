using System.Drawing.Imaging;

namespace ImageLaka.ImageEngine
{
    public class ImageTarget : ITarget
    {
        private FileInfo _fileInfo;
        public BitmapData CurrentData { get; set; }

        public ImageTarget(string path)
        {
            _fileInfo = new FileInfo(path);
        }

        public void Open()
        {
            CurrentData = ImageHelper.Read(_fileInfo.FullName);
        }

        public void Close()
        {
            _fileInfo = null;
            CurrentData = null;
        }
    }
}