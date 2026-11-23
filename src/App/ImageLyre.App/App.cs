using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.App.Cli;
using ImageLyre.App.Startup;
using ImageLyre.Bootstrap;
using ImageLyre.Contracts;
using Microsoft.Extensions.DependencyInjection;
using AtomMenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace ImageLyre.App;

/// <summary>ImageLyre 的 Avalonia 应用：先显示欢迎窗口，再在 UI 线程上完成 Bootstrap、取壳与主窗体切换。</summary>
/// <remarks>
/// <para>
/// 宿主分派在 <see cref="Program.Main"/> 完成，本类因此只在桌面分支被构造；欢迎窗口先挂到桌面主窗口，
/// <see cref="ComposeAsync"/> 随后在 UI 线程上异步启动 Bootstrap、创建本次启动唯一的 <c>SlotManager</c>、
/// 取壳、附着 Feature，再把主窗体换到桌面并关闭欢迎窗口。
/// </para>
/// <para>
/// 装配门闩：<see cref="CompositionGate"/> 保证同一进程的编排与关停各只发生一次；<see cref="ComposeAsync"/>
/// 抢不到开始权时直接返回，<see cref="StopAsync"/> 被 <c>Closed</c>、<c>desktop.Exit</c> 与失败分支重复调用时只执行一次。
/// </para>
/// <para>
/// 失败路径：启动期异常先走 <see cref="StopAsync"/> 停掉已启动的 Bootstrap，再在欢迎窗口上显示原因；
/// 正常关停时先释放壳体（退订四个 <c>SlotPanel</c> 的槽位事件），再停止插件，避免插件停止后控件仍被访问。
/// </para>
/// <para>
/// 线程要求：从注入槽位管理器到 <c>window.Show()</c> 的每一步都在 UI 线程执行，因为取壳会构造 Avalonia 控件、
/// 附着会创建 Feature 内容；Bootstrap 的等待不阻塞 UI 循环，编排以 fire-and-forget 任务启动，异常在任务内部捕获。
/// </para>
/// </remarks>
public sealed partial class App : Application
{
    private readonly StartupOptions _options;
    private readonly ILocalizationService _localization;
    private readonly ImageLyre.Shell.Menus.MenuRegistry _menus = new();
    private readonly CompositionGate _gate = new();

    private BootstrapHost? _bootstrap;
    private Window? _shellWindow;
    private SlotManager? _slotManager;

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
            // desktop.Exit 与主窗体 Closed 共用同一个 StopAsync；门闩让后到的那条路径直接返回。
            desktop.Exit += (_, _) => _ = StopAsync();

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
    /// 失败：启动期异常先停掉已经启动的 Bootstrap，再在欢迎窗口显示原因；主窗体关闭时走同一个 <see cref="StopAsync"/>。
    /// </para>
    /// </remarks>
    private async Task ComposeAsync(IClassicDesktopStyleApplicationLifetime desktop, WelcomeWindow welcome)
    {
        if (!_gate.TryBegin())
            return;

        try
        {
            // ponytail: WinExe 没有控制台，写 stderr 的日志在双击启动时会丢失；日志管线归后续任务，不为它引入新包。
            var loggerFactory = new StderrLoggerFactory(Console.Error);

            BootstrapHost bootstrap = await BootstrapHost.StartAsync(
                loggerFactory,
                HostMode.Desktop,
                services => {
                    services.AddSingleton<ILocalizationService>(_localization);
                    services.AddSingleton<ImageLyre.Shell.Contracts.Menus.IMenuRegistry>(_menus);
                },
                CancellationToken.None);

            _bootstrap = bootstrap;

            // 启动期间桌面可能已经退出：StopAsync 已把门闩钉在 Stopped，这里不能再建窗口。
            if (_gate.IsStopped)
            {
                // 谁先拿到字段谁负责停；若 StopAsync 已取走，它已经停过，不能重复 Dispose。
                BootstrapHost? owned = Interlocked.Exchange(ref _bootstrap, null);
                if (ReferenceEquals(owned, bootstrap))
                    await DisposeBootstrapAsync(bootstrap);

                return;
            }

            SlotManager slotManager = CreateSlotManagerOnce();
            Window window = MainWindowFactory.Compose(bootstrap.PluginManager, slotManager);
            _shellWindow = window;

            // 取壳到 Show 之间没有 await；若这段同步代码里收到退出信号，门闩已停止，不能再显示窗口。
            if (_gate.IsStopped)
            {
                // 谁先拿到字段谁负责释放；若 StopAsync 已取走，它已经释放过，不能重复 Dispose。
                Window? owned = Interlocked.Exchange(ref _shellWindow, null);
                if (ReferenceEquals(owned, window) && window is IDisposable disposable)
                    disposable.Dispose();

                return;
            }

            HookSmokeClose(window);
            window.Closed += async (_, _) => await StopAsync();

            desktop.MainWindow = window;
            window.Show();
            HookSmokeMenuOpen(window);
            HookSmokeLanguage(window);
            welcome.Close();
        }
        catch (Exception ex)
        {
            // T13：Bootstrap 已启动但取壳失败时，先把插件停掉再显示失败，不能留下半启动进程。
            // 失败原因同时写 stderr，命令行冒烟与诊断能看到；WinExe 双击仍可能丢日志。
            Console.Error.WriteLine($"启动失败：{ex}");
            await StopAsync();
            HookSmokeClose(welcome);
            welcome.ShowFailure(ex);
        }
    }

    /// <summary>创建本次启动唯一的槽位管理器；第二次调用直接抛异常。</summary>
    /// <returns>本次启动唯一的 <see cref="SlotManager"/>，注入、壳面板与 Feature 附着共用它。</returns>
    /// <exception cref="InvalidOperationException">已经创建过槽位管理器。</exception>
    /// <remarks>
    /// <see cref="CompositionGate.TryBegin"/> 已保证编排只进入一次；这里的字段与异常是第二道断言，
    /// 防止后来有人把创建动作移到别的路径而悄悄产生第二个槽位表。
    /// </remarks>
    private SlotManager CreateSlotManagerOnce()
    {
        if (_slotManager is not null)
            throw new InvalidOperationException("一次启动只能创建一个 SlotManager。");

        _slotManager = new SlotManager();
        return _slotManager;
    }

    /// <summary>统一的关停收口：先释放壳体，再停止 Bootstrap；门闩保证只执行一次。</summary>
    /// <remarks>
    /// <c>window.Closed</c>、<c>desktop.Exit</c> 与启动失败三条路径都调用本方法。重复调用返回 <see langword="false"/>
    /// 的 <see cref="CompositionGate.TryStop"/> 分支，不会重复释放壳体，也不会重复停止插件。
    /// </remarks>
    private async Task StopAsync()
    {
        if (!_gate.TryStop())
            return;

        Window? window = Interlocked.Exchange(ref _shellWindow, null);
        if (window is IDisposable disposable)
            disposable.Dispose();

        BootstrapHost? bootstrap = Interlocked.Exchange(ref _bootstrap, null);
        if (bootstrap is null)
            return;

        await DisposeBootstrapAsync(bootstrap);
    }

    /// <summary>停止 Bootstrap 并把关停异常收敛成调试输出。</summary>
    /// <param name="bootstrap">要停止的 Bootstrap，不得为 <see langword="null"/>。</param>
    /// <remarks>关停失败不反向阻断 UI 退出；WinExe 下没有可见日志出口，等日志管线接入后改记日志。</remarks>
    private static async Task DisposeBootstrapAsync(BootstrapHost bootstrap)
    {
        try
        {
            await bootstrap.DisposeAsync();
        }
        catch (Exception ex)
        {
            // ponytail: 关停失败没有日志出口，WinExe 下 stderr 不可见；日志管线接入后改记日志。
            System.Diagnostics.Debug.WriteLine($"Bootstrap 关停失败：{ex}");
        }
    }

    /// <summary>按 <c>LYRE_SMOKE_OPEN_MENU=1</c> 约定在主窗体显示后展开第一个带子项的菜单。</summary>
    /// <param name="window">主窗体，菜单由壳插件在它内部构造。</param>
    /// <remarks>
    /// 存在的理由是一次真实漏测：壳窗口曾继承 Avalonia 原生 <c>Window</c>，在 AtomUI 主题下没有 OverlayLayer，
    /// 点开「文件」子菜单就抛 <c>InvalidOperationException</c>，而当时的冒烟只验证窗口出现与自关，从未展开子菜单。
    /// 本钩子把「菜单子项能打开」纳入冒烟；展开失败时写 stderr 并把进程退出码置 1，让命令行冒烟能判定失败。
    /// </remarks>
    private static void HookSmokeMenuOpen(Window window)
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LYRE_SMOKE_OPEN_MENU"), "1", StringComparison.Ordinal))
            return;

        // ponytail: M1 的冒烟钩子，覆盖“菜单子项 Popup 能打开”；接入真实 UI 测试后删除。
        Dispatcher.UIThread.Post(
            () =>
            {
                try
                {
                    AtomMenuItem? fileItem = window.GetVisualDescendants()
                        .OfType<AtomMenuItem>()
                        .FirstOrDefault(item => item.Items.Count > 0);

                    if (fileItem is null)
                        throw new InvalidOperationException("找不到可展开的菜单项");

                    fileItem.Open();
                    Console.Error.WriteLine("冒烟：菜单子项已展开。");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"冒烟：菜单展开失败：{ex.GetType().Name}：{ex.Message}");
                    Environment.ExitCode = 1;
                }
            },
            DispatcherPriority.Background);
    }

    /// <summary>按 <c>LYRE_SMOKE_LANGUAGE</c> 约定在主窗体显示后切换一次界面语言。</summary>
    /// <param name="window">主窗体，标题在切换完成后读取。</param>
    /// <remarks>
    /// 值是目标语言名，例如 <c>en-US</c>；钩子只在环境变量存在时生效，切换完成后向 stderr 打印实际标题，
    /// 失败时打印异常并把进程退出码置 1。语言切换本身只广播事件，壳与 Feature 各自刷新已有控件，不重建窗口。
    /// </remarks>
    private void HookSmokeLanguage(Window window)
    {
        string? language = Environment.GetEnvironmentVariable("LYRE_SMOKE_LANGUAGE");
        if (string.IsNullOrWhiteSpace(language))
            return;

        // ponytail: 与另外两个冒烟钩子一样，属 M1 冒烟脚手架，接入真实 UI 测试后一起删除。
        Dispatcher.UIThread.Post(
            async () =>
            {
                try
                {
                    await _localization.SetLanguageAsync(language);
                    Dispatcher.UIThread.RunJobs();
                    Console.Error.WriteLine($"冒烟：语言已切换为 {language}，标题={window.Title}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"冒烟：语言切换失败：{ex.GetType().Name}：{ex.Message}");
                    Environment.ExitCode = 1;
                }
            },
            DispatcherPriority.Background);
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
