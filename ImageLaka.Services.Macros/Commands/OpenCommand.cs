using ImageLaka;

namespace ImageLaka.Services.Macros.Commands;

public class OpenCommand : BaseCommand
{
    public OpenCommand(ITarget target) : base(target)
    {
    }

    public override void Do()
    {
        _target.Open();
    }

    public override void Undo()
    {
        throw new NotImplementedException();
    }
}