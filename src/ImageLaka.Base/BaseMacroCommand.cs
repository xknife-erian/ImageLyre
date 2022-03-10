// ReSharper disable once CheckNamespace

namespace ImageLaka;

public abstract class BaseMacroCommand : IMacroCommand
{
    public BaseMacroCommand(ITarget target)
    {
        Target = target;
    }

    /// <summary>
    ///     宏命令的操作目标
    /// </summary>
    public ITarget Target { get; }

    /// <summary>
    ///     命令的操作是否已完成
    /// </summary>
    public bool IsDone { get; protected set; }

    /// <summary>
    ///     执行命令操作。如果正常完成，会将<see cref="IsDone" />属性置为true。
    /// </summary>
    public virtual void Execute(object? parameter)
    {
        IsDone = DoSpecific(parameter);
    }

    /// <summary>
    ///     命令的操作是否可以回退
    /// </summary>
    public bool UndoEnable { get; protected set; } = true;

    /// <summary>
    ///     回退命令操作
    /// </summary>
    public abstract void UnExecute();

    /// <summary>Defines the method that determines whether the command can execute in its current state.</summary>
    /// <param name="parameter">
    ///     Data used by the command.  If the command does not require data to be passed, this object can
    ///     be set to <see langword="null" />.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if this command can be executed; otherwise, <see langword="false" />.
    /// </returns>
    public abstract bool CanExecute(object? parameter);

    /// <summary>Occurs when changes occur that affect whether or not the command should execute.</summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    ///     完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected abstract bool DoSpecific(object? parameter);
}