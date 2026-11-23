using System.Collections.Immutable;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.App.Localization;
using ImageLyre.App.Startup;
using ImageLyre.Bootstrap;
using ImageLyre.Contracts;
using ImageLyre.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace ImageLyre.App.Cli;

/// <summary>无界面宿主：执行一条 CLI 命令并把结果写到指定文本流。</summary>
/// <remarks>
/// <para>
/// 本类只在 <see cref="StartupOptions.Headless"/> 为 <see langword="true"/> 时由 <c>Program.Main</c> 调用，
/// 它不构造 <c>AppBuilder</c>、不创建窗口、不初始化 Avalonia；三条命令只读写传入的文本流，其中 stdout 留给命令结果，
/// stderr 留给诊断与错误，因此调用方必须传入两个不同的写入器。
/// </para>
/// <para>
/// 退出码集中在本类的三个常量里：<see cref="ExitSuccess"/> 表示命令执行成功，<see cref="ExitUsageError"/> 表示参数未知或冲突，
/// <see cref="ExitRuntimeFailure"/> 表示命令已识别但运行期失败；前两者是确定的契约，后者只保证非零，
/// 调用方不应据此区分具体故障。
/// </para>
/// <para>
/// 输出文本是机器可读接口而不是界面文案，因此不经过 <c>ILocalizationService</c>，也不随界面语言切换；
/// 这条判断同时写在 <c>tests/UnitTest.ImageLyre.App/README.md</c> 里，避免后续把 CLI 文案接进本地化服务。
/// </para>
/// </remarks>
public static class CliHost
{
    /// <summary>命令执行成功的退出码。</summary>
    public const int ExitSuccess = 0;

    /// <summary>命令已识别但运行期失败的退出码，例如 Bootstrap 启动失败或命令输出写入失败。</summary>
    public const int ExitRuntimeFailure = 1;

    /// <summary>参数未知、重复或出现多个命令时的退出码。</summary>
    public const int ExitUsageError = 2;

    private const string ProductName = "ImageLyre";

    private static readonly string[] s_helpLines =
    [
        "用法：ImageLyre [--version | --plugins | --help]",
        string.Empty,
        "不带任何命令参数时启动桌面界面。",
        string.Empty,
        "命令：",
        "  --version  输出版本行",
        "  --plugins  以无界面模式启动 Bootstrap，列出插件 Id、名称、状态与是否 Feature",
        "  --help     显示本帮助",
        string.Empty,
        "退出码：",
        "  0  命令执行成功",
        "  1  运行期失败（例如插件启动失败）",
        "  2  未知或冲突的命令行参数",
    ];

    /// <summary>执行一条无界面命令并把结果写到指定文本流。</summary>
    /// <param name="options">已解析的启动参数，命令为 <see cref="StartupCommand.Desktop"/> 时非法。</param>
    /// <param name="output">命令输出流，通常是 <see cref="Console.Out"/>；只接收命令结果，不接收日志。</param>
    /// <param name="error">错误与诊断流，通常是 <see cref="Console.Error"/>；日志与失败原因都写到这里。</param>
    /// <param name="cancellationToken">取消令牌，只影响 <see cref="StartupCommand.Plugins"/> 启动 Bootstrap 的等待。</param>
    /// <returns>进程退出码，取值见 <see cref="ExitSuccess"/>、<see cref="ExitRuntimeFailure"/> 与 <see cref="ExitUsageError"/>。</returns>
    /// <exception cref="ArgumentNullException">任一引用参数为 <see langword="null"/>。</exception>
    /// <exception cref="ArgumentException"><paramref name="options"/> 的命令为 <see cref="StartupCommand.Desktop"/>。</exception>
    /// <remarks>
    /// <para>
    /// 分派只按 <see cref="StartupOptions.Command"/> 走四条分支：<c>--version</c> 与 <c>--help</c> 只写文本，
    /// <c>--plugins</c> 先以 <see cref="HostMode.Headless"/> 启动 <see cref="BootstrapHost"/> 再遍历插件，
    /// <c>Invalid</c> 只回显原始参数；四条分支都不创建窗口、不初始化 Avalonia。
    /// </para>
    /// <para>
    /// 任何运行期异常（包括 Bootstrap 启动失败与取消）都在本方法内捕获，向 <paramref name="error"/> 写一行原因并返回
    /// <see cref="ExitRuntimeFailure"/>；参数为 <see langword="null"/> 与桌面命令误用属于调用方编程错误，仍按异常抛出，
    /// 因为把编程错误伪装成退出码会让入口层的分派缺陷更难发现。
    /// </para>
    /// </remarks>
    public static async Task<int> RunAsync(
        StartupOptions options,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (options.Command == StartupCommand.Desktop)
            throw new ArgumentException("桌面命令不得交给 CliHost 执行。", nameof(options));

        try
        {
            return options.Command switch
            {
                StartupCommand.Version => await WriteVersionAsync(output),
                StartupCommand.Plugins => await WritePluginsAsync(output, error, cancellationToken),
                StartupCommand.Help => await WriteHelpAsync(output),
                StartupCommand.Invalid => await WriteInvalidAsync(options, error),
                _ => throw new ArgumentOutOfRangeException(nameof(options), options.Command, "未知的启动命令。"),
            };
        }
        catch (Exception ex)
        {
            await error.WriteLineAsync($"运行失败：{ex.GetType().Name}：{ex.Message}");
            return ExitRuntimeFailure;
        }
    }

    /// <summary>输出产品名与当前程序集版本。</summary>
    /// <param name="output">命令输出流。</param>
    /// <returns>恒为 <see cref="ExitSuccess"/>。</returns>
    private static async Task<int> WriteVersionAsync(TextWriter output)
    {
        string version = typeof(CliHost).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
        await output.WriteLineAsync($"{ProductName} {version}");
        return ExitSuccess;
    }

    /// <summary>启动无界面 Bootstrap，列出插件并输出汇总行。</summary>
    /// <param name="output">命令输出流，只接收插件清单。</param>
    /// <param name="error">诊断流，传给 <see cref="StderrLoggerFactory"/> 接收 Bootstrap 日志。</param>
    /// <param name="cancellationToken">取消令牌，只取消插件启动的等待。</param>
    /// <returns>插件清单写完后恒为 <see cref="ExitSuccess"/>；启动失败由调用方统一捕获。</returns>
    /// <remarks>
    /// 每个插件占一个四行区块，字段依次为 Id、名称、状态与是否 Feature；状态取插件实例的 <c>State</c>，
    /// Feature 判定取 <see cref="IPicoFeatures"/> 标记接口，汇总行同时给出插件总数与 Feature 数。
    /// 启动前注册一个本地化实例供壳插件解析：CLI 自己的输出不走本地化，但插件启动链要求宿主容器里有
    /// <see cref="ILocalizationService"/>，否则壳插件会在 <c>OnStartAsync</c> 落 Failed。
    /// </remarks>
    private static async Task<int> WritePluginsAsync(TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        using var loggerFactory = new StderrLoggerFactory(error);
        var localization = new LocalizationService();
        localization.AddSource(new AppStringSource());

        await using BootstrapHost host = await BootstrapHost.StartAsync(
            loggerFactory,
            HostMode.Headless,
            services => services.AddSingleton<ILocalizationService>(localization),
            cancellationToken);

        IImmutableList<IPicoPlugin> plugins = host.PluginManager.Plugins;
        int featureCount = 0;

        await output.WriteLineAsync("插件清单（无界面宿主）：");
        foreach (IPicoPlugin plugin in plugins)
        {
            PluginInfo info = host.PluginManager.GetPluginInfo(plugin.GetType());
            bool isFeature = plugin is IPicoFeatures;
            if (isFeature)
                featureCount++;

            await output.WriteLineAsync();
            await output.WriteLineAsync($"Id: {info.Id}");
            await output.WriteLineAsync($"名称: {info.Name}");
            await output.WriteLineAsync($"状态: {plugin.State}");
            await output.WriteLineAsync($"Feature: {(isFeature ? "是" : "否")}");
        }

        await output.WriteLineAsync();
        await output.WriteLineAsync($"共 {plugins.Count} 个插件，其中 Feature {featureCount} 个");

        // ponytail: 插件清单是纯文本，G2 的插件面板会替换这里的呈现方式；替换时保留 Id、名称、状态与 Feature 四个字段。
        return ExitSuccess;
    }

    /// <summary>输出用法、三条命令与退出码约定。</summary>
    /// <param name="output">命令输出流。</param>
    /// <returns>恒为 <see cref="ExitSuccess"/>。</returns>
    private static async Task<int> WriteHelpAsync(TextWriter output)
    {
        foreach (string line in s_helpLines)
            await output.WriteLineAsync(line);

        return ExitSuccess;
    }

    /// <summary>把原始参数回显到错误流，并给出一行用法要点。</summary>
    /// <param name="options">解析结果，提供原始参数。</param>
    /// <param name="error">错误流。</param>
    /// <returns>恒为 <see cref="ExitUsageError"/>。</returns>
    private static async Task<int> WriteInvalidAsync(StartupOptions options, TextWriter error)
    {
        string rawArguments = string.Join(' ', options.RawArguments);
        await error.WriteLineAsync($"未知或冲突的参数：{rawArguments}");
        await error.WriteLineAsync("用法：ImageLyre [--version | --plugins | --help]。");
        return ExitUsageError;
    }
}
