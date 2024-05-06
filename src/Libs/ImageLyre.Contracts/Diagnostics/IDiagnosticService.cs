
namespace ImageLyre.Contracts.Diagnostics;

/// <summary>
/// 诊断日志与遥测门控；性能快照见 <see cref="IPerformanceMonitor"/>；
/// 结构化诊断条目（问题面板）由 Shell 层契约承载——三者职责分离。
/// 支持运行时诊断信息的面板展示（如开发工具或技术支持）。
/// 
/// ⚠️ 设计原则：
/// - 与 ILogger / Microsoft.Extensions.Logging 互补：此接口是 Shell 层对诊断面板的抽象，
///   负责收集、聚合和展示，而非底层日志写入。
/// - 权限隔离：普通用户可能只看到摘要，管理员/开发者看到完整日志。
/// - 隐私合规：遥测数据应匿名化，并提供关闭开关。
/// </summary>
public interface IDiagnosticService
{
    /// <summary>
    /// 记录一条诊断日志（由 Shell 或模块主动上报）。
    /// 级别低于 <see cref="LogLevel.Warning"/> 的日志通常不弹通知，仅归档。
    /// </summary>
    void Log(string source, string message, LogLevel level = LogLevel.Information, Exception? exception = null, Dictionary<string, object?>? context = null);

    /// <summary>
    /// 当前所有日志条目（只读快照，按时间倒序）。
    /// </summary>
    IReadOnlyList<DiagnosticLogEntry> Logs { get; }

    /// <summary>
    /// 按级别过滤的日志查询。
    /// </summary>
    /// <param name="minLevel">最低日志级别。</param>
    /// <param name="sourceFilter">可选来源过滤（前缀匹配或精确匹配）。</param>
    /// <param name="maxCount">最大返回条数。</param>
    IReadOnlyList<DiagnosticLogEntry> QueryLogs(LogLevel minLevel, string? sourceFilter = null, int maxCount = 1000);

    /// <summary>
    /// 当新日志写入时推送（可配置最低推送级别）。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="Logs"/> 使用，订阅后仅推后续追加）；hot。
    /// </remarks>
    IObservable<DiagnosticLogEntry> LogAppended { get; }

    /// <summary>
    /// 清除所有已归档日志（通常用于释放内存或隐私清理）。
    /// </summary>
    void ClearLogs();

    /// <summary>
    /// 导出日志为 JSON / CSV 等格式（用于技术支持）。
    /// </summary>
    /// <param name="format">导出格式标识。</param>
    /// <returns>导出内容字符串。</returns>
    string ExportLogs(string format = "json");

    /// <summary>
    /// 是否启用遥测数据收集（可由用户设置关闭）。
    /// </summary>
    bool IsTelemetryEnabled { get; set; }

    /// <summary>
    /// 打开诊断面板/开发者工具（由 Shell 实现决定具体形式）。
    /// </summary>
    void OpenDiagnosticPanel();
}

/// <summary>
/// 日志级别。
/// </summary>
public enum LogLevel
{
    /// <summary>详细诊断（最 verbose）。</summary>
    Verbose,

    /// <summary>调试信息。</summary>
    Debug,

    /// <summary>普通信息。</summary>
    Information,

    /// <summary>警告（非致命，但需关注）。</summary>
    Warning,

    /// <summary>错误（操作失败）。</summary>
    Error,

    /// <summary>致命错误（应用可能无法继续）。</summary>
    Fatal
}

/// <summary>
/// 诊断日志条目。
/// </summary>
public record DiagnosticLogEntry
{
    /// <summary>日志 ID（GUID）。</summary>
    public required string LogId { get; init; }

    /// <summary>时间戳（UTC）。</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>日志来源（如 "Cangming.Shell.Documents", "Orbit.MotionBridge"）。</summary>
    public required string Source { get; init; }

    /// <summary>消息文本。</summary>
    public required string Message { get; init; }

    /// <summary>日志级别。</summary>
    public required LogLevel Level { get; init; }

    /// <summary>关联异常（若适用）。</summary>
    public Exception? Exception { get; init; }

    /// <summary>附加上下文数据（如文档 ID、用户操作、请求 ID 等）。</summary>
    public Dictionary<string, object?>? Context { get; init; }

    /// <summary>线程 ID。</summary>
    public int ThreadId { get; init; }
}

