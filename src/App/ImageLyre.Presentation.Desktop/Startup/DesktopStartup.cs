using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Cangming.PicoPlugins.Core;
using ImageLyre.Bootstrap;
using ImageLyre.Bootstrap.Startup;
using ImageLyre.Contracts;
using ImageLyre.Shell.Contracts.Menus;
using ImageLyre.Shell.Menus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>桌面启动的宿主入口：创建欢迎窗、构建并启动 Generic Host，再在 UI 线程执行 PresentationPlan。</summary>
/// <remarks>
/// <para>
/// 执行顺序固定为四段：显示欢迎窗 → 构建并启动 Host（注册本地化与菜单注册表）→ 用共享执行器跑 PresentationPlan →
/// 挂上冒烟回调与兜底关停。欢迎窗先于 Host 启动出现，是因为它只依赖本地化，可以在插件启动期覆盖空窗；
/// 取壳与附着必须在 Host 启动之后，因为它们依赖插件产物。
/// </para>
/// <para>
/// 线程：本方法由 <c>App.OnFrameworkInitializationCompleted</c> 在 UI 线程调用，第一个 <c>await</c> 之前完成的欢迎窗创建与
/// <c>MainWindow</c> 赋值因此仍发生在经典桌面生命周期显示主窗口之前；<c>await</c> 之后由 Avalonia 同步上下文回到 UI 线程，
/// PresentationPlan 的每个步骤都在 UI 线程构造与显示控件。
/// </para>
/// <para>
/// 关停：<see cref="IClassicDesktopStyleApplicationLifetime.ShutdownRequested"/> 先取消本次退出，等待
/// <see cref="DesktopShutdown.StopAsync"/> 完成后才再次调用 <c>Shutdown()</c>，因此进程不会先于插件停止退出；
/// 取消后的重入由 <c>exitAllowed</c> 放行。<c>desktop.Exit</c> 与主窗体 <c>Closed</c> 只作兜底，触发同一个可等待的关停。
/// </para>
/// <para>
/// 失败：任何启动期异常都先用执行器的报告定位失败步骤，再释放已构造的壳窗体、关停宿主，把原因写 stderr 与日志并显示在欢迎窗上；
/// <see cref="StepOutcome.Degraded"/> 由执行器记录后继续，不阻断主窗体显示。
/// </para>
/// </remarks>
public static class DesktopStartup
{
    /// <summary>在 UI 线程上启动桌面宿主、装配主窗体并接管桌面生命周期。</summary>
    /// <param name="desktop">经典桌面生命周期，用于替换 <see cref="IClassicDesktopStyleApplicationLifetime.MainWindow"/> 与订阅退出。</param>
    /// <param name="localization">本次启动共用的本地化服务，欢迎窗、壳与 Feature 都从它取文案。</param>
    /// <param name="loggerFactory">宿主日志工厂，由 App 创建并传入，Bootstrap 用它初始化日志门面。</param>
    /// <returns>启动编排完成的任务；所有启动期异常在内部处理，不向调用方传播。</returns>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null"/>。</exception>
    /// <remarks>
    /// 欢迎窗在第一个 <c>await</c> 之前创建并挂到 <see cref="IClassicDesktopStyleApplicationLifetime.MainWindow"/>，
    /// 因此调用方必须以同步方式进入本方法（fire-and-forget 也可），不能先等待别的异步动作再转交，
    /// 否则经典桌面生命周期会先显示一个空桌面。
    /// </remarks>
    public static async Task RunAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        ILocalizationService localization,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        ILogger logger = loggerFactory.CreateLogger("ImageLyre.Presentation.Desktop.DesktopStartup");
        var shutdown = new DesktopShutdown(loggerFactory);

        // ShutdownRequested 先取消退出再等待关停；exitAllowed 只由“关停完成后的再次 Shutdown”使用，避免递归取消。
        var shutdownGate = new Lock();
        bool stopping = false;
        bool exitAllowed = false;

        desktop.ShutdownRequested += (_, e) => {
            lock (shutdownGate)
            {
                if (exitAllowed)
                    return;

                e.Cancel = true;
                if (stopping)
                    return;

                stopping = true;
            }

            _ = StopThenShutdownAsync();
        };

        // desktop.Exit 与主窗体 Closed 只作兜底：它们触发同一个可等待的关停，但不负责发起退出。
        desktop.Exit += (_, _) => _ = StopQuietlyAsync(shutdown, logger);

        var welcome = new WelcomeWindow(localization);
        desktop.MainWindow = welcome;             // 不显式 Show：经典桌面生命周期会显示它

        // 启动期间桌面可能已经退出：门闩已钉在 Stopped，这里不能再建宿主或窗口。
        if (!shutdown.TryBegin())
            return;

        DesktopBootstrapContext? context = null;
        try
        {
            IHost host = BootstrapHostBuilder.Build(
                loggerFactory,
                HostMode.Desktop,
                services => {
                    services.AddSingleton<ILocalizationService>(localization);
                    services.AddSingleton<IMenuRegistry>(new MenuRegistry());
                },
                args: []);

            if (!shutdown.AttachHost(host))
            {
                // 关停先于宿主登记：宿主从未启动，直接释放即可，不能留给已经结束的关停路径。
                host.Dispose();
                return;
            }

            await host.StartAsync();

            if (shutdown.IsStopped)
                return;

            IPluginManager pluginManager = host.Services.GetRequiredService<IPluginManager>();
            StartupReport report = host.Services.GetRequiredService<StartupReport>();
            context = new DesktopBootstrapContext(desktop, pluginManager, welcome);

            // Host 与 Presentation 共用同一份报告：失败步骤与回滚标记落在同一个按组聚合的视图里。
            await new BootstrapPlanRunner<DesktopBootstrapContext>(PresentationPlan.Create(), report, logger)
                .RunAsync(context, CancellationToken.None);

            Window shellWindow = context.ShellWindow
                ?? throw new InvalidOperationException("PresentationPlan 执行完成但没有取出壳窗体。");

            if (!shutdown.AttachShell(shellWindow))
            {
                DisposeShell(shellWindow);
                return;
            }

            shellWindow.Closed += (_, _) => _ = StopQuietlyAsync(shutdown, logger);

            DesktopSmokeHooks.CloseAfterDelay(shellWindow);
            DesktopSmokeHooks.OpenMenu(shellWindow);
            DesktopSmokeHooks.SwitchLanguage(shellWindow, localization);
        }
        catch (Exception ex)
        {
            // T13：Host 已启动但取壳失败时，先释放壳再停插件，不能留下半启动进程。
            // 失败原因同时写 stderr 与日志，命令行冒烟与诊断能看到；WinExe 双击仍可能丢 stderr。
            Console.Error.WriteLine($"启动失败：{ex}");
            logger.LogError(ex, "桌面启动失败。");
            DisposeShell(context?.ShellWindow);
            await StopQuietlyAsync(shutdown, logger);
            DesktopSmokeHooks.CloseAfterDelay(welcome);
            welcome.ShowFailure(ex);
        }

        async Task StopThenShutdownAsync()
        {
            try
            {
                await shutdown.StopAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "桌面关停未正常完成，继续退出。");
            }
            finally
            {
                lock (shutdownGate)
                    exitAllowed = true;

                desktop.Shutdown();
            }
        }
    }

    /// <summary>等待唯一一次关停，并把失败收敛为日志；供失败路径与兜底事件共用。</summary>
    /// <param name="shutdown">本次启动的关停收口。</param>
    /// <param name="logger">失败日志出口。</param>
    /// <returns>关停完成的任务；异常在内部记录，不向调用方传播。</returns>
    private static async Task StopQuietlyAsync(DesktopShutdown shutdown, ILogger logger)
    {
        try
        {
            await shutdown.StopAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "桌面关停失败。");
        }
    }

    /// <summary>释放实现 <see cref="IDisposable"/> 的壳窗体；为 <see langword="null"/> 时什么都不做。</summary>
    /// <param name="window">待释放的壳窗体。</param>
    private static void DisposeShell(Window? window)
    {
        if (window is IDisposable disposable)
            disposable.Dispose();
    }
}
