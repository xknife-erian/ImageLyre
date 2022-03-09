using NLog;

namespace ImageLaka.Services.Macros.Commands;

public class To16BitCommand : BaseMacroCommand
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public To16BitCommand(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific()
    {
        try
        {
            Target.To16Bit();
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
        throw new NotImplementedException();
    }
}