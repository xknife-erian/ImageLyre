namespace ImageLaka.ImageEngine
{
    public class ImageTarget : ITarget
    {
        public ImageReader Reader { get; set; }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}