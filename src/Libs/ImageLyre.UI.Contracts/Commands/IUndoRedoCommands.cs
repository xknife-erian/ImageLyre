using System.Reactive;

namespace ImageLyre.UI.Contracts.Commands;
/// <summary>
/// 撤销与重做命令接口。
/// 由支持操作历史回退的组件（如文本编辑器、绘图板、节点图）实现。
/// 提供标准的 Undo/Redo 能力查询与执行方法。
/// </summary>
public interface IUndoRedoCommands
{
    /// <summary>
    /// 获取当前是否可以执行“撤销”操作。
    /// 通常由内部 Undo 栈是否为空决定。
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// 获取当前是否可以执行“重做”操作。
    /// 通常由内部 Redo 栈是否为空决定。
    /// </summary>
    bool CanRedo { get; }

    /// <summary>
    /// 获取最近一次可撤销操作的显示名称（用于菜单项，例如：“撤销 输入文本”）。
    /// 若无可撤销操作，应返回 null 或空字符串。
    /// </summary>
    string? UndoActionName { get; }

    /// <summary>
    /// 获取最近一次可重做操作的显示名称（例如：“重做 删除节点”）。
    /// 若无可重做操作，应返回 null 或空字符串。
    /// </summary>
    string? RedoActionName { get; }

    /// <summary>
    /// 执行“撤销”操作，回退到上一个状态。
    /// 调用前应确保 <see cref="CanUndo"/> 为 true。
    /// </summary>
    void Undo();

    /// <summary>
    /// 执行“重做”操作，恢复被撤销的状态。
    /// 调用前应确保 <see cref="CanRedo"/> 为 true。
    /// </summary>
    void Redo();

    /// <summary>
    /// 当 <see cref="CanUndo"/> 状态发生变化时推送（Unit 变更信号）。
    /// UI 可订阅此流以更新菜单或按钮的启用状态。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="CanUndo"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<Unit> CanUndoChanged { get; }

    /// <summary>
    /// 当 <see cref="CanRedo"/> 状态发生变化时推送（Unit 变更信号）。
    /// UI 可订阅此流以更新“重做”相关控件状态。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="CanRedo"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<Unit> CanRedoChanged { get; }
}
