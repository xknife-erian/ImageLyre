namespace ImageLyre.Bootstrap.Startup;

/// <summary>Host 阶段的固定 Plan：按 infrastructure、plugins、host-services 三组组织启动步骤。</summary>
/// <remarks>
/// <para>
/// 组间顺序由 <see cref="Create"/> 的构造参数写死，组内顺序由组自身的 Step 列表决定；M1 只有 <c>plugins</c> 组有真实步骤，
/// 另外两组保留为空，等基础设施与宿主服务出现真实启动项时再填入，不用空操作步骤凑容量。
/// </para>
/// <para>
/// 空组仍参与报告分组与顺序断言，这样新增步骤只落到对应组，不需要改组间顺序；容量校验由 Plan 的
/// <c>ValidateGroupCapacity</c> 在增长压测中显式执行，不在本工厂里强制。
/// </para>
/// </remarks>
public static class HostPlan
{
    /// <summary>创建 Host 阶段的固定 Plan。</summary>
    /// <returns>每次调用返回一份新的、组序固定的 Plan；其中的插件步骤无状态。</returns>
    public static BootstrapPlan<HostBootstrapContext> Create() =>
        new(
        [
            new BootstrapGroup<HostBootstrapContext>("infrastructure", []),
            new BootstrapGroup<HostBootstrapContext>("plugins", [new StartPluginsStep()]),
            new BootstrapGroup<HostBootstrapContext>("host-services", []),
        ]);
}
