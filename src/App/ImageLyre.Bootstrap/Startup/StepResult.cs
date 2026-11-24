namespace ImageLyre.Bootstrap.Startup;

/// <summary>单步执行返回的结果：四态之一，附带错误与降级诊断。</summary>
/// <remarks>
/// <para>
/// <see cref="StepOutcome.Completed"/> 与 <see cref="StepOutcome.Aborted"/> 不携带错误；<see cref="StepOutcome.Failed"/> 必须带 <see cref="Error"/>，
/// 调用方据此把异常类型写入报告；<see cref="StepOutcome.Degraded"/> 用 <see cref="Diagnostics"/> 标明失败模块与原因，可同时带 <see cref="Error"/>。
/// </para>
/// </remarks>
/// <param name="Outcome">执行结局，决定后续步骤继续还是回滚。</param>
/// <param name="Error">硬失败或软失败对应的异常；正常完成与取消时为 <see langword="null"/>。</param>
/// <param name="Diagnostics">降级失败的结构化说明，按发生顺序排列；无降级时为 <see langword="null"/>。</param>
public sealed record StepResult(
    StepOutcome Outcome,
    Exception? Error = null,
    IReadOnlyList<StepDiagnostic>? Diagnostics = null);

/// <summary>降级结果的结构化失败说明：失败模块与原因，随报告与日志一起保留。</summary>
/// <param name="Module">失败的插件、模块或组件名，用于把降级归属到具体产物。</param>
/// <param name="Message">失败原因，写清发生了什么，而不是只写失败。</param>
public sealed record StepDiagnostic(string Module, string Message);
