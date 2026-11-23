using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.App.Cli;
using ImageLyre.App.Startup;
using ImageLyre.Bootstrap;
using ImageLyre.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace ImageLyre.App;

/// <summary>ImageLyre 的 Avalonia 应用：先显示欢迎窗口，再在 UI 线程上完成 Bootstrap、取壳与主窗体切换。</summary>
/// <remarks>
/// <para>
/// 宿主分派在 <see cref="Program.Main"/> 完成，本类因此只在桌面分支被构造；欢迎窗口先挂到桌面主窗口，
/// <see cref="ComposeAsync"/> 随后在 UI 线程上异步启动 Bootstrap、创建本次启动唯一的 <c>SlotManager</c>、
/// 取壳、附着 Feature，再把主窗体换到桌面并关闭欢迎窗口。
/// </para>
/// <para>
/// 线程要求：从注入槽位管理器到 <c>window.Show()</c> 的每一步都在 UI 线程执行，因为取壳会构造 Avalonia 控件、
/// 附着会创建 Feature 内容；Bootstrap 的等待不阻塞 UI 循环，编排以 fire-and-forget 任务启动，异常在任务内部捕获。
/// </para>
/// <para>
/// 失败路径：启动期异常在欢迎窗口上显示原因，不让进程只剩日志；主窗体关闭时先释放壳体，再停止 Bootstrap。
/// 本地化实例由入口创建并经容器注册为同一实例，欢迎窗口与主窗体共享它。
/// </para>
/// </remarks>
public sealed partial class App : Application
{
    private readonly StartupOptions _options;
    private readonly ILocalizationService _localization;

    /// <summary>创建应用实例。</summary>
    /// <param name="options">启动参数解析结果，用于约束无界面分支不得进入本类。</param>
    /// <param name="localization">本地化服务，欢迎窗口与主界面共用同一实例。</param>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> 或 <paramref name="localization"/> 为 <see langword="null"/>。</exception>
    public App(StartupOptions options, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(localization);

        _options = options;
        _localization = localization;
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var welcome = new WelcomeWindow(_localization, _options);
            desktop.MainWindow = welcome;                 // 不显式 Show：经典桌面生命周期会显示它
            _ = ComposeAsync(desktop, welcome);           // 不阻塞 UI 循环
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>在 UI 线程上启动 Bootstrap、取壳、附着 Feature 并切换到主窗体。</summary>
    /// <param name="desktop">经典桌面生命周期，用于替换 <see cref="IClassicDesktopStyleApplicationLifetime.MainWindow"/>。</param>
    /// <param name="welcome">启动期欢迎窗口，成功时关闭，失败时显示原因。</param>
    /// <returns>编排完成的任务；所有启动期异常在内部处理，不向调用方传播。</returns>
    /// <remarks>
    /// <para>
    /// 步骤顺序固定：启动 Bootstrap（注册同一本地化实例）→ 创建唯一 <c>SlotManager</c> → 注入并取壳（壳在构造期贡献槽位）
    /// → 附着 Feature → 挂主窗体并显示 → 关闭欢迎窗口。cangming v1 的槽位必须先贡献再附着，不能把取壳与附着互换。
    /// </para>
    /// <para>
    /// 线程：方法从 UI 线程进入；<c>await</c> 之后由 Avalonia 同步上下文回到 UI 线程，因此取壳与显示不会跨线程碰控件。
    /// </para>
    /// <para>
    /// 失败：启动期异常被捕获后在欢迎窗口显示原因；主窗体关闭时先释放壳体，再停止 Bootstrap，避免插件停止后控件仍被访问。
    /// </para>
    /// </remarks>
    private async Task ComposeAsync(IClassicDesktopStyleApplicationLifetime desktop, WelcomeWindow welcome)
    {
        try
        {
            // ponytail: WinExe 没有控制台，写 stderr 的日志在双击启动时会丢失；日志管线归后续任务，不为它引入新包。
            var loggerFactory = new StderrLoggerFactory(Console.Error);

            BootstrapHost bootstrap = await BootstrapHost.StartAsync(
                loggerFactory,
                HostMode.Desktop,
                services => services.AddSingleton<ILocalizationService>(_localization),
                CancellationToken.None);

            var slotManager = new SlotManager();
            Window window = MainWindowFactory.Compose(bootstrap.PluginManager, slotManager);

            HookSmokeClose(window);

            window.Closed += async (_, _) =>
            {
                if (window is IDisposable disposable)
                    disposable.Dispose();

                try
                {
                    await bootstrap.DisposeAsync();
                }
                catch (Exception ex)
                {
                    // ponytail: 关停失败没有日志出口，WinExe 下 stderr 不可见；日志管线接入后改记日志。
                    System.Diagnostics.Debug.WriteLine($"Bootstrap 关停失败：{ex}");
                }
            };

            desktop.MainWindow = window;
            window.Show();
            welcome.Close();
        }
        catch (Exception ex)
        {
            HookSmokeClose(welcome);
            welcome.ShowFailure(ex);
        }
    }

    /// <summary>按 <c>LYRE_SMOKE_CLOSE_MS</c> 约定在窗口打开后定时关闭它。</summary>
    /// <param name="window">要监听的窗口；主窗体与失败时的欢迎窗口共用这一方法。</param>
    /// <remarks>
    /// 环境变量缺失、不是整数或不为正数时什么都不做；窗口已经可见时立即启动定时器，否则等 <c>Opened</c>。
    /// 定时器只在 UI 线程上创建与启动，同一窗口只启动一次。
    /// </remarks>
    private static void HookSmokeClose(Window window)
    {
        string? raw = Environment.GetEnvironmentVariable("LYRE_SMOKE_CLOSE_MS");
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int delay) || delay <= 0)
            return;

        bool started = false;

        void StartTimer()
        {
            if (started)
                return;

            started = true;
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delay) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                window.Close();
            };
            timer.Start();
        }

        // ponytail: 这是 M1 的自动冒烟钩子，等接入真实 UI 测试后删除，不要把它当成产品特性。
        if (window.IsVisible)
            StartTimer();
        else
            window.Opened += (_, _) => StartTimer();
    }
}
