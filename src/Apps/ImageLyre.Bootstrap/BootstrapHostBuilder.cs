using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Cangming.Logs;
using Cangming.PicoPlugins;
using ImageLyre.Bootstrap.Logging;
using ImageLyre.Bootstrap.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap;

/// <summary>Host 层装配入口：日志与插件系统，全部与界面无关。</summary>
/// <remarks>
/// <para>
///     本层不出现任何 Avalonia 或 AtomUI 类型，界面宿主只负责把 UI 接在 <c>Host.StartAsync</c> 之后；
///     这条分工让"启动期禁止创建控件"成为结构约束，而不是口头约定。
/// </para>
/// <para>
///     装配顺序固定为：日志先行 → 禁用名单 → 扫描前缀 → 程序集预加载 → <c>PicoPluginSystem.Launch</c>
///     → 插件托管服务。日志必须先于任何插件类型的静态初始化，晚一步，该类型的静态日志字段就永久失效。
/// </para>
/// </remarks>
public static partial class BootstrapHostBuilder
{
    private static readonly ILogger s_logger = LogHub.For(typeof(BootstrapHostBuilder));

    /// <summary>创建并配置 Host，尚未启动。</summary>
    /// <param name="args">命令行参数，透传给 <see cref="Host.CreateDefaultBuilder(string[])" />。</param>
    /// <returns>已配置的 Host 构建器。</returns>
    public static IHostBuilder CreateHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureImageLyreLogging()
            .AddImageLyrePlugins();

    /// <summary>初始化日志并把日志工厂注册进容器。</summary>
    /// <param name="builder">Host 构建器。</param>
    /// <param name="logDirectory">日志根目录；为空时落到用户文档目录下的当月文件夹。</param>
    /// <returns>同一个构建器，便于链式调用。</returns>
    /// <remarks>
    ///     NLog 负责落盘与控制台，<see cref="LogHub" /> 与容器共用同一个 <see cref="ILoggerFactory" />；
    ///     日志目录与本次会话号由 <see cref="LoggingBootstrap" /> 统一管理。
    /// </remarks>
    public static IHostBuilder ConfigureImageLyreLogging(this IHostBuilder builder, string? logDirectory = null)
    {
        LoggingBootstrap.Initialize(logDirectory);
        var loggerFactory = LoggingBootstrap.GetLoggerFactory();

        return builder.ConfigureLogging((_, logging) =>
        {
            logging.ClearProviders();
            logging.Services.AddSingleton(loggerFactory);
        });
    }

    /// <summary>把插件系统装进 Host：禁用名单、扫描前缀、程序集预加载、目录与托管服务。</summary>
    /// <param name="builder">Host 构建器。</param>
    /// <returns>同一个构建器，便于链式调用。</returns>
    public static IHostBuilder AddImageLyrePlugins(this IHostBuilder builder) =>
        builder.ConfigureServices((_, services) =>
        {
            // ① 禁用名单必须在 Launch 之前回放：管理器构造时只读一次静态集合。
            var startupPolicy = PluginStartupPolicy.Load(PluginStartupPolicy.DefaultFilePath);
            services.AddSingleton(startupPolicy);

            foreach (var pluginId in startupPolicy.DisabledPluginIds)
            {
                PicoPluginSettings.DenyPlugin(pluginId);
            }

            // ② 扫描器按程序集文件名前缀认插件；内置前缀表里没有 ImageLyre.，必须自己补上。
            PicoPluginSettings.AddPluginAssemblyPrefixes(["ImageLyre."]);

            // ③ 随包清单：插件目录里的 plugin.json 可以覆盖特性上的标识、名称与版本。
            PicoPluginSettings.PluginManifestFileName = "plugin.json";

            var pluginsDirectory = Path.Combine(AppContext.BaseDirectory, "Plugins");
            Directory.CreateDirectory(pluginsDirectory);
            PicoPluginSettings.AddPluginDirectories(pluginsDirectory);

            // ④ 沧溟只扫描"已加载"的程序集：宿主没引用的插件 dll 不会被看见，必须先按前缀预加载。
            PreloadPluginAssemblies(pluginsDirectory);

            // ⑤ 扫描启动目录：把带 [Plugin] 的类型登记为单例（此刻不创建实例）。
            PicoPluginSystem.Launch(services);

            // ⑥ 插件的创建与启动交给 Host 生命周期。
            services.AddHostedService<PluginManagerHostedService>();
        });

    /// <summary>按插件前缀预加载程序集，让没有被宿主引用的插件也能进入扫描范围。</summary>
    /// <param name="pluginsDirectory">外部插件目录；目录不存在时跳过。</param>
    private static void PreloadPluginAssemblies(string pluginsDirectory)
    {
        var prefixes = PicoPluginSettings.DefaultPluginAssemblyPrefixesList.ToArray();
        var loaded = 0;

        foreach (var directory in new[] { AppContext.BaseDirectory, pluginsDirectory })
        {
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(directory, "*.dll"))
            {
                var name = Path.GetFileNameWithoutExtension(file);

                if (name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!prefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                try
                {
                    Assembly.LoadFrom(file);
                    loaded++;
                }
                catch (Exception exception)
                {
                    // 单个插件装不进来只记警告：坏掉的插件不该拖垮整个宿主。
                    LogPreloadFailed(s_logger, exception, name);
                }
            }
        }

        LogPreloadCompleted(s_logger, loaded, prefixes.Length);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "预加载插件程序集失败：{Assembly}")]
    private static partial void LogPreloadFailed(ILogger logger, Exception exception, string assembly);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "插件程序集预加载完成：{Count} 个（前缀 {PrefixCount} 条）")]
    private static partial void LogPreloadCompleted(ILogger logger, int count, int prefixCount);
}
