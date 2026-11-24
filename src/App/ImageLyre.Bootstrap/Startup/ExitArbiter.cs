using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Startup;

/// <summary>进程关停的并发裁决：关闭动作至多执行一次，所有调用方等待同一个任务，停止可先于开始，超时有日志且不吞掉原始异常。</summary>
/// <remarks>
/// <para>
/// 状态只有三个：Idle → Begun → Stopped。<see cref="TryBegin"/> 用比较交换从 Idle 迁移到 Begun，
/// <see cref="TryStopAsync"/> 从任意状态迁移到 Stopped 并返回同一个任务；因此停止先于开始时，迟到的 <see cref="TryBegin"/> 一律返回 false。
/// 状态用 <see cref="Interlocked"/> 维护，任务缓存与动作启动在 <c>_gate</c> 内完成，读状态不加锁。
/// </para>
/// <para>
/// 关闭动作接收一个在超时后取消的令牌，调用方应把令牌继续传给宿主的停止方法；超过 <c>timeout</c> 时
/// <see cref="TryStopAsync"/> 记录警告并抛出 <see cref="TimeoutException"/>，而关闭动作自身抛出的异常原样传给等待方，
/// 不会被超时或取消掩盖。超时后动作若才失败，异常经日志记录而不是静默丢弃。
/// </para>
/// <para>
/// 本类型不创建线程、不持有宿主；它只负责“开始一次、关停一次、可等待”的时序，宿主与窗口的释放由调用方在关闭动作里完成。
/// </para>
/// </remarks>
public sealed class ExitArbiter
{
    private const int Idle = 0;
    private const int Begun = 1;
    private const int Stopped = 2;

    private readonly Func<CancellationToken, Task> _stopAction;
    private readonly TimeSpan _timeout;
    private readonly ILogger<ExitArbiter> _logger;
    private readonly Lock _gate = new();

    private int _state;
    private Task? _stopTask;

    /// <summary>创建一个关停裁决器。</summary>
    /// <param name="stopAction">关闭动作，只执行一次；接收超时后取消的令牌，异常原样传播给等待方。</param>
    /// <param name="timeout">等待关闭动作完成的上限，必须为正的有限时长。</param>
    /// <param name="logger">超时与超时后失败的日志出口。</param>
    /// <exception cref="ArgumentNullException"><paramref name="stopAction"/> 或 <paramref name="logger"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> 不为正的有限时长。</exception>
    public ExitArbiter(Func<CancellationToken, Task> stopAction, TimeSpan timeout, ILogger<ExitArbiter> logger)
    {
        ArgumentNullException.ThrowIfNull(stopAction);
        ArgumentNullException.ThrowIfNull(logger);

        if (timeout <= TimeSpan.Zero || timeout == Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "关停超时必须为正的有限时长。");

        _stopAction = stopAction;
        _timeout = timeout;
        _logger = logger;
    }

    /// <summary>尝试开始一次启动；只有从未开始且从未关停时成功。</summary>
    /// <returns>首次调用返回 <see langword="true"/>；已经开始或已经关停时返回 <see langword="false"/>。</returns>
    /// <remarks>本方法无锁，与 <see cref="TryStopAsync"/> 并发调用时以比较交换的结果为准，不会出现既开始又关停的状态。</remarks>
    public bool TryBegin() => Interlocked.CompareExchange(ref _state, Begun, Idle) == Idle;

    /// <summary>是否已经进入关停。</summary>
    /// <remarks>只读快照，供调用方在异步等待之后判断期间是否已经退出；不改变状态。</remarks>
    public bool IsStopped => Volatile.Read(ref _state) == Stopped;

    /// <summary>执行或等待唯一一次关停；重复调用与并发调用返回同一个任务。</summary>
    /// <returns>关闭动作完成的任务；超时时为 <see cref="TimeoutException"/>，动作失败时为原始异常。</returns>
    /// <remarks>
    /// 停止先于开始时也执行关闭动作，从而让“启动尚未登记资源就退出”的路径仍有唯一的清理点；
    /// 动作只启动一次，之后的所有调用都返回同一个任务，等待方因此都能观察到相同的完成或失败。
    /// </remarks>
    public Task TryStopAsync()
    {
        Task task;

        lock (_gate)
        {
            Interlocked.Exchange(ref _state, Stopped);
            task = _stopTask ??= StopCoreAsync();
        }

        return task;
    }

    /// <summary>带超时执行关闭动作，并把超时转成有日志的 <see cref="TimeoutException"/>。</summary>
    /// <returns>关闭动作完成的任务。</returns>
    private async Task StopCoreAsync()
    {
        using var timeoutSource = new CancellationTokenSource(_timeout);
        Task stopTask = _stopAction(timeoutSource.Token);

        try
        {
            await stopTask.WaitAsync(_timeout).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("关停动作超时：在 {Timeout} 内未完成，停止等待。", _timeout);
            ObserveLateFailure(stopTask);
            throw;
        }
    }

    /// <summary>在超时之后继续观察关闭动作，避免它迟到的异常变成未观察任务异常。</summary>
    /// <param name="stopTask">已经超时但仍可能完成的关闭任务。</param>
    private void ObserveLateFailure(Task stopTask)
    {
        _ = stopTask.ContinueWith(
            completed => _logger.LogError(completed.Exception, "关停动作在超时后仍然失败。"),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }
}
