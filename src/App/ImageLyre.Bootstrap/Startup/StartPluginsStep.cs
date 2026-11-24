using System.Reflection;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Startup;

/// <summary>插件启动的复合步骤：通知容器已构建、启动全部插件，并把 Failed 插件汇总为 Degraded 而不是异常。</summary>
/// <remarks>
/// <para>
/// 内部顺序是 cangming 的固定契约：<c>NotifyServiceProviderBuilt</c> 在 <c>StartAsync</c> 之前，插件启动完成后再统计能力插件；
/// 这些动作不拆成可重排的独立步骤，Fail 的插件只记录诊断，不阻断后续 Host 步骤，也不触发已完成步骤的回滚。
/// </para>
/// <para>
/// 步骤本身不持有资源，<see cref="CanRollback"/> 为 <see langword="false"/>；正常关停由宿主的 <c>IHost.StopAsync</c> 驱动
/// <see cref="AppBootstrapService.StopAsync"/> 停止插件管理器，而不是走启动链的机械逆序。
/// </para>
/// </remarks>
public sealed class StartPluginsStep : IBootstrapStep<HostBootstrapContext>
{
    /// <summary>步骤标识，格式为 <c>host.plugins.start</c>，发布后不复用。</summary>
    public string Id => "host.plugins.start";

    /// <summary>所属组为 HostPlan 固定的 <c>plugins</c> 组。</summary>
    public string Group => "plugins";

    /// <summary>插件启动属于 Host 阶段，可在任意线程执行，不构造控件。</summary>
    public StepPhase Phase => StepPhase.Host;

    /// <summary>插件由宿主关停负责停止，本步骤不提供失败回滚。</summary>
    public bool CanRollback => false;

    /// <summary>通知插件系统容器已构建，启动全部插件，并把失败插件汇总为降级诊断。</summary>
    /// <param name="context">Host 阶段上下文，提供容器、管理器与日志器。</param>
    /// <param name="cancellationToken">取消令牌，只取消插件启动的等待。</param>
    /// <returns>全部插件启动完成时为 <see cref="StepOutcome.Completed"/>，存在 Failed 插件时为 <see cref="StepOutcome.Degraded"/>。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="OperationCanceledException">插件启动等待被取消。</exception>
    public async ValueTask<StepResult> ExecuteAsync(
        HostBootstrapContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        PicoPluginSystem.NotifyServiceProviderBuilt(context.Services);
        await context.PluginManager.StartAsync().WaitAsync(cancellationToken);

        StepResult result = CreateResult(
            context.PluginManager.Plugins.Select(plugin => (GetPluginId(context.PluginManager, plugin), plugin.State)));

        int capabilityCount = context.PluginManager.Plugins.Count(
            plugin => GetPluginId(context.PluginManager, plugin).StartsWith("lyre.capability.", StringComparison.Ordinal));
        context.Logger.LogInformation("能力插件 {Count} 个", capabilityCount);

        return result;
    }

    /// <summary>本步骤不可回滚，调用方在 <see cref="CanRollback"/> 为 <see langword="false"/> 时应跳过本方法。</summary>
    /// <param name="context">Host 阶段上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>已完成的任务。</returns>
    public ValueTask RollbackAsync(HostBootstrapContext context, CancellationToken cancellationToken) =>
        ValueTask.CompletedTask;

    /// <summary>把插件状态映射为步骤结果：只有 <see cref="PluginState.Failed"/> 产生降级诊断，其余状态视为正常。</summary>
    /// <param name="plugins">插件 Id 与运行期状态的序列，调用方按扫描顺序提供。</param>
    /// <returns>无失败插件时为 <see cref="StepOutcome.Completed"/>，存在失败插件时为带诊断的 <see cref="StepOutcome.Degraded"/>。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="plugins"/> 为 <see langword="null"/>。</exception>
    internal static StepResult CreateResult(
        IEnumerable<(string Id, PluginState State)> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        StepDiagnostic[] diagnostics = plugins
            .Where(plugin => plugin.State == PluginState.Failed)
            .Select(plugin => new StepDiagnostic(plugin.Id, "插件启动状态为 Failed，已降级继续。"))
            .ToArray();

        return diagnostics.Length == 0
            ? new StepResult(StepOutcome.Completed)
            : new StepResult(StepOutcome.Degraded, Diagnostics: diagnostics);
    }

    /// <summary>优先读取构造失败占位条目保留的扫描期 Id，再回退管理器按类型查找。</summary>
    /// <param name="manager">插件管理器。</param>
    /// <param name="plugin">插件实例。</param>
    /// <returns>插件 Id，用于失败诊断与能力插件计数。</returns>
    /// <remarks>
    /// cangming 的 <c>BrokenPlugin</c> 把扫描期 <c>[Plugin]</c> 元数据保存在公开的 <c>Info</c> 属性里，
    /// 但按占位类型重查会得到占位类型自身的 Id；这里先读 <c>Info</c>，让失败的故障样本仍按真实前缀计数与归属。
    /// </remarks>
    private static string GetPluginId(IPluginManager manager, IPicoPlugin plugin)
    {
        PropertyInfo? infoProperty = plugin.GetType().GetProperty(
            "Info",
            BindingFlags.Instance | BindingFlags.Public);

        return infoProperty?.GetValue(plugin) is PluginInfo info
                   ? info.Id
                   : manager.GetPluginInfo(plugin.GetType()).Id;
    }
}
