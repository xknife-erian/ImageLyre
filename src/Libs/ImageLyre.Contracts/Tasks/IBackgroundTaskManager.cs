
namespace ImageLyre.Contracts.Tasks;

/// <summary>
/// 后台任务队列管理器。支持任务排队、优先级、取消令牌和进度回传。
/// 与 Shell 层的忙状态服务配合使用：本接口负责执行与调度，忙状态服务负责可视化。
/// 
/// ⚠️ 设计原则：
/// - 避免阻塞 UI 线程：长时间计算、批量导出、网络同步等应通过此队列执行。
/// - 支持优先级抢占：Critical 任务可插队到队列头部。
/// - 持久化可选：实现可决定是否将任务队列持久化到磁盘（用于崩溃恢复）。
/// </summary>
public interface IBackgroundTaskManager
{
    /// <summary>
    /// 排队一个后台任务。若任务可立即执行（有空闲槽位），则立即开始。
    /// </summary>
    /// <param name="descriptor">任务描述（名称、工作体、优先级等）。</param>
    /// <returns>任务唯一 ID。</returns>
    Guid Enqueue(BackgroundTaskDescriptor descriptor);

    /// <summary>
    /// 请求取消指定任务（通过 CancellationToken）。
    /// 若任务已完成或未运行，则静默返回。
    /// </summary>
    /// <param name="taskId">任务 ID。</param>
    void Cancel(Guid taskId);

    /// <summary>
    /// 获取任务当前状态。
    /// </summary>
    /// <param name="taskId">任务 ID。</param>
    /// <returns>任务状态；若任务不存在返回 null。</returns>
    BackgroundTaskStatus? GetStatus(Guid taskId);

    /// <summary>
    /// 所有正在运行和排队的任务（按优先级和排队时间排序）。
    /// </summary>
    IReadOnlyList<BackgroundTaskStatus> ActiveTasks { get; }

    /// <summary>
    /// 当任务状态变更（排队→运行→完成/取消/失败）时推送。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="ActiveTasks"/> 与 <see cref="GetStatus"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<BackgroundTaskStatus> TaskStatusChanged { get; }

    /// <summary>
    /// 同时允许运行的最大任务数（实现决定，通常等于 CPU 核心数或自定义线程池大小）。
    /// </summary>
    int MaxConcurrentTasks { get; }

    /// <summary>
    /// 当前正在运行的任务数。
    /// </summary>
    int RunningTaskCount { get; }
}

/// <summary>
/// 后台任务描述符。
/// </summary>
public record BackgroundTaskDescriptor
{
    /// <summary>
    /// 任务显示名称（用于状态栏和任务列表）。
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// 任务执行体。传入 <see cref="IBackgroundTaskContext"/> 提供进度、取消令牌和状态回传能力。
    /// </summary>
    public required Func<IBackgroundTaskContext, Task> Work { get; init; }

    /// <summary>
    /// 任务优先级。Critical 可插队到队列头部。
    /// </summary>
    public TaskPriority Priority { get; init; } = TaskPriority.Normal;

    /// <summary>
    /// 是否在状态栏显示进度指示器。
    /// </summary>
    public bool ShowInStatusBar { get; init; } = true;

    /// <summary>
    /// 是否在 Shell 层的忙状态服务中注册忙指示器。
    /// </summary>
    public bool ShowBusyIndicator { get; init; } = true;

    /// <summary>
    /// 可选：任务完成后是否显示成功通知（由宿主通知服务承载）。
    /// </summary>
    public bool NotifyOnCompletion { get; init; }

    /// <summary>
    /// 可选上下文数据（如关联文档 ID、导出路径等）。
    /// </summary>
    public object? Context { get; init; }
}

/// <summary>
/// 后台任务执行上下文。由 IBackgroundTaskManager 创建并注入到 Work 委托中。
/// </summary>
public interface IBackgroundTaskContext
{
    /// <summary>任务 ID。</summary>
    Guid TaskId { get; }

    /// <summary>取消令牌。工作体应定期检查并响应取消。</summary>
    CancellationToken CancellationToken { get; }

    /// <summary>报告进度（0.0 ~ 1.0）。</summary>
    IProgress<double> Progress { get; }

    /// <summary>报告状态消息（如"正在处理第 3 页。.."）。</summary>
    void ReportStatus(string message);

    /// <summary>设置任务结果（完成后调用）。</summary>
    void SetResult(object? result = null);

    /// <summary>设置任务失败原因。</summary>
    void SetException(Exception exception);
}

/// <summary>
/// 后台任务状态快照。
/// </summary>
public record BackgroundTaskStatus
{
    /// <summary>任务 ID。</summary>
    public required Guid TaskId { get; init; }

    /// <summary>显示名称。</summary>
    public required string DisplayName { get; init; }

    /// <summary>当前状态。</summary>
    public required TaskState State { get; init; }

    /// <summary>进度百分比（0.0 ~ 1.0），未完成时可能为 null。</summary>
    public double? ProgressPercent { get; init; }

    /// <summary>当前状态消息。</summary>
    public string? StatusMessage { get; init; }

    /// <summary>优先级。</summary>
    public TaskPriority Priority { get; init; }

    /// <summary>排队时间。</summary>
    public DateTimeOffset EnqueuedAt { get; init; }

    /// <summary>开始运行时间（若已运行）。</summary>
    public DateTimeOffset? StartedAt { get; init; }

    /// <summary>完成/失败/取消时间（若已结束）。</summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>失败时的异常信息（仅 State = Failed 时有效）。</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// 任务状态。
/// </summary>
public enum TaskState
{
    /// <summary>在队列中等待执行。</summary>
    Queued,

    /// <summary>正在运行。</summary>
    Running,

    /// <summary>成功完成。</summary>
    Completed,

    /// <summary>被取消。</summary>
    Canceled,

    /// <summary>执行失败。</summary>
    Failed
}

/// <summary>
/// 任务优先级。
/// </summary>
public enum TaskPriority
{
    /// <summary>低优先级，可延迟执行。</summary>
    Low,

    /// <summary>默认优先级。</summary>
    Normal,

    /// <summary>高优先级，尽快执行。</summary>
    High,

    /// <summary>关键任务，可插队到队列头部。</summary>
    Critical
}
