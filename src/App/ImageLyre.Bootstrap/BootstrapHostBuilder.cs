using Cangming.Logs;
using Cangming.PicoPlugins;
using ImageLyre.Bootstrap.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap;

/// <summary>把现有的插件启动链装配成 Generic Host：一次构建产出一个 <see cref="IHost"/>，宿主启动时执行 HostPlan。</summary>
/// <remarks>
/// <para>
/// 顺序是契约：先初始化 <c>LogHub</c>，再设置 <c>PicoPluginSettings.HasUiRoute</c> 与扫描前缀，随后构建 Host、注册宿主服务，
/// 最后在 <c>Build</c> 之前调用 <c>PicoPluginSystem.Launch</c>，因为 cangming 在扫描阶段读取这些静态设置。
/// <c>NotifyServiceProviderBuilt</c> 与插件启动不在这里发生，它们属于 HostPlan 的插件复合步骤。
/// </para>
/// <para>
/// 进程级限制：<c>LogHub</c> 只允许初始化一次，重复构建复用第一次发布的工厂，因此同一进程的多个宿主共享同一个日志门面；
/// 扫描前缀只在缺失时补齐，<c>HasUiRoute</c> 每次构建按传入模式重设。生产路径一个进程只构建一个宿主，多宿主只用于测试与将来的宿主切换。
/// </para>
/// </remarks>
public static class BootstrapHostBuilder
{
    private static readonly string[] s_pluginAssemblyPrefixes = ["ImageLyre.Module.", "ImageLyre.Feature."];

    /// <summary>按给定日志工厂与宿主模式构建一个尚未启动的 Generic Host。</summary>
    /// <param name="loggerFactory">宿主日志工厂，同时交给 <c>LogHub</c> 与容器；不会被默认 provider 替换。</param>
    /// <param name="mode">宿主模式，只决定扫描期是否保留 Feature。</param>
    /// <param name="configure">宿主追加的服务注册；在机制服务注册之前执行，与迁移前一致，显式注册可覆盖默认实现。</param>
    /// <param name="args">宿主配置参数，原样交给 <see cref="Host.CreateApplicationBuilder(string[])"/>；应用命令参数不应传入。</param>
    /// <returns>尚未启动的 <see cref="IHost"/>；调用方负责 <c>StartAsync</c> 与关停。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory"/> 或 <paramref name="args"/> 为 <see langword="null"/>。</exception>
    public static IHost Build(
        ILoggerFactory loggerFactory,
        HostMode mode,
        Action<IServiceCollection>? configure,
        string[] args)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(args);

        // ponytail: LogHub 只允许初始化一次；重复构建共享第一次的工厂，等 cangming 提供可重置的日志门面再改。
        if (!LogHub.IsInitialized)
        {
            LogHub.Initialize(loggerFactory);
        }

        // HasUiRoute 是进程级静态开关，每次构建按传入模式重设；扫描前缀只补缺失项，避免重复构建让列表膨胀。
        PicoPluginSettings.HasUiRoute = mode == HostMode.Desktop;
        string[] missingPrefixes = s_pluginAssemblyPrefixes
            .Where(prefix => !PicoPluginSettings.DefaultPluginAssemblyPrefixesList.Contains(prefix))
            .ToArray();
        if (missingPrefixes.Length > 0)
        {
            PicoPluginSettings.AddPluginAssemblyPrefixes(missingPrefixes);
        }

        // 与迁移前保持同一日志分类，CLI 冒烟按文本匹配 HasUiRoute；这里直接用传入工厂，避免 LogHub 首次绑定与测试顺序耦合。
        loggerFactory
            .CreateLogger("ImageLyre.Bootstrap.BootstrapHost")
            .LogInformation("HasUiRoute={HasUiRoute}", PicoPluginSettings.HasUiRoute);

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        // Host 默认会加 Console/Debug/EventSource provider；清空后把宿主工厂换成调用方提供的实例，stderr 工厂才不会被默认 provider 覆盖。
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton(loggerFactory);

        configure?.Invoke(builder.Services);

        builder.Services.AddSingleton<StartupReport>();
        builder.Services.AddSingleton(HostPlan.Create());
        builder.Services.AddHostedService<AppBootstrapService>();

        // 扫描与容器注册必须发生在 Build 之前；cangming 在 Launch 时读取 HasUiRoute 与扫描前缀的快照。
        PicoPluginSystem.Launch(builder.Services);

        return builder.Build();
    }
}
