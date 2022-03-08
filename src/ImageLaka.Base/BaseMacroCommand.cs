// ReSharper disable once CheckNamespace
namespace ImageLaka;

public abstract class BaseMacroCommand : IMacroCommand
{
    /// <summary>
    /// 宏命令的操作目标
    /// </summary>
    public ITarget Target { get; private set; }

    /// <summary>
    /// 命令的操作是否已完成
    /// </summary>
    public bool IsDone { get; protected set; } = false;

    public BaseMacroCommand(ITarget target)
    {
        Target = target;
    }

    /// <summary>
    /// 执行命令操作。如果正常完成，会将<see cref="IsDone"/>属性置为true。
    /// </summary>
    public virtual void Do()
    {
        IsDone = DoSpecific();
    }

    /// <summary>
    /// 完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected abstract bool DoSpecific();

    /// <summary>
    /// 命令的操作是否可以回退
    /// </summary>
    public bool UndoEnable { get; protected set; } = true;

    /// <summary>
    /// 回退命令操作
    /// </summary>
    public abstract void Undo();
}