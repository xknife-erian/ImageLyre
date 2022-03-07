using ImageLaka;

namespace UTest.ServicesTest.Macros.CommandExtensions;

public class TextTarget : ITarget
{
    public string TestTarget { get; set; } = string.Empty;

    public void Open()
    {
        TestTarget += $"{nameof(Open)}/";
    }

    public void Close()
    {
        TestTarget += $"{nameof(Close)}/";
    }
}

public static class TargetExtensions
{
    public static void Play(this TextTarget it)
    {
        it.TestTarget += $"{nameof(Play)}/";
    }

    public static void UnPlay(this TextTarget it)
    {
        it.TestTarget += it.TestTarget.Replace($"{nameof(Play)}/", string.Empty);
    }
}

public class PlayCommand : BaseCommand
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
