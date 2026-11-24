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
/// 执行顺序固定为四段：显示欢迎窗 → 构建并启动 Host（注册本地化与菜单注册表）→ 执行 PresentationPlan →
/// 挂上主窗体关闭与冒烟回调。欢迎窗先于 Host 启动出现，是因为它只依赖本地化，可以在插件启动期覆盖空窗；
/// 取壳与附着必须在 Host 启动之后，因为它们依赖插件产物。
/// </para>
/// <para>
/// 线程：本方法由 <c>App.OnFrameworkInitializationCompleted</c> 在 UI 线程调用，第一个 <c>await</c> 之前完成的欢迎窗创建与
/// <c>MainWindow</c> 赋值因此仍发生在经典桌面生命周期显示主窗口之前；<c>await</c> 之后由 Avalonia 同步上下文回到 UI 线程，
/// PresentationPlan 的每个步骤都在 UI 线程构造与显示控件。
/// </para>
/// <para>
/// 失败：任何启动期异常都先释放已构造的壳窗体、关停宿主，再把原因写 stderr 并显示在欢迎窗上，不留下半启动进程；
/// 成功时欢迎窗由显示组关闭，主窗体关闭或 <c>desktop.Exit</c> 走同一个 <see cref="DesktopShutdown"/>，重复关停只执行一次。
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

        var shutdown = new DesktopShutdown();
        // desktop.Exit 与主窗体 Closed 共用同一个关停；门闩让后到的那条路径直接返回。
        desktop.Exit += (_, _) => _ = shutdown.StopAsync();

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
            context = new DesktopBootstrapContext(desktop, pluginManager, welcome);

            await RunPresentationPlanAsync(context);

            Window shellWindow = context.ShellWindow
                ?? throw new InvalidOperationException("PresentationPlan 执行完成但没有取出壳窗体。");

            if (!shutdown.AttachShell(shellWindow))
            {
                DisposeShell(shellWindow);
                return;
            }

            shellWindow.Closed += async (_, _) => await shutdown.StopAsync();

            DesktopSmokeHooks.CloseAfterDelay(shellWindow);
            DesktopSmokeHooks.OpenMenu(shellWindow);
            DesktopSmokeHooks.SwitchLanguage(shellWindow, localization);
        }
        catch (Exception ex)
        {
            // T13：Host 已启动但取壳失败时，先释放壳再停插件，不能留下半启动进程。
            // 失败原因同时写 stderr，命令行冒烟与诊断能看到；WinExe 双击仍可能丢日志。
            Console.Error.WriteLine($"启动失败：{ex}");
            DisposeShell(context?.ShellWindow);
            await shutdown.StopAsync();
            DesktopSmokeHooks.CloseAfterDelay(welcome);
            welcome.ShowFailure(ex);
        }
    }

    /// <summary>按扁平顺序执行 PresentationPlan，遇到失败或中止即抛出带 Step Id 的异常。</summary>
    /// <param name="context">本次 Presentation 阶段的上下文。</param>
    /// <returns>全部步骤完成的任务。</returns>
    /// <exception cref="InvalidOperationException">某一步返回 <see cref="StepOutcome.Failed"/> 或 <see cref="StepOutcome.Aborted"/>。</exception>
    /// <remarks>
    /// 本方法不切换线程、不做回滚，也不写 StartupReport：线程由调用方已在 UI 线程约定，回滚与报告归 T09/T10；
    /// 异常消息带 Step Id，是为了让失败先能定位到步骤，而不是只能读调用栈。
    /// </remarks>
    private static async Task RunPresentationPlanAsync(DesktopBootstrapContext context)
    {
        foreach (IBootstrapStep<DesktopBootstrapContext> step in PresentationPlan.Create().Flatten())
        {
            StepResult result = await step.ExecuteAsync(context, CancellationToken.None);
            if (result.Outcome is StepOutcome.Failed or StepOutcome.Aborted)
                throw new InvalidOperationException($"Presentation 步骤 {step.Id} 未完成：{result.Outcome}。", result.Error);
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
