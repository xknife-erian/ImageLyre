using NLog;

namespace ImageLaka.Services.Macros.Commands;

public class To8BitCommand : BaseMacroCommand
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public To8BitCommand(ITarget target) : base(target)
    {
    }

    protected override bool DoSpecific(object? parameter)
    {
        try
        {
            Target.To8Bit();
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
        throw new NotImplementedException();
    }

    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">
    ///     Data used by the command.  If the command does not require data to be passed, this object can
    ///     be set to <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
    /// </returns>
    public override bool CanExecute(object? parameter)
    {
        throw new NotImplementedException();
    }
}