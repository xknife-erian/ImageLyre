using ImageLaka.ImageEngine;

namespace ImageLaka.Services.Macros;

public abstract class BaseMacroBeat : IMacroBeat
{
    public BaseMacroBeat(ITarget target)
    {
        Target = target;
        ImageTarget = (ImageTarget) target;
    }

    /// <summary>
    ///     宏命令的操作目标
    /// </summary>
    public ITarget Target { get; }

    /// <summary>
    ///     宏命令的操作目标
    /// </summary>
    public ImageTarget ImageTarget { get; }

    /// <summary>
    ///     命令的操作是否已完成
    /// </summary>
    public bool IsDone { get; protected set; }

    /// <summary>
    ///     执行命令操作。如果正常完成，会将<see cref="IsDone" />属性置为true。
    /// </summary>
    public void Execute()
    {
        DoSpecific();
    }

    /// <summary>
    ///     命令的操作是否可以回退
    /// </summary>
    public bool UnExecuteEnable { get; protected set; } = true;

    /// <summary>
    ///     回退命令操作
    /// </summary>
    public abstract void UnExecute();

    /// <summary>
    ///     完成命令的操作
    /// </summary>
    /// <returns>如果正常完成后，返回true；否则返回false。</returns>
    protected abstract bool DoSpecific();
}