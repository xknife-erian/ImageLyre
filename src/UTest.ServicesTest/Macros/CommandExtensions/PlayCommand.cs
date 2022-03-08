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

    public override void Do()
    {
        _textTarget.Play();
    }

    public override void Undo()
    {
        _textTarget.UnPlay();
    }
}
