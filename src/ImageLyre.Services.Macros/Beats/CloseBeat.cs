using NLog;

namespace ImageLyre.Services.Macros.Beats;

public class CloseBeat(ITarget target) : BaseMacroBeat(target)
{
    private static readonly ILogger s_log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected override bool DoSpecific()
    {
        try
        {
            Target.Close();
            s_log.Info($"{ImageTarget.FileInfo.FullName} 已关闭。");
            return true;
        }
        catch (Exception e)
        {
            s_log.Warn(e);
            return false;
        }
    }

    public override void UnExecute()
    {
        Target.Open();
    }
}