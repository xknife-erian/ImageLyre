using ImageLad;

namespace UTest.ServicesTest.Macros.Demos;

/**
 * 以下三个演示类，用来描述及演示<see cref="ITarget"/>,<see cref="IMacroBeat"/>在设计过程中，以及
 * 当插件模式下的使用方法。
 * 2022/3/8
 */

public class TextTarget : ITarget
{
    public TextTarget(string source = "")
    {
        Text = source;
    }

    public string Text { get; set; }

    public void Open()
    {
        Text += $"{nameof(Open)}\r\n";
    }

    public void Close()
    {
        Text += $"{nameof(Close)}\r\n";
    }

    /// <summary>
    /// 将目标图像转为8位图
    /// </summary>
    public void To8Bit()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 将目标图像转为16位图
    /// </summary>
    public void To16Bit()
    {
        throw new System.NotImplementedException();
    }
}