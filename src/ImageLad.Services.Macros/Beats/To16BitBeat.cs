using ImageLad.ImageEngine;
using NLog;

namespace ImageLad.Services.Macros.Beats;

public class To16BitBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public To16BitBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.To16Bit();
            _Log.Info($"{ImageTarget.FileInfo.FullName} To16Bit.");
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