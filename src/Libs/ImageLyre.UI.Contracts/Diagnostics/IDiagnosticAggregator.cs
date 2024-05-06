using System.Reactive;

namespace ImageLyre.UI.Contracts.Diagnostics;

/// <summary>
/// 诊断严重度（对标 LSP DiagnosticSeverity）。
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>错误：程序无法正常工作或明显违背约束。</summary>
    Error,

    /// <summary>警告：可能存在问题的可疑代码，不阻断运行。</summary>
    Warning,

    /// <summary>信息：提示性消息（如接口变更通知）。</summary>
    Information,

    /// <summary>提示：更轻微的建议（如代码风格建议）。</summary>
    Hint,
}

/// <summary>
/// 诊断位置（1-based 行列；全部可空 = 整文档级诊断）。
/// </summary>
/// <param name="StartLine">起始行（1-based）；null 表示整文档级诊断。</param>
/// <param name="StartColumn">起始列（1-based）。</param>
/// <param name="EndLine">结束行（1-based，含）。</param>
/// <param name="EndColumn">结束列（1-based，含）。</param>
public record DiagnosticPosition(int? StartLine, int? StartColumn, int? EndLine, int? EndColumn);

/// <summary>
/// 单条诊断条目（结构化；与 <see cref="IOutputChannel"/> 的自由文本流严格区分）。
/// </summary>
public record DiagnosticEntry
{
    /// <summary>诊断条目唯一标识（owner 内唯一）。</summary>
    public required string Id { get; init; }

    /// <summary>严重度。</summary>
    public required DiagnosticSeverity Severity { get; init; }

    /// <summary>人类可读的诊断消息。</summary>
    public required string Message { get; init; }

    /// <summary>所属文档 ContentId；null 表示应用级诊断。</summary>
    public string? ContentId { get; init; }

    /// <summary>诊断代码（如编译错误号 / 规则 ID），可关联文档或帮助页。</summary>
    public string? Code { get; init; }

    /// <summary>诊断来源（如编译器名称、校验器名称）。</summary>
    public string? Source { get; init; }

    /// <summary>诊断位置；null 表示整文档级诊断。</summary>
    public DiagnosticPosition? Position { get; init; }

    /// <summary>附加标签（对标 LSP DiagnosticTag，如 deprecated / unnecessary）。</summary>
    public string[]? Tags { get; init; }
}

/// <summary>
/// 诊断聚合器：按 owner 覆盖式提交诊断条目集（owner = 产生诊断的子系统，如编译服务/校验器）。
/// 供 Shell 的问题面板（Problems）消费。与 <c>ImageLyre.Contracts.Diagnostics.IDiagnosticService</c>（日志与遥测门控）职责分离：
/// 本接口只承载"结构化的、可定位到文档的"诊断条目，不含日志流与性能快照。
///
/// ⚠️ 设计原则：
/// - 覆盖式提交：每次 <see cref="Replace"/> 都是 owner 当前全部诊断的完整快照，天然幂等，无增量/合并语义。
/// - 所有方法线程安全，可在任意线程调用（实现需调度到 UI 线程）。
/// </summary>
public interface IDiagnosticAggregator
{
    /// <summary>覆盖式提交 owner 的全部当前诊断；entries 为空 = 清除该 owner 全部诊断。</summary>
    /// <param name="ownerId">诊断来源子系统标识（如编译服务/校验器）。</param>
    /// <param name="entries">该 owner 的最新完整诊断集。</param>
    /// <exception cref="ArgumentException">ownerId 为 null 或空。</exception>
    void Replace(string ownerId, IReadOnlyList<DiagnosticEntry> entries);

    /// <summary>清除指定 owner 的诊断。</summary>
    /// <param name="ownerId">诊断来源子系统标识。</param>
    void Clear(string ownerId);

    /// <summary>当前全部诊断条目（只读快照，跨 owner 合并）。</summary>
    IReadOnlyList<DiagnosticEntry> GetAll();

    /// <summary>指定文档的诊断条目（只读快照）。</summary>
    /// <param name="contentId">文档 ContentId。</param>
    IReadOnlyList<DiagnosticEntry> GetForDocument(string contentId);

    /// <summary>任意 owner 诊断集变更时推送（Set/Clear 均触发）。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：无（配合快照查询 <see cref="GetAll"/> / <see cref="GetForDocument"/> 使用，订阅后仅推后续变更）；hot。</remarks>
    IObservable<Unit> DiagnosticsChanged { get; }
}
