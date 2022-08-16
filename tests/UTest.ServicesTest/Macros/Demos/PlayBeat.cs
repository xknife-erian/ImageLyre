using ImageLyric;
using ImageLyric.Services.Macros;

namespace UTest.ServicesTest.Macros.Demos;

public class PlayBeat : BaseMacroBeat
{
    private readonly TextTarget _textTarget;

    public PlayBeat(ITarget target)
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

    public override void UnExecute()
    {
        _textTarget.UnPlay();
    }
}
