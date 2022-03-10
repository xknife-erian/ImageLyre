using ImageLaka;
using NLog;

namespace ImageLaka.Services.Macros.Commands;

public class CloseCommand : BaseMacroCommand
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public CloseCommand(ITarget target) 
        : base(target)
    {
    }

    /// <summary>
    /// 完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected override bool DoSpecific()
    {
        try
        {
            Target.Close();
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
        Target.Open();
    }
}