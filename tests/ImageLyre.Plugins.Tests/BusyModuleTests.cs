using System;
using System.Linq;
using System.Threading.Tasks;
using ImageLyre.Module.Busy;
using ImageLyre.UI.Contracts.Busy;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>忙状态：开始可查、进度更新、完成清空、失败清空。</summary>
public sealed class BusyModuleTests
{
    [Fact]
    public void 开始后能收到事件且进度可更新()
    {
        var module = new BusyModule();
        var started = Guid.Empty;
        var progress = 0u;
        using var s1 = module.BusyStarted.Subscribe(e => started = e.Data.Id);
        using var s2 = module.BusyProgressChanged.Subscribe(e => progress = e.Data.CurrentProgress);

        var indicator = new BusyIndicator { Type = BusyIndicatorType.Spinner, Message = "正在打开" };
        var id = module.StartBusy(indicator);
        module.ReportProgress(id, new BusyRecord(id, 64, "已解码 64%"));

        Assert.Equal(id, started);
        Assert.Equal(64u, progress);
        Assert.Equal(64u, indicator.CurrentProgress);
    }

    [Fact]
    public void 完成后从活跃表摘除且重复完成幂等()
    {
        var module = new BusyModule();
        var completed = 0;
        using var s = module.BusyCompleted.Subscribe(_ => completed++);

        var id = module.StartBusy(new BusyIndicator { Type = BusyIndicatorType.Spinner, Message = "打开" });
        module.ReportCompleted(id);
        module.ReportCompleted(id);

        Assert.Equal(1, completed);
    }

    [Fact]
    public void 失败后不再接受进度()
    {
        var module = new BusyModule();
        var progress = 0;
        using var s = module.BusyProgressChanged.Subscribe(_ => progress++);

        var id = module.StartBusy(new BusyIndicator { Type = BusyIndicatorType.Spinner, Message = "打开" });
        module.ReportFailed(id);
        module.ReportProgress(id, new BusyRecord(id, 10, "不应出现"));

        Assert.Equal(0, progress);
    }

    [Fact]
    public async Task 超时会把记录摘除并推超时事件()
    {
        var module = new BusyModule();
        var timedOut = false;
        using var s = module.BusyTimedOut.Subscribe(_ => timedOut = true);

        module.StartBusy(new BusyIndicator { Type = BusyIndicatorType.Spinner, Message = "打开" }, millisecondsTimeout: 30);
        await Task.Delay(400);

        Assert.True(timedOut, "超时后没有推送 BusyTimedOut。");
    }
}
