namespace ImageLyre.Services.Macros;

/// <summary>
/// 宏。
/// 宏是动作(命令<see cref="IMacroBeat"/>)的管理集合，以后对命令的直接操作。
/// 宏不关心命令所操作的目标，宏只关心命令。
/// </summary>
public class Macro
{
    /// <summary>
    /// 动作的管理集合
    /// </summary>
    private readonly LinkedList<IMacroBeat> _commands = new();

    /// <summary>
    /// 执行一个或多个命令。当命令执行完成后，将命令置入管理集合中。
    /// </summary>
    /// <param name="commands">一个或多个命令</param>
    public void DoCurrent(params IMacroBeat[] commands)
    {
        foreach (var command in commands)
        {
            command.Execute();
            _commands.AddLast(command);
        }
    }

    /// <summary>
    /// 从命令集合中移除指定的命令。
    /// </summary>
    /// <param name="dc">指定的命令</param>
    public void Remove(IMacroBeat dc)
    {
        _commands.Remove(dc);
    }
}