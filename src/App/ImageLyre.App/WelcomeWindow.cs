using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ImageLyre.App.Localization;
using ImageLyre.App.Startup;
using ImageLyre.Contracts;
using AtomCard = AtomUI.Desktop.Controls.Card;
using AtomSpace = AtomUI.Desktop.Controls.Space;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace ImageLyre.App;

/// <summary>启动期显示的欢迎窗口：产品名、程序集版本与一行状态文本。</summary>
/// <remarks>
/// <para>
/// 窗口骨架必须在插件系统就绪之前存在，因此完全由 App 构造、不依赖任何插件或槽位；文案全部经 <see cref="ILocalizationService.Get"/> 取得，
/// 控件走 AtomUI 的默认主题，从而与将来壳窗口的视觉基线一致。
/// </para>
/// <para>
/// 生命周期：实例由 <see cref="App"/> 创建并挂到桌面主窗口，关闭即结束进程；M1 不订阅 <see cref="ILocalizationService.LanguageChanged"/>，
/// 欢迎界面是静态占位，语言切换后的刷新留给 T15 的壳文案任务。
/// </para>
/// </remarks>
public sealed class WelcomeWindow : Window
{
    /// <summary>创建一个欢迎窗口。</summary>
    /// <param name="localization">文案服务，窗口内的每个字符串都从它取得。</param>
    /// <param name="options">启动参数解析结果，只用于断言当前不是无界面分支。</param>
    /// <exception cref="ArgumentNullException"><paramref name="localization"/> 或 <paramref name="options"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException"><paramref name="options"/> 表示无界面分支。</exception>
    /// <remarks>构造必须在 UI 线程执行；本类不创建线程，也不持有需要释放的资源。</remarks>
    public WelcomeWindow(ILocalizationService localization, StartupOptions options)
    {
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(options);

        if (options.Headless)
            throw new InvalidOperationException("无界面分支不得创建欢迎窗口。");

        string product = localization.Get(AppStringKeys.Product);

        Title = product;
        Width = 520;
        Height = 300;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = new AtomCard
        {
            Margin = new Thickness(24),
            Content = new AtomSpace
            {
                Orientation = Orientation.Vertical,
                ItemSpacing = 10,
                Children =
                {
                    new AtomTextBlock
                    {
                        Text = product,
                        FontSize = 28,
                        FontWeight = FontWeight.SemiBold,
                    },
                    new AtomTextBlock
                    {
                        Text = $"{localization.Get(AppStringKeys.Version)} {GetAssemblyVersion()}",
                        Opacity = 0.72,
                    },
                    new AtomTextBlock
                    {
                        Text = localization.Get(AppStringKeys.Status),
                        TextWrapping = TextWrapping.Wrap,
                    },
                },
            },
        };

        // ponytail: 欢迎界面是 M1 的占位，内容区将来由身份类 Feature 填充；接入后替换卡片布局并改为按槽位渲染。
    }

    /// <summary>取当前程序集的四段版本号，取不到时退回 <c>0.0.0.0</c>。</summary>
    /// <returns>形如 <c>1.0.0.0</c> 的版本文本。</returns>
    private static string GetAssemblyVersion() =>
        typeof(WelcomeWindow).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
}
