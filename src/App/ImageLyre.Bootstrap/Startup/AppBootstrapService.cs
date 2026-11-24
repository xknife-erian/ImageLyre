using System.Diagnostics;
using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Startup;

/// <summary>唯一执行 HostPlan 的 HostedService：宿主启动时按扁平顺序跑步骤并把结果写入 <see cref="StartupReport"/>。</summary>
/// <remarks>
/// <para>
/// 失败策略与设计一致：<see cref="StepOutcome.Degraded"/> 只记录并继续，<see cref="StepOutcome.Failed"/> 终止 Host 启动并把原始异常交给宿主，
/// <see cref="StepOutcome.Aborted"/> 停止后续步骤；回滚与失败界面属于后续任务，本类不提前实现。
/// </para>
/// <para>
/// 关停只停止插件管理器，不反向执行启动步骤；重复调用 <see cref="StopAsync"/> 交由 cangming 的停止语义与宿主生命周期处理。
/// </para>
/// </remarks>
public sealed class AppBootstrapService : IHostedService
{
    private readonly BootstrapPlan<HostBootstrapContext> _plan;
    private readonly IHost _host;
    private readonly IServiceProvider _services;
    private readonly IPluginManager _pluginManager;
    private readonly ILogger<AppBootstrapService> _logger;
    private readonly StartupReport _report;

    /// <summary>创建 HostPlan 执行服务。</summary>
    /// <param name="plan">Host 阶段 Plan，由组合根注册为单例。</param>
    /// <param name="host">本次启动的宿主，用于填充 <see cref="HostBootstrapContext"/>。</param>
    /// <param name="services">宿主服务容器。</param>
    /// <param name="pluginManager">本次启动的插件管理器，关停时由本服务停止。</param>
    /// <param name="logger">宿主日志器，传给每个步骤。</param>
    /// <param name="report">启动报告单例，步骤执行后追加记录。</param>
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

        _plan = plan;
        _host = host;
        _services = services;
        _pluginManager = pluginManager;
        _logger = logger;
        _report = report;
    }

    /// <summary>按扁平顺序执行 HostPlan，并在每一步结束后写入报告。</summary>
    /// <param name="cancellationToken">宿主启动取消令牌，步骤据此停止后续动作。</param>
    /// <returns>Plan 执行完成的任务。</returns>
    /// <exception cref="OperationCanceledException">步骤在取消后中止，宿主启动被取消。</exception>
    /// <exception cref="InvalidOperationException">步骤返回 <see cref="StepOutcome.Failed"/>，宿主启动失败。</exception>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var context = new HostBootstrapContext(_host, _services, _pluginManager, _logger);
        foreach (IBootstrapStep<HostBootstrapContext> step in _plan.Flatten())
        {
            long startedAt = Stopwatch.GetTimestamp();
            StepResult result;
            try
            {
                result = await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _report.Add(CreateRecord(step, StepOutcome.Aborted, error: null, Stopwatch.GetElapsedTime(startedAt)));
                throw;
            }

            _report.Add(CreateRecord(step, result.Outcome, result.Error, Stopwatch.GetElapsedTime(startedAt)));

            // ponytail: 回滚、失败界面与报告诊断归 T09/T10；本任务只保证 Failed 终止宿主启动、Degraded 继续。
            if (result.Outcome == StepOutcome.Failed)
            {
                throw new InvalidOperationException($"Host 启动步骤 {step.Id} 失败。", result.Error);
            }

            if (result.Outcome == StepOutcome.Aborted)
            {
                return;
            }
        }
    }

    /// <summary>停止本次启动的插件管理器，由宿主关停流程调用。</summary>
    /// <param name="cancellationToken">关停取消令牌。</param>
    /// <returns>插件停止完成的任务。</returns>
    public Task StopAsync(CancellationToken cancellationToken) =>
        _pluginManager.StopAsync().WaitAsync(cancellationToken);

    private static StepRecord CreateRecord(
        IBootstrapStep<HostBootstrapContext> step,
        StepOutcome outcome,
        Exception? error,
        TimeSpan elapsed) =>
        new(step.Id, step.Group, step.Phase, outcome, elapsed, error?.GetType().Name, RolledBack: false);
}
