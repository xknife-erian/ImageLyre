using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Startup;

/// <summary>Host 阶段与 Presentation 阶段共用的 Plan 执行器：按扁平顺序执行、逐 Step 写报告、失败逆序回滚已完成的可回滚步骤。</summary>
/// <remarks>
/// <para>
/// 结果策略与设计一致：<see cref="StepOutcome.Completed"/> 与 <see cref="StepOutcome.Degraded"/> 继续执行并进入回滚候选，
/// <see cref="StepOutcome.Failed"/> 记录后按完成逆序回滚持有资源的步骤并抛出带 Step Id 的异常，
/// <see cref="StepOutcome.Aborted"/> 记录后停止后续步骤但不视为失败；步骤直接抛出的非取消异常按 <see cref="StepOutcome.Failed"/>
/// 处理并回滚后包装为带 Step Id 的异常，取消异常在令牌已取消时记为中止并原样向上传播。
/// </para>
/// <para>
/// 报告是唯一的结果出口：每个步骤无论结局都写入一条 <see cref="StepRecord"/>，回滚成功的步骤用
/// <see cref="StartupReport.MarkRolledBack"/> 就地补标记，不新增重复记录；回滚失败只记日志，不覆盖最初的失败原因。
/// </para>
/// <para>
/// 线程：本类不切换线程、不加锁，按 Plan 顺序串行执行；调用方负责保证 Host 阶段在任意线程、Presentation 阶段在 UI 线程上运行。
/// </para>
/// </remarks>
/// <typeparam name="TContext">本阶段共享的上下文类型，由调用方构造并原样传给每个步骤。</typeparam>
public sealed class BootstrapPlanRunner<TContext>
{
    private readonly BootstrapPlan<TContext> _plan;
    private readonly StartupReport _report;
    private readonly ILogger _logger;

    /// <summary>创建一次 Plan 执行器。</summary>
    /// <param name="plan">要执行的嵌套 Plan，执行前不再变更。</param>
    /// <param name="report">启动报告，逐步骤追加记录并标记回滚。</param>
    /// <param name="logger">报告回滚失败与步骤结果的日志出口。</param>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null"/>。</exception>
    public BootstrapPlanRunner(BootstrapPlan<TContext> plan, StartupReport report, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(logger);

        _plan = plan;
        _report = report;
        _logger = logger;
    }

    /// <summary>按扁平顺序执行整个 Plan，失败时逆序回滚已完成的可回滚步骤后抛出。</summary>
    /// <param name="context">本阶段共享的上下文，原样传给每个步骤。</param>
    /// <param name="cancellationToken">取消令牌；取消后不再执行后续步骤。</param>
    /// <returns>Plan 执行完成的任务。</returns>
    /// <exception cref="OperationCanceledException">步骤在令牌取消后中止，或步骤抛出未包装的取消异常。</exception>
    /// <exception cref="InvalidOperationException">步骤返回 <see cref="StepOutcome.Failed"/> 或直接抛出非取消异常，消息带失败步骤的 Id。</exception>
    /// <remarks>回滚只覆盖失败步骤之前已经完成且声明 <see cref="IBootstrapStep{TContext}.CanRollback"/> 的步骤，失败步骤自身不参与。</remarks>
    public async Task RunAsync(TContext context, CancellationToken cancellationToken)
    {
        var completed = new List<IBootstrapStep<TContext>>();

        foreach (IBootstrapStep<TContext> step in _plan.Flatten())
        {
            long startedAt = Stopwatch.GetTimestamp();
            StepResult result;

            try
            {
                result = await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                TimeSpan abortedElapsed = Stopwatch.GetElapsedTime(startedAt);
                _report.Add(CreateRecord(
                    step,
                    StepOutcome.Aborted,
                    error: null,
                    abortedElapsed,
                    diagnostics: null));
                WriteStepLog(step, StepOutcome.Aborted, abortedElapsed, diagnostics: null, error: null);
                throw;
            }
            catch (Exception ex)
            {
                TimeSpan failedElapsed = Stopwatch.GetElapsedTime(startedAt);
                _report.Add(CreateRecord(
                    step,
                    StepOutcome.Failed,
                    ex,
                    failedElapsed,
                    diagnostics: null));
                WriteStepLog(step, StepOutcome.Failed, failedElapsed, diagnostics: null, error: ex);
                await RollbackAsync(completed, context, cancellationToken).ConfigureAwait(false);
                throw new InvalidOperationException($"启动步骤 {step.Id} 失败。", ex);
            }

            TimeSpan elapsed = Stopwatch.GetElapsedTime(startedAt);
            _report.Add(CreateRecord(
                step,
                result.Outcome,
                result.Error,
                elapsed,
                result.Diagnostics));
            WriteStepLog(
                step,
                result.Outcome,
                elapsed,
                result.Diagnostics,
                result.Outcome == StepOutcome.Failed ? result.Error : null);

            switch (result.Outcome)
            {
                case StepOutcome.Completed:
                case StepOutcome.Degraded:
                    completed.Add(step);
                    break;

                case StepOutcome.Failed:
                    await RollbackAsync(completed, context, cancellationToken).ConfigureAwait(false);
                    throw new InvalidOperationException($"启动步骤 {step.Id} 失败。", result.Error);

                case StepOutcome.Aborted:
                    return;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(result),
                        result.Outcome,
                        $"步骤 {step.Id} 返回了未定义的启动结果。");
            }
        }
    }

    /// <summary>按完成逆序回滚所有可回滚步骤，并逐条补记回滚标记。</summary>
    /// <param name="completed">已经完成的步骤，按执行顺序排列。</param>
    /// <param name="context">与执行时相同的上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>回滚完成的任务。</returns>
    /// <remarks>单个步骤回滚失败只记日志，继续回滚其余步骤，从而不让一个回滚异常掩盖最初的启动失败。</remarks>
    private async Task RollbackAsync(
        List<IBootstrapStep<TContext>> completed,
        TContext context,
        CancellationToken cancellationToken)
    {
        for (int index = completed.Count - 1; index >= 0; index--)
        {
            IBootstrapStep<TContext> step = completed[index];
            if (!step.CanRollback)
                continue;

            try
            {
                await step.RollbackAsync(context, cancellationToken).ConfigureAwait(false);
                _report.MarkRolledBack(step.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动步骤 {StepId} 回滚失败，继续回滚其余步骤。", step.Id);
            }
        }
    }

    /// <summary>把一步执行结果写进结构化日志：StepId、结局与耗时固定，降级诊断合并成一个字段。</summary>
    /// <param name="step">刚执行完的步骤。</param>
    /// <param name="outcome">执行结局。</param>
    /// <param name="elapsed">本步骤的耗时。</param>
    /// <param name="diagnostics">降级步骤的结构化诊断；无降级时为 <see langword="null"/>。</param>
    /// <param name="error">失败时随日志记录的异常；无异常时为 <see langword="null"/>。</param>
    private void WriteStepLog(
        IBootstrapStep<TContext> step,
        StepOutcome outcome,
        TimeSpan elapsed,
        IReadOnlyList<StepDiagnostic>? diagnostics,
        Exception? error)
    {
        string? diagnosticText = diagnostics is { Count: > 0 }
            ? string.Join("；", diagnostics.Select(item => $"{item.Module}：{item.Message}"))
            : null;

        LogLevel level = outcome switch
        {
            StepOutcome.Failed => LogLevel.Error,
            StepOutcome.Degraded => LogLevel.Warning,
            _ => LogLevel.Information,
        };

        const string template = "启动步骤 {StepId} 结束：结果 {Outcome}，耗时 {ElapsedMs:0.###} 毫秒";
        if (diagnosticText is null)
        {
            _logger.Log(level, error, template + "。", step.Id, outcome, elapsed.TotalMilliseconds);
        }
        else
        {
            _logger.Log(
                level,
                error,
                template + "，诊断 {Diagnostics}",
                step.Id,
                outcome,
                elapsed.TotalMilliseconds,
                diagnosticText);
        }
    }

    /// <summary>把一次步骤执行映射为报告记录，耗时、异常类型与降级诊断由执行器填写。</summary>
    /// <param name="step">刚执行完的步骤。</param>
    /// <param name="outcome">执行结局。</param>
    /// <param name="error">失败或降级携带的异常；正常完成与中止时为 <see langword="null"/>。</param>
    /// <param name="elapsed">本步骤的耗时。</param>
    /// <param name="diagnostics">降级步骤的结构化诊断；无降级时为 <see langword="null"/>。</param>
    /// <returns>尚未标记回滚的步骤记录。</returns>
    private static StepRecord CreateRecord(
        IBootstrapStep<TContext> step,
        StepOutcome outcome,
        Exception? error,
        TimeSpan elapsed,
        IReadOnlyList<StepDiagnostic>? diagnostics) =>
        new(step.Id, step.Group, step.Phase, outcome, elapsed, error?.GetType().Name, RolledBack: false, diagnostics);
}
