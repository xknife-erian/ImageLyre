using ImageLyric.ImageEngine;
using NLog;

namespace ImageLyric.Services.Macros.Beats;

public class ToHSVBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public ToHSVBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.ToHSV();
            _Log.Info($"{ImageTarget.FileInfo.FullName} ToHSV.");
            return true;
        }
        catch (Exception e)
        {
            _Log.Warn(e);
            return false;
        }
    }

    public override void UnExecute()
    {
        throw new NotImplementedException();
    }
}