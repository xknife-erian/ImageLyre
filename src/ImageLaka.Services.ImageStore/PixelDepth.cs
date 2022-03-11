namespace ImageLaka.ImageEngine;

/// <summary>
/// 像素深度是指存储每个像素所需要的比特数。
/// 像素深度也是用来度量图像的色彩分辨率的，他确定了彩色图像的每个像素可能有的色彩数，或者确定灰度图像的每个像素可能有的灰度级数。
/// </summary>
public enum PixelDepth
{
    /// <summary>
    /// 每个像素有8位,则最大灰度数目为8次方,即256
    /// </summary>
    Bit8,
    /// <summary>
    /// 16位(65536色)
    /// </summary>
    Bit16,
    /// <summary>
    /// 一个像素共用24位表示，就说像素的深度为24，每个像素可以是16777216(2的24次方)种颜色中的一种
    /// </summary>
    Bit24, 
    Bit32
}