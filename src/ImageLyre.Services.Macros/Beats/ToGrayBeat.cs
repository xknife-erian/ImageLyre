using ImageLyre.ImageEngine;
using NLog;

namespace ImageLyre.Services.Macros.Beats;

public class ToGrayBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public ToGrayBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.ToGray();
            _Log.Info($"{ImageTarget.FileInfo.FullName} ToGray.");
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