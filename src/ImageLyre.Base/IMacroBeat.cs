namespace ImageLyric;

/// <summary>
/// 本接口描述一个宏节拍。
/// 事实上，在业界这里应该被称之为command，即指令(命令)，但在本项目中为避免与WPF的command混淆，故重新定义名称为“节拍”。
/// 每一个宏都象一首歌曲一样(歌曲有长有短)，由一个一个节拍构成。
/// </summary>
public interface IMacroBeat
{
    /// <summary>
    ///     宏命令的操作目标
    /// </summary>
    ITarget Target { get; }

    /// <summary>
    ///     命令的操作是否已完成
    /// </summary>
    bool IsDone { get; }

    /// <summary>
    ///     命令的操作是否可以回退
    /// </summary>
    bool UnExecuteEnable { get; }

    /// <summary>
    ///     执行命令操作。如果正常完成，会将<see cref="IsDone" />属性置为true。
    /// </summary>
    void Execute();

    /// <summary>
    ///     回退命令操作
    /// </summary>
    void UnExecute();
}