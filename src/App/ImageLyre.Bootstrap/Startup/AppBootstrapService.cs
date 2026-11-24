using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Startup;

/// <summary>唯一执行 HostPlan 的 HostedService：宿主启动时交给共享执行器跑步骤并把结果写入 <see cref="StartupReport"/>。</summary>
/// <remarks>
/// <para>
/// 结果策略在 <see cref="BootstrapPlanRunner{TContext}"/> 里统一实现：<see cref="StepOutcome.Degraded"/> 只记录并继续，
/// <see cref="StepOutcome.Failed"/> 逆序回滚已完成的可回滚步骤后终止 Host 启动并把原始异常交给宿主，
/// <see cref="StepOutcome.Aborted"/> 停止后续步骤。本类不再自带第二套执行循环，只负责构造上下文与停止插件。
/// </para>
/// <para>
/// 关停只停止插件管理器，不反向执行启动步骤；重复调用 <see cref="StopAsync"/> 交由 cangming 的停止语义与宿主生命周期处理。
/// </para>
/// </remarks>
public sealed class AppBootstrapService : IHostedService
{
    private readonly BootstrapPlanRunner<HostBootstrapContext> _runner;
    private readonly IHost _host;
    private readonly IServiceProvider _services;
    private readonly IPluginManager _pluginManager;
    private readonly ILogger<AppBootstrapService> _logger;

    /// <summary>创建 HostPlan 执行服务。</summary>
    /// <param name="plan">Host 阶段 Plan，由组合根注册为单例。</param>
    /// <param name="host">本次启动的宿主，用于填充 <see cref="HostBootstrapContext"/>。</param>
    /// <param name="services">宿主服务容器。</param>
    /// <param name="pluginManager">本次启动的插件管理器，关停时由本服务停止。</param>
    /// <param name="logger">宿主日志器，传给每个步骤并作为执行器日志出口。</param>
    /// <param name="report">启动报告单例，执行器逐步骤追加记录。</param>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null"/>。</exception>
    public AppBootstrapService(
        BootstrapPlan<HostBootstrapContext> plan,
        IHost host,
        IServiceProvider services,
        IPluginManager pluginManager,
        ILogger<AppBootstrapService> logger,
        StartupReport report)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(report);

        _runner = new BootstrapPlanRunner<HostBootstrapContext>(plan, report, logger);
        _host = host;
        _services = services;
        _pluginManager = pluginManager;
        _logger = logger;
    }

    /// <summary>按扁平顺序执行 HostPlan，并在每一步结束后写入报告。</summary>
    /// <param name="cancellationToken">宿主启动取消令牌，步骤据此停止后续动作。</param>
    /// <returns>Plan 执行完成的任务。</returns>
    /// <exception cref="OperationCanceledException">步骤在取消后中止，宿主启动被取消。</exception>
    /// <exception cref="InvalidOperationException">步骤返回 <see cref="StepOutcome.Failed"/>，宿主启动失败。</exception>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var context = new HostBootstrapContext(_host, _services, _pluginManager, _logger);
        return _runner.RunAsync(context, cancellationToken);
    }

    /// <summary>停止本次启动的插件管理器，由宿主关停流程调用。</summary>
    /// <param name="cancellationToken">关停取消令牌。</param>
    /// <returns>插件停止完成的任务。</returns>
    public Task StopAsync(CancellationToken cancellationToken) =>
        _pluginManager.StopAsync().WaitAsync(cancellationToken);
}
