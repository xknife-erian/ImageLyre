using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Cangming.Logs;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.Toolkit.Avalonias.AtomUI;
using ImageLyre.App.Startup;
using ImageLyre.Bootstrap;
using ImageLyre.Bootstrap.Services;
using ImageLyre.Shell.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.App;

/// <summary>桌面进程入口：日志与插件 → 界面 → 反序收尾。</summary>
/// <remarks>
/// <para>
///     启动顺序是本类的四步主干。插件启动阶段禁止创建控件，因此 Host 必须先启动；
///     主窗口只能由插件产出，因此窗口编排推迟到 Avalonia 就绪之后。
/// </para>
/// <para>
///     收尾必须离开 Avalonia 的同步上下文：消息循环返回后，本线程上仍挂着已停摆的派发器，
///     任何没有 <c>ConfigureAwait(false)</c> 的 <c>await</c> 都会把续体排进那个队列，于是关窗不退出。
/// </para>
/// </remarks>
internal static partial class Program
{
    private static readonly ILogger s_logger = LogHub.For(typeof(Program));

    /// <summary>进程入口。</summary>
    /// <param name="args">命令行参数，同时交给 Host 与 Avalonia。</param>
    [STAThread]
    public static void Main(string[] args)
    {
        // 异常通道要早于 Host 构建：装配阶段抛的异常同样需要落地。
        UnhandledExceptionProcessor.Setup();

        IHost host = BootstrapHostBuilder.CreateHost(args).Build();

        try
        {
            host.StartAsync().GetAwaiter().GetResult();

            IPluginManager pluginManager = host.Services.GetRequiredService<IPluginManager>();

            BuildAvaloniaApp(() => ComposeMainWindow(pluginManager)).StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            var watch = Stopwatch.StartNew();
            HostShutdown.Stop(host);
            LogShutdownElapsed(s_logger, watch.ElapsedMilliseconds);
        }
    }

    /// <summary>构建 Avalonia 应用，界面栈经沧溟的 AtomUI 接入配置。</summary>
    /// <param name="composeMainWindow">主窗口编排工厂，在 Avalonia 就绪后执行。</param>
    /// <returns>配置好的构建器。</returns>
    private static AppBuilder BuildAvaloniaApp(Func<Window> composeMainWindow) =>
        AppBuilder.Configure(() => new App(composeMainWindow))
                  .UseCangmingAtomUI()
                  .LogToTrace();

    /// <summary>编排主窗口：注入区域管理器、由容器模块建窗口、附着功能、记录区域诊断。</summary>
    /// <param name="pluginManager">已完成启动的插件管理器。</param>
    /// <returns>由容器模块产出的主窗口。</returns>
    /// <exception cref="InvalidOperationException">容器模块缺席时抛出。</exception>
    private static Window ComposeMainWindow(IPluginManager pluginManager)
    {
        // 顺序固定：先注入区域管理器（容器模块的上下文只保存延迟引用），
        // 再由容器模块建窗口，最后附着功能。反过来做会在创建窗口时抛"SlotManager 未注入"。
        ISlotManager slotManager = pluginManager.InjectSlotManager<SlotManager>();

        IWorkbenchShell shell = pluginManager.FindModuleBuilder<IWorkbenchShell>()?.Build().Value
                                ?? throw new InvalidOperationException(
                                    "IWorkbenchShell 未找到：宿主必须引用 ImageLyre.Module.Workbench，否则 dll 不在输出目录、扫描静默缺席。");

        Window window = shell.CreateShell();
        pluginManager.RegisterFeatureAttachments(slotManager);

        ReportSlots(slotManager);

        return window;
    }

    /// <summary>把区域与附着物数量写进日志，排障时一眼看出哪个功能没挂上。</summary>
    private static void ReportSlots(ISlotManager slotManager)
    {
        var summary = string.Join(
            ", ",
            slotManager.GetAllSlots().Select(slot => $"{slot.Name}={slotManager.GetSlotContents(slot.Name).Count}"));

        LogSlotSummary(s_logger, summary);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "宿主收尾耗时 {Elapsed}ms")]
    private static partial void LogShutdownElapsed(ILogger logger, long elapsed);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "区域附着情况：{Slots}")]
    private static partial void LogSlotSummary(ILogger logger, string slots);
}
