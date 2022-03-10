using ImageLaka.ImageEngine;
using NLog;

namespace ImageLaka.Services.Macros.Commands;

public class To32BitBeat : BaseMacroBeat
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public To32BitBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.To32Bit();
            return true;
        }
        catch (Exception e)
        {
            Log.Warn(e);
            return false;
        }
    }

    public override void UnExecute()
    {
        throw new NotImplementedException();
    }
}