using ImageLaka.Services.Macros.Commands;

namespace ImageLaka.Services.Macros;

public class Macro
{
    private readonly LinkedList<BaseCommand> Commands;

    public Macro()
    {
        Commands = new LinkedList<BaseCommand>();
    }

    public void Add(BaseCommand dc)
    {
        Commands.AddLast(dc);
    }

    public void AddAndDoCurrent(BaseCommand command)
    {
        Add(command);
        command.Do();
    }

    public void Remove(BaseCommand dc)
    {
        Commands.Remove(dc);
    }

    public void Do()
    {
        foreach (var command in Commands) command.Do();
    }

    public void DoLast()
    {
        Commands.Last().Do();
    }

}