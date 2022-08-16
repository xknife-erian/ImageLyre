namespace ImageLyric.ImageEngine.Enums;

/// <summary>
/// 像素深度是指存储每个像素所需要的比特数。
/// https://gitee.com/xknife/ImageLad/wikis/知识领域/图像处理/像素深度
/// </summary>
public enum BitsPerPixel : ushort
{
    Undefined =0,
    Bit1 = 1,
    Bit4 = 4,
    Bit8 = 8,
    Bit16 = 16,
    Bit24 = 24, 
    Bit32 = 32,
    Bit48 = 48,
    Bit64 = 64
}