using ImageLaka;

namespace ImageLaka.Services.Macros.Commands;

public class CloseCommand : BaseCommand
{
    public CloseCommand(ITarget target) : base(target)
    {
    }

    public override void Do()
    {
        _target.Close();
    }

    public override void Undo()
    {
        throw new NotImplementedException();
    }
}