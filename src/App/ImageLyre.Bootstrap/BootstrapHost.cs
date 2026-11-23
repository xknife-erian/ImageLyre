using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap;

/// <summary>一次插件启动的结果：已构建的服务容器与解析出的插件管理器。</summary>
/// <remarks>
/// <para>
/// 实例由 <see cref="BootstrapHost.StartAsync"/> 产出，调用方在进程或窗口结束时调用 <see cref="DisposeAsync"/> 停止全部插件；
/// 本类不创建窗口、不持有线程，也不对启动失败做降级，失败时异常直接交给宿主显示或记录。
/// </para>
/// <para>
/// <see cref="Services"/> 与 <see cref="PluginManager"/> 在整个宿主生命周期内不变；管理器是容器内的单例，
/// 因此外部按接口再次解析得到的也是同一个实例。
/// </para>
/// </remarks>
public sealed class BootstrapHost : IAsyncDisposable
{
    private static readonly ILogger s_logger = LogHub.For(typeof(BootstrapHost));

    private readonly IPluginManager _pluginManager;

    private BootstrapHost(IServiceProvider services, IPluginManager pluginManager)
    {
        Services = services;
        _pluginManager = pluginManager;
    }

    /// <summary>启动链构建并已通知 cangming 的服务容器。</summary>
    /// <remarks>容器由本实例持有，调用方不应自行释放；<see cref="DisposeAsync"/> 只负责停止插件。</remarks>
    public IServiceProvider Services { get; }

    /// <summary>本次启动解析出的插件管理器，与 <see cref="Services"/> 中的实例相同。</summary>
    public IPluginManager PluginManager => _pluginManager;

    /// <summary>按固定顺序执行启动链并返回可关停的宿主。</summary>
    /// <param name="loggerFactory">日志工厂，用于初始化 <c>LogHub</c> 并注册进容器供插件管理器构造使用。</param>
    /// <param name="mode">宿主模式，只决定扫描期是否保留 Feature。</param>
    /// <param name="configure">宿主追加的服务注册；为空时不注册额外服务。</param>
    /// <param name="cancellationToken">取消令牌，只取消插件启动的等待。</param>
    /// <returns>已完成启动的宿主。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="OperationCanceledException">插件启动等待被 <paramref name="cancellationToken"/> 取消。</exception>
    /// <remarks>
    /// <para>
    /// 步骤顺序是契约：先初始化 <c>LogHub</c>，再设置 <c>PicoPluginSettings.HasUiRoute</c> 与扫描前缀，
    /// 因为 cangming 在 <c>Launch</c> 的扫描阶段读取这些静态设置；随后才创建容器、执行宿主的 <paramref name="configure"/>、
    /// 扫描并注册插件、构建容器、解析管理器、通知容器已构建，最后启动插件并输出能力插件计数。
    /// 任何一步提前或调换都会让扫描结果与日志和实际状态不一致。
    /// </para>
    /// <para>
    /// 线程与重入：本方法在调用方线程上同步执行到 <see cref="IPluginManager.StartAsync"/> 的等待，
    /// 因此 M1 的所有插件 <c>StartServiceAsync</c> 必须同步完成，不得等待 UI 上下文的异步续体。
    /// </para>
    /// </remarks>
    public static async Task<BootstrapHost> StartAsync(
        ILoggerFactory loggerFactory,
        HostMode mode,
        Action<IServiceCollection>? configure,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        // ponytail: StartAsync 没有重入保护，同一进程重复启动会与 LogHub 的一次性初始化和静态扫描冲突，等 T13 装配链收口时补。
        LogHub.Initialize(loggerFactory);

        // ponytail: HasUiRoute 是进程级静态开关，同进程要跑两种宿主时再改成实例化配置。
        PicoPluginSettings.HasUiRoute = mode == HostMode.Desktop;
        PicoPluginSettings.AddPluginAssemblyPrefixes(["ImageLyre.Module.", "ImageLyre.Feature."]);

        // T04 的判据要从日志确认扫描路线：cangming 只输出扫到的插件数，不输出该开关的值。
        s_logger.LogInformation("HasUiRoute={HasUiRoute}", PicoPluginSettings.HasUiRoute);

        var services = new ServiceCollection();

        // 管理器构造依赖 ILoggerFactory，而 cangming 不注册日志服务，必须由宿主登记；放在 configure 之前便于覆盖。
        services.AddSingleton(loggerFactory);

        // ponytail: 容器里只有 ILoggerFactory、没有 ILogger<T>；插件要构造注入日志器时再补 AddLogging，M1 一律用 LogHub。
        configure?.Invoke(services);

        PicoPluginSystem.Launch(services);

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IPluginManager>();

        PicoPluginSystem.NotifyServiceProviderBuilt(provider);
        await manager.StartAsync().WaitAsync(cancellationToken);

        // ponytail: 只统计 lyre.capability. 前缀，平台与表达插件要等 G2 的插件面板。
        int capabilityCount = manager.Plugins.Count(
            plugin => manager.GetPluginInfo(plugin.GetType()).Id.StartsWith("lyre.capability.", StringComparison.Ordinal));
        s_logger.LogInformation("能力插件 {Count} 个", capabilityCount);

        return new BootstrapHost(provider, manager);
    }

    /// <summary>停止本次启动的全部插件。</summary>
    /// <returns>停止完成的任务。</returns>
    /// <remarks>调用方应在主窗体关闭或进程退出前调用一次；本方法不释放 <see cref="Services"/> 容器。</remarks>
    public async ValueTask DisposeAsync() => await _pluginManager.StopAsync();
}
