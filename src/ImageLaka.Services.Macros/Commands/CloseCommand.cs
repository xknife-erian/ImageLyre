using ImageLaka;

namespace ImageLaka.Services.Macros.Commands;

public class CloseCommand : BaseMacroCommand
{
    public CloseCommand(ITarget target) : base(target)
    {
    }

    public override void Do()
    {
        Target.Close();
    }

    public override void Undo()
    {
        throw new NotImplementedException();
    }
}