namespace ImageLyre.Bootstrap.Startup;

/// <summary>Plan 的一个叶组：组名加一组显式有序的 Step，组内顺序就是执行顺序。</summary>
/// <remarks>
/// <para>
/// 组是回滚与报告的聚合单位；组容量 5 至 9 项由 <see cref="BootstrapPlan{TContext}.ValidateGroupCapacity"/> 校验，本类型不拒绝少量步骤，
/// 因为迁移期的 Plan 会先出现一到两个步骤，容量校验由测试与增长压测显式执行。
/// </para>
/// </remarks>
/// <typeparam name="TContext">本阶段共享的上下文类型，由组合根构造。</typeparam>
public sealed class BootstrapGroup<TContext>
{
    /// <summary>用组名与显式有序的步骤快照创建一个叶组。</summary>
    /// <param name="name">组名，在同一个 Plan 内唯一，且要与组内每个 Step 的 <see cref="IBootstrapStep{TContext}.Group"/> 相同。</param>
    /// <param name="steps">按执行顺序排列的步骤；构造时复制为只读快照，之后对原集合的修改不影响本组。</param>
    /// <exception cref="ArgumentException"><paramref name="name"/> 为 <see langword="null"/>、空串或空白。</exception>
    /// <exception cref="ArgumentNullException"><paramref name="steps"/> 为 <see langword="null"/>。</exception>
    public BootstrapGroup(string name, IEnumerable<IBootstrapStep<TContext>> steps)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(steps);

        Name = name;
        Steps = steps.ToArray();
    }

    /// <summary>组名，在同一个 Plan 内唯一，并作为报告聚合键。</summary>
    public string Name { get; }

    /// <summary>组内按执行顺序排列的步骤，构造时为只读快照。</summary>
    public IReadOnlyList<IBootstrapStep<TContext>> Steps { get; }
}
