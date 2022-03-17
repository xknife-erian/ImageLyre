using ImageLaka.ImageEngine;
using NLog;

namespace ImageLaka.Services.Macros.Beats;

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
            _Log.Info($"{ImageTarget.File.FullName} ToGray.");
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