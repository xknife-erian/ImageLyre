namespace ImageLaka.ImageEngine;

public static class ImageTargetExtension
{
    /// <summary>
    /// 将目标图像转为8位灰度图
    /// </summary>
    public static void To8Bit(this ImageTarget target)
    {
        target.Bitmap = ImageUtil.ToGray(target.Bitmap, ImageBits.Bit8);
    }

    /// <summary>
    /// 将目标图像转为16位灰度图
    /// </summary>
    public static void To16Bit(this ImageTarget target)
    {
        target.Bitmap = ImageUtil.ToGray(target.Bitmap, ImageBits.Bit16);
    }

    /// <summary>
    /// 将目标图像转为32位灰度图
    /// </summary>
    public static void To32Bit(this ImageTarget target)
    {
        target.Bitmap = ImageUtil.ToGray(target.Bitmap, ImageBits.Bit32);
    }
}