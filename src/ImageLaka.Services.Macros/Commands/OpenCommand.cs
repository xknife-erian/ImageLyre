using ImageLaka;

namespace ImageLaka.Services.Macros.Commands;

public class OpenCommand : BaseMacroCommand
{
    public OpenCommand(ITarget target) : base(target)
    {
    }

    public override void Do()
    {
        Target.Open();
        IsDone = true;
    }

    public override void Undo()
    {
    }
}