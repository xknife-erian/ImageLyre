using System;
using System.Reactive;

namespace ImageLyre.Contracts.Undo;

/// <summary>一步可撤销的改动：既能回退，也能重做，名字用于界面展示。</summary>
/// <remarks>
///     实现方负责保证 <see cref="Undo"/> 与 <see cref="Redo"/> 互为逆操作；栈只负责顺序与可用性，
///     不校验业务语义，因此同一步操作被外部改动破坏时，实现方应在内部自行兜底而不是让栈抛异常。
/// </remarks>
public interface IUndoableAction
{
    /// <summary>获取可读名称，例如"移动 ROI"或"设置比例尺"。</summary>
    string Name { get; }

    /// <summary>回退这一步。</summary>
    void Undo();

    /// <summary>重做这一步。</summary>
    void Redo();
}

/// <summary>撤销栈：按文档实例持有，只做顺序与可用性判定。</summary>
/// <remarks>
///     线程模型：栈的操作视为单线程（界面线程）；<see cref="Changed" /> 在引发变化的那条线程上推送，
///     订阅方自行调度。超出容量时从最早的一步开始丢弃，不会把整个栈清空。
/// </remarks>
public interface IUndoHistory
{
    /// <summary>获取是否还有可回退的步骤。</summary>
    bool CanUndo { get; }

    /// <summary>获取是否还有可重做的步骤。</summary>
    bool CanRedo { get; }

    /// <summary>获取下一步回退的名称；无可回退时为 <see langword="null" />。</summary>
    string? UndoName { get; }

    /// <summary>获取下一步重做的名称；无可重做时为 <see langword="null" />。</summary>
    string? RedoName { get; }

    /// <summary>压入一步已经发生的改动；压入后原有的重做分支被丢弃。</summary>
    /// <param name="action">已经执行完成的改动。</param>
    void Push(IUndoableAction action);

    /// <summary>回退最近一步；无可回退时不做事。</summary>
    void Undo();

    /// <summary>重做最近被回退的一步；无可重做时不做事。</summary>
    void Redo();

    /// <summary>清空全部历史，例如文档关闭或重新加载之后。</summary>
    void Clear();

    /// <summary>可用性或名称变化时推送。</summary>
    IObservable<Unit> Changed { get; }
}
