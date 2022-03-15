using ImageLaka;
using ImageLaka.ImageEngine;
using NLog;

namespace ImageLaka.Services.Macros.Commands;

public class OpenBeat : BaseMacroBeat
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public OpenBeat(ITarget target) : base(target)
    {
    }

    #region Overrides of BaseMacroBeat

    /// <summary>
    /// 完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected override bool DoSpecific()
    {
        try
        {
            Target.Open();
            Log.Info($"{ImageTarget.File.FullName} {nameof(Target.Open)}.");
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
        Target.Close();
    }

    #endregion
}