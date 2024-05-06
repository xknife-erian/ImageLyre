using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Services;

/// <summary>把 <see cref="IPluginManager" /> 适配成 <see cref="IHostedService" />：Host 启停即插件启停。</summary>
/// <remarks>
///     <para>
///         启动顺序固定：先通知插件系统容器已构建（实现 <c>ISupportUsingIoC</c> 的插件靠这一步解析服务），
///         再由管理器逐个注入与启动。此阶段界面尚未启动，任何插件都不得创建控件。
///     </para>
///     <para>停止阶段的异常只记录不向上抛：插件没停干净不应挡住进程退出。</para>
/// </remarks>
public sealed partial class PluginManagerHostedService : IHostedService
{
    private static readonly ILogger s_logger = LogHub.For<PluginManagerHostedService>();

    private readonly IPluginManager _pluginManager;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>创建托管服务。</summary>
    /// <param name="pluginManager">由 <c>PicoPluginSystem.Launch</c> 登记进容器的插件管理器。</param>
    /// <param name="serviceProvider">已构建的容器。</param>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null" /> 时抛出。</exception>
    public PluginManagerHostedService(IPluginManager pluginManager, IServiceProvider serviceProvider)
    {
        _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        PicoPluginSystem.NotifyServiceProviderBuilt(_serviceProvider);

        await _pluginManager.StartAsync().ConfigureAwait(false);

        LogPluginSystemStarted(s_logger, _pluginManager.Plugins.Count);
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _pluginManager.StopAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogPluginSystemStopFailed(s_logger, exception);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "插件系统已启动，插件数 {Count}")]
    private static partial void LogPluginSystemStarted(ILogger logger, int count);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "停止插件系统失败")]
    private static partial void LogPluginSystemStopFailed(ILogger logger, Exception exception);
}
