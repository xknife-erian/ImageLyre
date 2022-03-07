// ReSharper disable once CheckNamespace
namespace ImageLaka;

public abstract class BaseCommand
{
    protected ITarget _target;

    public BaseCommand(ITarget target)
    {
        _target = target;
    }

    public abstract void Do();

    public abstract void Undo();
}