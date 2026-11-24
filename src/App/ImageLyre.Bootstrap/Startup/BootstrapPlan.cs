namespace ImageLyre.Bootstrap.Startup;

/// <summary>固定两层的嵌套启动计划：Plan → 组 → Step；构造时校验组名、Step 归属与 Id 唯一，<see cref="Flatten"/> 给出唯一执行序列。</summary>
/// <remarks>
/// <para>
/// 组间顺序由构造参数顺序决定，组内顺序由 <see cref="BootstrapGroup{TContext}.Steps"/> 决定；本类型不提供运行期注册、递归嵌套或全局 <c>Order</c>。
/// 组容量 5 至 9 项属于 Plan 校验而不是构造约束，由 <see cref="ValidateGroupCapacity"/> 在测试与增长压测中显式执行。
/// </para>
/// </remarks>
/// <typeparam name="TContext">本阶段共享的上下文类型，由组合根构造。</typeparam>
public sealed class BootstrapPlan<TContext>
{
    private const int MinStepsPerGroup = 5;
    private const int MaxStepsPerGroup = 9;

    private readonly IReadOnlyList<IBootstrapStep<TContext>> _flattened;

    /// <summary>按组间顺序与组内顺序固化一份 Plan，并校验组名、Step 归属与 Id 唯一性。</summary>
    /// <param name="groups">按执行顺序排列的组；构造时复制为快照。</param>
    /// <exception cref="ArgumentNullException"><paramref name="groups"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException">
    /// 组集合为空、组为 <see langword="null"/>、组名重复、Step 声明的 Group 与所属组不一致，或 Step Id 重复；异常消息包含冲突的组名或 Step Id。
    /// </exception>
    public BootstrapPlan(IEnumerable<BootstrapGroup<TContext>> groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var groupList = groups.ToArray();
        if (groupList.Length == 0)
        {
            throw new ArgumentException("Plan 至少需要一个组。", nameof(groups));
        }

        var groupNames = new HashSet<string>(StringComparer.Ordinal);
        var stepIds = new HashSet<string>(StringComparer.Ordinal);
        var flattened = new List<IBootstrapStep<TContext>>();
        foreach (var group in groupList)
        {
            if (group is null)
            {
                throw new ArgumentException("Plan 的组不能为 null。", nameof(groups));
            }

            if (!groupNames.Add(group.Name))
            {
                throw new ArgumentException($"Plan 内组名重复：{group.Name}。", nameof(groups));
            }

            foreach (var step in group.Steps)
            {
                if (!string.Equals(step.Group, group.Name, StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        $"Step {step.Id} 声明的 Group 为 {step.Group}，与所属组 {group.Name} 不一致。",
                        nameof(groups));
                }

                if (!stepIds.Add(step.Id))
                {
                    throw new ArgumentException($"Plan 内 Step Id 重复：{step.Id}。", nameof(groups));
                }

                flattened.Add(step);
            }
        }

        Groups = groupList;
        _flattened = flattened;
    }

    /// <summary>按组间顺序排列的组快照，构造后不变。</summary>
    public IReadOnlyList<BootstrapGroup<TContext>> Groups { get; }

    /// <summary>把嵌套 Plan 扁平化为唯一执行序列，供执行与顺序断言共用。</summary>
    /// <returns>按组间顺序与组内顺序排列的全部 Step；构造时已固化，重复调用返回同一份快照。</returns>
    public IReadOnlyList<IBootstrapStep<TContext>> Flatten() => _flattened;

    /// <summary>校验每个叶组是否落在 5 至 9 项的容量区间内。</summary>
    /// <remarks>
    /// 本方法只做校验，不调整顺序、不拆分组，也不在构造时自动执行；迁移期 Plan 允许少量步骤，验收与增长压测再显式调用。
    /// </remarks>
    /// <exception cref="InvalidOperationException">至少一个组的 Step 数量小于 5 或大于 9；消息列出全部越界组及其实际数量。</exception>
    public void ValidateGroupCapacity()
    {
        var violations = Groups
            .Where(group => group.Steps.Count is < MinStepsPerGroup or > MaxStepsPerGroup)
            .Select(group => $"组 {group.Name} 含 {group.Steps.Count} 个 Step")
            .ToArray();

        if (violations.Length > 0)
        {
            throw new InvalidOperationException(
                $"{string.Join("；", violations)}；嵌套 Plan 的叶组容量必须在 {MinStepsPerGroup} 至 {MaxStepsPerGroup} 项之间。");
        }
    }
}
