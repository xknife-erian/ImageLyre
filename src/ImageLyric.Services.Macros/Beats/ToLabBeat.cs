using ImageLyric.ImageEngine;
using NLog;

namespace ImageLyric.Services.Macros.Beats;

public class ToLabBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

    public ToLabBeat(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            ImageTarget.ToLab();
            _Log.Info($"{ImageTarget.FileInfo.FullName} ToLab.");
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