// ReSharper disable once CheckNamespace
namespace ImageLaka;

/// <summary>
/// 描述一个宏命令的接口
/// </summary>
public interface IMacroCommand
{
    /// <summary>
    /// 宏命令的操作目标
    /// </summary>
    ITarget Target { get; }
    /// <summary>
    /// 命令的操作是否已完成
    /// </summary>
    bool IsDone { get; }
    /// <summary>
    /// 执行命令操作
    /// </summary>
    void Do();
    /// <summary>
    /// 命令的操作是否可以回退
    /// </summary>
    bool UndoEnable { get; }
    /// <summary>
    /// 回退命令操作
    /// </summary>
    void Undo();
}