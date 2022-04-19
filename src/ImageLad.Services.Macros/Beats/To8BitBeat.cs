using ImageLad.ImageEngine;
using NLog;

namespace ImageLad.Services.Macros.Beats;

public class To8BitBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public To8BitBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.To8Bit();
            _Log.Info($"{ImageTarget.FileInfo.FullName} To8Bit.");

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