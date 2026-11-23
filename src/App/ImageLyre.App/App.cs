using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ImageLyre.App.Startup;
using ImageLyre.Contracts;

namespace ImageLyre.App;

/// <summary>ImageLyre 的 Avalonia 应用，M1 只把欢迎占位窗口挂到桌面主窗口。</summary>
/// <remarks>
/// <para>
/// 宿主分派在 <see cref="Program.Main"/> 完成，本类因此只在桌面分支被构造；它不驱动 Bootstrap、不取壳、不附着 Feature，
/// 那些编排属于 T12，M1 的欢迎窗口是固定语义区域，由 App 直接构造而不是槽位。
/// </para>
/// <para>
/// 生命周期：每个进程一个实例，随桌面生命周期创建与销毁；<see cref="OnFrameworkInitializationCompleted"/> 只设置
/// <see cref="IClassicDesktopStyleApplicationLifetime.MainWindow"/>，窗口显示由经典桌面生命周期在启动时完成，这里不重复 <c>Show</c>。
/// </para>
/// </remarks>
public sealed partial class App : Application
{
    private readonly StartupOptions _options;
    private readonly ILocalizationService _localization;

    /// <summary>创建应用实例。</summary>
    /// <param name="options">启动参数解析结果，用于约束无界面分支不得进入本类。</param>
    /// <param name="localization">本地化服务，欢迎窗口与将来的主界面共用同一实例。</param>
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
            HookSmokeClose(welcome);
            desktop.MainWindow = welcome;
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>按 <c>LYRE_SMOKE_CLOSE_MS</c> 约定在窗口打开后定时关闭它。</summary>
    /// <param name="window">要监听的窗口。</param>
    /// <remarks>环境变量缺失、不是整数或不为正数时什么都不做；定时器只在 UI 线程上创建与启动。</remarks>
    private static void HookSmokeClose(Window window)
    {
        string? raw = Environment.GetEnvironmentVariable("LYRE_SMOKE_CLOSE_MS");
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int delay) || delay <= 0)
            return;

        // ponytail: 这是 M1 的自动冒烟钩子，等接入真实 UI 测试后删除，不要把它当成产品特性。
        window.Opened += (_, _) =>
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delay) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                window.Close();
            };
            timer.Start();
        };
    }
}
