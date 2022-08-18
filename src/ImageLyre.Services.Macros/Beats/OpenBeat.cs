using NLog;

namespace ImageLyric.Services.Macros.Beats;

public class OpenBeat : BaseMacroBeat
{
    private static readonly ILogger _Log = LogManager.GetCurrentClassLogger();

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
            _Log.Info($"{ImageTarget.FileInfo.FullName} 已打开。");
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
        Target.Close();
    }

    #endregion
}