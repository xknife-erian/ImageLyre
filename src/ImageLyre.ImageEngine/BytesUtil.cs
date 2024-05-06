namespace ImageLyre.ImageEngine
{
    public static class BytesUtil
    {
        /// <summary>
        /// 反转图像流
        /// </summary>
        /// <param name="src">源图像流</param>
        /// <param name="target">目标图像流</param>
        /// <param name="bytePerPixel">每个像素的字节数</param>
        public static void ReverseImageStream(byte[] src, byte[] target, ushort bytePerPixel)
        {
            var last = target.Length;
            for (int i = 0; i < src.Length; i += bytePerPixel)
            {
                last -= bytePerPixel;
                for (int j = 0; j < bytePerPixel; j++)
                    target[last + j] = src[i + j];
            }
        }
    }
}
