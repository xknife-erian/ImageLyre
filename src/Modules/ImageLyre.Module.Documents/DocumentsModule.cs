using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Contracts.Undo;

namespace ImageLyre.Module.Documents;

/// <summary>撤销栈实现：按文档实例持有，容量默认五十步。</summary>
/// <remarks>
///     压入新一步会丢弃重做分支，这是"改动之后旧的重做不再有效"的常规语义；超出容量时丢最早的一步，
///     不整栈清空，因为整栈清空会让长会话的用户在第五十一步突然失去全部历史。
/// </remarks>
[Plugin(
    id: "ImageLyre.Documents.Module",
    name: "文档与撤销",
    alias: "Documents",
    description: "撤销栈与文档会话的基础能力")]
public sealed class DocumentsModule : BasePicoModule<IUndoHistoryFactory>, IUndoHistoryFactory, IDisposable
{
    private readonly Subject<Unit> _created = new();
    private readonly Lazy<IUndoHistoryFactory> _self;
    private bool _disposed;

    /// <summary>创建文档模块。</summary>
    public DocumentsModule()
    {
        _self = new Lazy<IUndoHistoryFactory>(() => this);
    }

    /// <inheritdoc />
    public override Lazy<IUndoHistoryFactory> Build(params object[] args) => _self;

    /// <summary>获取新栈创建事件，便于壳在撤销菜单上挂变化。</summary>
    public IObservable<Unit> StackCreated => _created.AsObservable();

    /// <inheritdoc />
    public IUndoHistory CreateStack(int capacity = 50)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);

        var stack = new UndoHistory(capacity);
        _created.OnNext(Unit.Default);
        return stack;
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _created.Dispose();
        base.Dispose();
    }
}

/// <summary>撤销栈工厂：文档模块对外的唯一入口。</summary>
public interface IUndoHistoryFactory
{
    /// <summary>为一份文档创建空栈。</summary>
    /// <param name="capacity">容量上限，默认五十步。</param>
    /// <returns>尚未包含任何历史的撤销栈。</returns>
    IUndoHistory CreateStack(int capacity = 50);
}

/// <summary>默认撤销栈：两个列表分别承载已执行与已回退的步骤。</summary>
internal sealed class UndoHistory : IUndoHistory, IDisposable
{
    private readonly List<IUndoableAction> _done = [];
    private readonly List<IUndoableAction> _undone = [];
    private readonly Subject<Unit> _changed = new();
    private readonly int _capacity;

    internal UndoHistory(int capacity) => _capacity = capacity;

    public bool CanUndo => _done.Count > 0;

    public bool CanRedo => _undone.Count > 0;

    public string? UndoName => CanUndo ? _done[^1].Name : null;

    public string? RedoName => CanRedo ? _undone[^1].Name : null;

    public IObservable<Unit> Changed => _changed.AsObservable();

    public void Push(IUndoableAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        _done.Add(action);
        _undone.Clear();

        if (_done.Count > _capacity)
        {
            _done.RemoveAt(0);
        }

        _changed.OnNext(Unit.Default);
    }

    public void Undo()
    {
        if (!CanUndo)
        {
            return;
        }

        var action = _done[^1];
        _done.RemoveAt(_done.Count - 1);
        action.Undo();
        _undone.Add(action);
        _changed.OnNext(Unit.Default);
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            return;
        }

        var action = _undone[^1];
        _undone.RemoveAt(_undone.Count - 1);
        action.Redo();
        _done.Add(action);
        _changed.OnNext(Unit.Default);
    }

    public void Clear()
    {
        _done.Clear();
        _undone.Clear();
        _changed.OnNext(Unit.Default);
    }

    public void Dispose() => _changed.Dispose();
}
