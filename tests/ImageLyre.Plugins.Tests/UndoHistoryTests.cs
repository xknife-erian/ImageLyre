using System;
using System.Collections.Generic;
using ImageLyre.Contracts.Undo;
using ImageLyre.Module.Documents;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>撤销历史的硬行为：顺序、重做分支丢弃、容量上限与幂等。</summary>
public sealed class UndoHistoryTests
{
    [Fact]
    public void 压入后回退与重做按后进先出()
    {
        var history = new DocumentsModule().CreateStack();
        var log = new List<string>();

        history.Push(new RecordingAction("第一步", log));
        history.Push(new RecordingAction("第二步", log));

        Assert.Equal("第二步", history.UndoName);

        history.Undo();
        history.Undo();
        history.Redo();

        Assert.Equal(["undo:第二步", "undo:第一步", "redo:第一步"], log);
    }

    [Fact]
    public void 压入新步骤会丢弃重做分支()
    {
        var history = new DocumentsModule().CreateStack();
        var log = new List<string>();
        history.Push(new RecordingAction("旧", log));
        history.Undo();

        Assert.True(history.CanRedo);

        history.Push(new RecordingAction("新", log));

        Assert.False(history.CanRedo);
        Assert.Equal("新", history.UndoName);
    }

    [Fact]
    public void 超出容量时丢弃最早一步而不是清空()
    {
        var history = new DocumentsModule().CreateStack(capacity: 2);
        var log = new List<string>();

        history.Push(new RecordingAction("一", log));
        history.Push(new RecordingAction("二", log));
        history.Push(new RecordingAction("三", log));

        Assert.Equal("三", history.UndoName);
        history.Undo();
        history.Undo();
        Assert.False(history.CanUndo);
        Assert.Equal(["undo:三", "undo:二"], log);
    }

    [Fact]
    public void 空栈上的回退与重做是幂等的()
    {
        var history = new DocumentsModule().CreateStack();

        history.Undo();
        history.Redo();
        history.Clear();

        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Null(history.UndoName);
    }

    private sealed class RecordingAction(string name, List<string> log) : IUndoableAction
    {
        public string Name { get; } = name;

        public void Undo() => log.Add("undo:" + Name);

        public void Redo() => log.Add("redo:" + Name);
    }
}
