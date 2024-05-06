namespace ImageLyre.UI.Contracts.Busy;

/// <summary>
/// 忙状态服务。
/// 允许模块启动一个带类型、消息、进度和超时控制的忙指示器，
/// 并通过响应式流通知 UI 层进行可视化反馈。
/// </summary>
public interface IBusyManager
{
    /// <summary>
    /// 启动一个新的忙指示器。
    /// </summary>
    /// <param name="busyIndicator">忙指示器配置（含类型、消息、进度范围等）。</param>
    /// <param name="millisecondsTimeout">
    /// 超时毫秒数。若 &gt; 0，超过此时间未结束则触发 <see cref="BusyTimedOut"/> 并自动终止该任务；
    /// 若 ≤ 0，则永不超时。
    /// </param>
    /// <returns>忙指示器的唯一 ID（即 busyIndicator.Id）。</returns>
    /// <exception cref="ArgumentNullException">当 busyIndicator 为 null。</exception>
    Guid StartBusy(BusyIndicator busyIndicator, int millisecondsTimeout = -1);

    /// <summary>
    /// 报告指定忙任务的进度更新。
    /// </summary>
    /// <param name="busyIndicatorId">忙任务 ID。</param>
    /// <param name="busyRecord">进度记录（含当前进度值和消息）。</param>
    /// <exception cref="ArgumentException">ID 无效、任务已结束或任务已超时终止。</exception>
    void ReportProgress(Guid busyIndicatorId, BusyRecord busyRecord);

    /// <summary>
    /// 报告忙任务成功完成。
    /// </summary>
    /// <param name="busyIndicatorId">忙任务 ID。</param>
    /// <exception cref="ArgumentException">ID 无效或任务已超时终止。</exception>
    void ReportCompleted(Guid busyIndicatorId);

    /// <summary>
    /// 报告忙任务因异常失败。
    /// </summary>
    /// <param name="busyIndicatorId">忙任务 ID。</param>
    /// <exception cref="ArgumentException">ID 无效或任务已超时终止。</exception>
    void ReportFailed(Guid busyIndicatorId);

    #region 响应式流

    /// <summary>
    /// 当新的忙指示器启动时推送。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（仅推后续事件）；hot。
    /// </remarks>
    IObservable<BusyEventArgs<BusyIndicator>> BusyStarted { get; }

    /// <summary>
    /// 当忙任务进度更新时推送。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（仅推后续事件）；hot。
    /// </remarks>
    IObservable<BusyEventArgs<BusyRecord>> BusyProgressChanged { get; }

    /// <summary>
    /// 当忙任务成功完成时推送。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（仅推后续事件）；hot。
    /// </remarks>
    IObservable<BusyEventArgs<Guid>> BusyCompleted { get; }

    /// <summary>
    /// 当忙任务因异常失败时推送。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（仅推后续事件）；hot。
    /// </remarks>
    IObservable<BusyEventArgs<Guid>> BusyFailed { get; }

    /// <summary>
    /// 当忙任务超时时推送。
    /// 超时即视为任务自动结束：此后对该 ID 调用 <see cref="ReportProgress"/>、
    /// <see cref="ReportCompleted"/> 或 <see cref="ReportFailed"/> 必须抛
    /// <see cref="ArgumentException"/>。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（仅推后续事件）；hot。
    /// </remarks>
    IObservable<BusyEventArgs<Guid>> BusyTimedOut { get; }

    #endregion
}
