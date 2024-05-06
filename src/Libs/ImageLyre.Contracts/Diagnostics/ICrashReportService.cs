using ImageLyre.Contracts.Messaging;

namespace ImageLyre.Contracts.Diagnostics;

/// <summary>崩溃报告（"死后的现场"，区别于活着的异常上报 <see cref="IGlobalExceptionHandler"/>）。</summary>
public record CrashReport
{
    /// <summary>崩溃时间（UTC）。</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>异常类型与消息（多条异常链时含 Inner）。</summary>
    public required string ExceptionSummary { get; init; }

    /// <summary>异常堆栈（可含换行）。</summary>
    public string? StackTrace { get; init; }

    /// <summary>应用版本。</summary>
    public required string AppVersion { get; init; }

    /// <summary>操作系统与版本。</summary>
    public required string OsDescription { get; init; }

    /// <summary>可选附件（日志文件路径/内存转储路径）。</summary>
    public IReadOnlyList<string>? Attachments { get; init; }
}

/// <summary>
/// 崩溃转储捕获与上报能力：把进程级未处理异常（UnhandledException / UnobservedTaskException /
/// 实现方注册的原生崩溃 handler）捕获为 <see cref="CrashReport"/>，经用户同意后上传。
/// </summary>
/// <remarks>
/// <para><b>隐私红线</b>（实现方必须遵守）：</para>
/// <list type="bullet">
/// <item>上报必须联动 <see cref="IDiagnosticService.IsTelemetryEnabled" />，遥测关闭时不采集也不上传；</item>
/// <item>用户同意为上传前提，契约层通过 <see cref="PromptBeforeUpload" /> 表达默认策略，宿主可覆盖；</item>
/// <item>报告内容禁止包含凭据与密钥，采集时即过滤。</item>
/// </list>
/// <para>
/// 与 <see cref="IGlobalExceptionHandler" /> 分工：后者负责活着的异常上报（进程继续运行），
/// 本接口负责死后的现场（进程即将或已经崩溃）。
/// </para>
/// </remarks>
public interface ICrashReportService
{
    /// <summary>上传前是否必须经用户确认（默认 true，宿主可覆盖为受控自动上报）。</summary>
    bool PromptBeforeUpload { get; }

    /// <summary>收集当前崩溃报告（进程仍在运行时调用，用于诊断面板展示）。</summary>
    CrashReport CollectReport();

    /// <summary>上传崩溃报告；返回服务端报告 ID（失败抛异常，含原因）。</summary>
    /// <exception cref="InvalidOperationException">遥测被禁用时调用（隐私红线）。</exception>
    Task<string> UploadAsync(CrashReport report, CancellationToken cancellationToken = default);

    /// <summary>启动崩溃捕获。</summary>
    /// <remarks>宿主应在启动早期调用；重复调用幂等，不会重复注册捕获器。</remarks>
    void StartCapture();
}
