using ImageLyre.ImageEngine;
using NLog;

namespace ImageLyre.Services.Macros.Beats;

public class ToCMYKBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public ToCMYKBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.ToCMYK();
            _Log.Info($"{ImageTarget.FileInfo.FullName} ToCMYK.");
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