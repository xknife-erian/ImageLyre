using ImageLaka.ImageEngine;
using NLog;

namespace ImageLaka.Services.Macros.Beats;

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
            _Log.Info($"{ImageTarget.File.FullName} ToHSV.");
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