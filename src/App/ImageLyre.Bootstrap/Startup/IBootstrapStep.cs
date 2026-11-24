namespace ImageLyre.Bootstrap.Startup;

/// <summary>一次启动中可独立命名、独立成功或失败的工作单元；Plan 只负责排序与分组，不实现步骤内部逻辑。</summary>
/// <remarks>
/// <para>
/// <see cref="Id"/> 在全进程唯一，格式为 <c>{plan}.{group}.{step}</c>，发布后不复用，因为失败报告与回滚轨迹以它为归属键；
/// <see cref="Group"/> 必须与容纳它的 <c>BootstrapGroup.Name</c> 相同，<c>BootstrapPlan</c> 构造时校验这一点。
/// </para>
/// <para>
/// 实现方不得用异常表达可预期的业务失败：单步失败返回 <see cref="StepOutcome.Failed"/>，插件级软失败返回 <see cref="StepOutcome.Degraded"/> 并带上诊断；
/// 未捕获的异常由后续 Pipeline 记为 <see cref="StepOutcome.Failed"/> 并写入报告。实现无需保证线程安全，Plan 按顺序调用 <see cref="ExecuteAsync"/>。
/// </para>
/// </remarks>
public interface IBootstrapStep<in TContext>
{
    /// <summary>稳定且唯一的步骤标识，格式为 <c>{plan}.{group}.{step}</c>，发布后不复用。</summary>
    string Id { get; }

    /// <summary>所属组名，必须与容纳它的 <c>BootstrapGroup.Name</c> 一致。</summary>
    string Group { get; }

    /// <summary>执行环境：<see cref="StepPhase.Host"/> 在任意线程，<see cref="StepPhase.Presentation"/> 在 Avalonia UI 线程。</summary>
    StepPhase Phase { get; }

    /// <summary>是否提供资源回滚；为 <see langword="false"/> 时调用方跳过 <see cref="RollbackAsync"/> 并在报告中保留未回滚标记。</summary>
    bool CanRollback { get; }

    /// <summary>执行本步骤并返回结果；可预期的失败通过 <see cref="StepResult"/> 表达，不抛异常。</summary>
    /// <param name="context">本阶段共享的上下文，由组合根构造并原样传递。</param>
    /// <param name="cancellationToken">取消令牌；取消后实现应尽快返回 <see cref="StepOutcome.Aborted"/>。</param>
    /// <returns>本步骤的结果；软失败用 <see cref="StepOutcome.Degraded"/>，不中断后续步骤。</returns>
    ValueTask<StepResult> ExecuteAsync(TContext context, CancellationToken cancellationToken);

    /// <summary>按逆序释放本步骤已持有的资源，仅在 <see cref="CanRollback"/> 为 <see langword="true"/> 且执行完成后调用。</summary>
    /// <param name="context">与 <see cref="ExecuteAsync"/> 相同的上下文。</param>
    /// <param name="cancellationToken">取消令牌；实现应尽量在取消后停止继续分配资源。</param>
    /// <returns>回滚完成的任务。</returns>
    ValueTask RollbackAsync(TContext context, CancellationToken cancellationToken);
}
