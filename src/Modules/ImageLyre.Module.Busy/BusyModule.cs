using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.UI.Contracts.Busy;

namespace ImageLyre.Module.Busy;

/// <summary>忙状态：登记一次长操作的生命周期，把开始、进度、完成、失败与超时五种变化推给渲染层。</summary>
/// <remarks>
///     <para>
///         记录以 <see cref="Guid"/> 为键；完成、失败、超时都会把它从活跃表里摘掉，重复上报幂等。
///         超时由构造时传入的毫秒数决定，负数表示不超时。
///     </para>
///     <para>
///         线程模型：成员可在任意线程调用，流在调用线程上推送；渲染层自行切回界面线程。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.Busy.Module",
    name: "忙状态",
    alias: "Busy",
    description: "长操作的进度、取消与超时")]
public sealed class BusyModule : BasePicoModule<IBusyManager>, IBusyManager, IDisposable
{
    private readonly Dictionary<Guid, BusyIndicator> _active = [];
    private readonly Dictionary<Guid, CancellationTokenSource> _timeouts = [];
    private readonly Subject<BusyEventArgs<BusyIndicator>> _started = new();
    private readonly Subject<BusyEventArgs<BusyRecord>> _progress = new();
    private readonly Subject<BusyEventArgs<Guid>> _completed = new();
    private readonly Subject<BusyEventArgs<Guid>> _failed = new();
    private readonly Subject<BusyEventArgs<Guid>> _timedOut = new();
    private bool _disposed;

    /// <summary>创建忙状态模块。</summary>
    public BusyModule()
    {
    }

    /// <inheritdoc />
    public IObservable<BusyEventArgs<BusyIndicator>> BusyStarted => _started.AsObservable();

    /// <inheritdoc />
    public IObservable<BusyEventArgs<BusyRecord>> BusyProgressChanged => _progress.AsObservable();

    /// <inheritdoc />
    public IObservable<BusyEventArgs<Guid>> BusyCompleted => _completed.AsObservable();

    /// <inheritdoc />
    public IObservable<BusyEventArgs<Guid>> BusyFailed => _failed.AsObservable();

    /// <inheritdoc />
    public IObservable<BusyEventArgs<Guid>> BusyTimedOut => _timedOut.AsObservable();

    /// <inheritdoc />
    protected override IBusyManager Module => this;

    /// <inheritdoc />
    public Guid StartBusy(BusyIndicator busyIndicator, int millisecondsTimeout = -1)
    {
        ArgumentNullException.ThrowIfNull(busyIndicator);

        _active[busyIndicator.Id] = busyIndicator;
        _started.OnNext(new BusyEventArgs<BusyIndicator>(busyIndicator));

        if (millisecondsTimeout > 0)
        {
            var cancellation = new CancellationTokenSource();
            _timeouts[busyIndicator.Id] = cancellation;
            _ = ExpireAsync(busyIndicator.Id, millisecondsTimeout, cancellation.Token);
        }

        return busyIndicator.Id;
    }

    /// <inheritdoc />
    public void ReportProgress(Guid busyIndicatorId, BusyRecord busyRecord)
    {
        if (!_active.TryGetValue(busyIndicatorId, out var indicator))
        {
            return;
        }

        indicator.CurrentProgress = busyRecord.CurrentProgress;
        if (!string.IsNullOrEmpty(busyRecord.Message))
        {
            indicator.Message = busyRecord.Message;
        }

        _progress.OnNext(new BusyEventArgs<BusyRecord>(busyRecord));
    }

    /// <inheritdoc />
    public void ReportCompleted(Guid busyIndicatorId) => Finish(busyIndicatorId, _completed);

    /// <inheritdoc />
    public void ReportFailed(Guid busyIndicatorId) => Finish(busyIndicatorId, _failed);

    /// <inheritdoc />
    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var timeout in _timeouts.Values)
        {
            timeout.Cancel();
            timeout.Dispose();
        }

        _timeouts.Clear();
        _active.Clear();
        _started.Dispose();
        _progress.Dispose();
        _completed.Dispose();
        _failed.Dispose();
        _timedOut.Dispose();
        base.Dispose();
    }

    private void Finish(Guid id, Subject<BusyEventArgs<Guid>> stream)
    {
        if (!_active.Remove(id))
        {
            return;
        }

        if (_timeouts.Remove(id, out var timeout))
        {
            timeout.Cancel();
            timeout.Dispose();
        }

        stream.OnNext(new BusyEventArgs<Guid>(id));
    }

    private async Task ExpireAsync(Guid id, int millisecondsTimeout, CancellationToken token)
    {
        try
        {
            await Task.Delay(millisecondsTimeout, token).ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (_active.Remove(id))
        {
            _timedOut.OnNext(new BusyEventArgs<Guid>(id));
        }
    }
}
