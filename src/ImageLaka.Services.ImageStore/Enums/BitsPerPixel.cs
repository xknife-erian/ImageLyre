namespace ImageLaka.ImageEngine.Enums;

/// <summary>
/// 像素深度是指存储每个像素所需要的比特数。
/// https://gitee.com/xknife/image-laka/wikis/%E7%9F%A5%E8%AF%86%E9%A2%86%E5%9F%9F/%E5%9B%BE%E5%83%8F%E5%A4%84%E7%90%86/%E5%83%8F%E7%B4%A0%E6%B7%B1%E5%BA%A6
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