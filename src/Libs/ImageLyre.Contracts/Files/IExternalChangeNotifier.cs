
namespace ImageLyre.Contracts.Files;

/// <summary>
/// 监视文件系统外部变更，通知文档或 Shell 进行重载/冲突处理。
/// 
/// ⚠️ 设计原则：
/// - 跨平台抽象：屏蔽 Windows FileSystemWatcher / macOS FSEvents / Linux inotify 差异。
/// - 批量防抖：文件系统事件常成批到达，实现应提供合理的防抖/合并策略。
/// - 与 IDocumentDirtyTracking 协作：外部变更可能触发"文件已被修改，是否重新加载？"提示。
/// </summary>
public interface IExternalChangeNotifier
{
    /// <summary>
    /// 开始监视指定路径（文件或目录）。
    /// 若同一 watcherId 已被使用，则替换原有监视。
    /// </summary>
    /// <param name="path">要监视的绝对路径。</param>
    /// <param name="watcherId">调用方指定的监视器标识，用于后续停止或区分事件来源。</param>
    /// <param name="includeSubdirectories">若为目录，是否递归监视子目录。</param>
    void Watch(string path, string watcherId, bool includeSubdirectories = false);

    /// <summary>
    /// 停止指定 watcherId 的监视。
    /// </summary>
    /// <param name="watcherId">先前传入的监视器标识。</param>
    void Unwatch(string watcherId);

    /// <summary>
    /// 当受监视资源发生变更时推送（文件修改、删除、重命名）。
    /// 实现应合并短时间内对同一文件的重复事件，避免抖动。
    /// </summary>
    /// <remarks>
    /// 触发线程：系统线程（文件系统 watcher 回调），订阅者须自行调度到 UI 线程；回放：无（仅推后续事件）；hot。
    /// </remarks>
    IObservable<FileChangeEvent> FileChanged { get; }

    /// <summary>
    /// 暂停所有监视（如批量保存期间避免自触发）。
    /// 可嵌套调用：PauseAll × 2 需要 ResumeAll × 2 才恢复。
    /// </summary>
    void PauseAll();

    /// <summary>
    /// 恢复所有监视。
    /// </summary>
    void ResumeAll();

    /// <summary>
    /// 当前是否处于暂停状态。
    /// </summary>
    bool IsPaused { get; }

    /// <summary>
    /// 所有活跃的 watcherId 列表。
    /// </summary>
    IReadOnlyList<string> ActiveWatcherIds { get; }
}

/// <summary>
/// 文件变更事件。
/// </summary>
public record FileChangeEvent
{
    /// <summary>
    /// 调用方注册时传入的 watcherId。
    /// </summary>
    public required string WatcherId { get; init; }

    /// <summary>
    /// 发生变更的完整路径。
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>
    /// 变更类型。
    /// </summary>
    public required FileChangeType ChangeType { get; init; }

    /// <summary>
    /// 事件时间戳。
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 重命名场景下的旧路径（仅 <see cref="FileChangeType.Renamed"/> 时有效）。
    /// </summary>
    public string? OldFullPath { get; init; }
}

/// <summary>
/// 文件变更类型。
/// </summary>
public enum FileChangeType
{
    /// <summary>文件内容被修改。</summary>
    Modified,

    /// <summary>文件或目录被删除。</summary>
    Deleted,

    /// <summary>文件或目录被重命名。</summary>
    Renamed,

    /// <summary>新文件或目录被创建。</summary>
    Created
}
