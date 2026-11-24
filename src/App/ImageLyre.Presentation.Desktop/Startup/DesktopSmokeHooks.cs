using System.Globalization;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ImageLyre.Contracts;
using AtomMenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>桌面宿主级冒烟回调：靠环境变量驱动窗口自关、菜单展开与语言切换，挂在 PresentationPlan 之外。</summary>
/// <remarks>
/// <para>
/// 三个回调都不进入 Plan：它们是 M1 的测试脚手架，不是产品启动语义；接入真实 UI 测试后整体删除，
/// 因此保留原来的环境变量、stderr 文本与退出码语义，让 <c>scripts/qa/smoke-m1.ps1</c> 的断言在迁移前后不变。
/// 环境变量缺失或取值非法时回调什么都不做，不改变正常启动路径。
/// </para>
/// <para>
/// 线程：三个回调都由 <see cref="DesktopStartup"/> 在 Avalonia UI 线程上调用，内部再用 <see cref="Dispatcher.UIThread"/> 的
/// 后台优先级排队，避免与窗口首次显示的布局争抢同一帧。
/// </para>
/// </remarks>
internal static class DesktopSmokeHooks
{
    /// <summary>按 <c>LYRE_SMOKE_OPEN_MENU=1</c> 约定在主窗体显示后展开第一个带子项的菜单。</summary>
    /// <param name="window">主窗体，菜单由壳插件在它内部构造。</param>
    /// <remarks>
    /// 存在的理由是一次真实漏测：壳窗口曾继承 Avalonia 原生 <c>Window</c>，在 AtomUI 主题下没有 OverlayLayer，
    /// 点开「文件」子菜单就抛 <c>InvalidOperationException</c>，而当时的冒烟只验证窗口出现与自关，从未展开子菜单。
    /// 本钩子把「菜单子项能打开」纳入冒烟；展开失败时写 stderr 并把进程退出码置 1，让命令行冒烟能判定失败。
    /// </remarks>
    internal static void OpenMenu(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

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
    /// <param name="localization">本次启动共用的本地化服务，切换对它广播。</param>
    /// <remarks>
    /// 值是目标语言名，例如 <c>en-US</c>；钩子只在环境变量存在时生效，切换完成后向 stderr 打印实际标题，
    /// 失败时打印异常并把进程退出码置 1。语言切换本身只广播事件，壳与 Feature 各自刷新已有控件，不重建窗口。
    /// </remarks>
    internal static void SwitchLanguage(Window window, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(localization);

        string? language = Environment.GetEnvironmentVariable("LYRE_SMOKE_LANGUAGE");
        if (string.IsNullOrWhiteSpace(language))
            return;

        // ponytail: 与另外两个冒烟钩子一样，属 M1 冒烟脚手架，接入真实 UI 测试后一起删除。
        Dispatcher.UIThread.Post(
            async () =>
            {
                try
                {
                    await localization.SetLanguageAsync(language);
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
    internal static void CloseAfterDelay(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);

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
