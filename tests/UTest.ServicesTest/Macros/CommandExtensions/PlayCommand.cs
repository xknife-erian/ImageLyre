using System.Drawing;
using ImageLaka;

namespace UTest.ServicesTest.Macros.CommandExtensions;

/**
 * 以下三个演示类，用来描述及演示<see cref="ITarget"/>,<see cref="IMacroCommand"/>在设计过程中，以及
 * 当插件模式下的使用方法。
 * 2022/3/8
 */

public class TextTarget : ITarget
{
    public TextTarget(string source = "")
    {
        Target = source;
    }

    public string Target { get; set; }

    public void Open()
    {
        Target += $"{nameof(Open)}\r\n";
    }

    public void Close()
    {
        Target += $"{nameof(Close)}\r\n";
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

public static class TargetExtensions
{
    public const string PLAY = "Play\r\n";

    public static void Play(this TextTarget it)
    {
        it.Target += PLAY;
    }

    public static void UnPlay(this TextTarget it)
    {
        it.Target += it.Target.Replace(PLAY, string.Empty);
    }
}

public class PlayCommand : BaseMacroCommand
{
    private readonly TextTarget _textTarget;

    public PlayCommand(ITarget target)
        : base(target)
    {
        _textTarget = (TextTarget) target;
    }

    /// <summary>
    /// 完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected override bool DoSpecific()
    {
        _textTarget.Play();
        return true;
    }

    public override void Undo()
    {
        _textTarget.UnPlay();
    }
}
