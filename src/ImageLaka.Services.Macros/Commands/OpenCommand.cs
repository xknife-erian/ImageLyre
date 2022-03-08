using ImageLaka;
using NLog;

namespace ImageLaka.Services.Macros.Commands;

public class OpenCommand : BaseMacroCommand
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public OpenCommand(ITarget target) : base(target)
    {
    }

    #region Overrides of BaseMacroCommand

    /// <summary>
    /// 完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected override bool DoSpecific()
    {
        try
        {
            Target.Open();
            return true;
        }
        catch (Exception e)
        {
            Log.Warn(e);
            return false;
        }
    }

    public override void Undo()
    {
        Target.Close();
    }

    #endregion
}