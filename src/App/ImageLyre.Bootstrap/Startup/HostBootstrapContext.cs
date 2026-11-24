using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Startup;

/// <summary>Host 阶段共享的启动上下文：宿主、容器、插件管理器与日志，只读且在一次 HostPlan 执行内不变。</summary>
/// <remarks>
/// <para>
/// 实例由 <see cref="AppBootstrapService"/> 在 <c>StartAsync</c> 时构造，之后原样传给每个 <see cref="IBootstrapStep{TContext}"/>；
/// 步骤不得替换其中的引用，也不得释放宿主或容器，它们只负责读取服务并执行自己的启动工作。
/// </para>
/// <para>
/// <see cref="Host"/> 与 <see cref="Services"/> 指向同一个已启动的宿主；<see cref="PluginManager"/> 是容器内的单例，
/// 与 <see cref="Services"/> 解析到的实例相同。上下文不是线程安全的，HostPlan 在宿主启动线程上串行使用它。
/// </para>
/// </remarks>
public sealed class HostBootstrapContext
{
    /// <summary>创建一次 HostPlan 执行的上下文。</summary>
    /// <param name="host">已构建的宿主，提供生命周期与容器。</param>
    /// <param name="services">宿主服务容器，与 <paramref name="host"/> 的 <c>Services</c> 相同。</param>
    /// <param name="pluginManager">本次启动解析出的插件管理器。</param>
    /// <param name="logger">Host 阶段日志器，由宿主日志工厂创建。</param>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null"/>。</exception>
    public HostBootstrapContext(
        IHost host,
        IServiceProvider services,
        IPluginManager pluginManager,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(logger);

        Host = host;
        Services = services;
        PluginManager = pluginManager;
        Logger = logger;
    }

    /// <summary>本次启动的宿主，步骤不得停止或释放它。</summary>
    public IHost Host { get; }

    /// <summary>宿主服务容器，与 <see cref="Host"/> 的 <c>Services</c> 相同。</summary>
    public IServiceProvider Services { get; }

    /// <summary>本次启动的插件管理器，与容器内单例相同。</summary>
    public IPluginManager PluginManager { get; }

    /// <summary>Host 阶段日志器，失败与降级信息经它写入宿主日志管道。</summary>
    public ILogger Logger { get; }
}
