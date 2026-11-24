namespace ImageLyre.Bootstrap.Startup;

/// <summary>启动报告中一条不可变的步骤记录：结果、耗时、异常类型、回滚状态与降级诊断。</summary>
/// <param name="Id">步骤标识，与 <see cref="IBootstrapStep{TContext}.Id"/> 相同，作为失败归属键。</param>
/// <param name="Group">步骤所属组，报告按它聚合。</param>
/// <param name="Phase">步骤执行阶段，区分 Host 与 Presentation。</param>
/// <param name="Outcome">本步骤的执行结局。</param>
/// <param name="Elapsed">从开始执行到返回结果的耗时，由 Pipeline 计时填入。</param>
/// <param name="ErrorType">失败或降级时的异常类型名；无异常时为 <see langword="null"/>。</param>
/// <param name="RolledBack">是否已执行回滚；不可回滚或未执行时保持 <see langword="false"/>。</param>
/// <param name="Diagnostics">降级步骤的结构化诊断，按发生顺序排列；无降级时为 <see langword="null"/>。</param>
public sealed record StepRecord(
    string Id,
    string Group,
    StepPhase Phase,
    StepOutcome Outcome,
    TimeSpan Elapsed,
    string? ErrorType,
    bool RolledBack,
    IReadOnlyList<StepDiagnostic>? Diagnostics = null);
