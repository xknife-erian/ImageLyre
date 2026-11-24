using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ImageLyre.Contracts;
using ImageLyre.Presentation.Desktop.Localization;
using AtomCard = AtomUI.Desktop.Controls.Card;
using AtomSpace = AtomUI.Desktop.Controls.Space;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace ImageLyre.Presentation.Desktop;

/// <summary>启动期显示的欢迎窗口：产品名、程序集版本与一行状态文本。</summary>
/// <remarks>
/// <para>
/// 窗口骨架必须在插件系统就绪之前存在，因此完全由 <see cref="Startup.DesktopStartup"/> 构造、不依赖任何插件或槽位；
/// 文案全部经 <see cref="ILocalizationService.Get"/> 取得，控件走 AtomUI 的默认主题，从而与将来壳窗口的视觉基线一致。
/// </para>
/// <para>
/// 生命周期：实例由桌面入口创建并挂到桌面主窗口，主窗体显示后由 PresentationPlan 的显示组关闭；M1 不订阅
/// <see cref="ILocalizationService.LanguageChanged"/>，欢迎界面是静态占位，语言切换后的刷新留给 T15 的壳文案任务。
/// </para>
/// </remarks>
public sealed class WelcomeWindow : Window
{
    private readonly ILocalizationService _localization;
    private readonly AtomTextBlock _failureText;

    /// <summary>创建一个欢迎窗口。</summary>
    /// <param name="localization">文案服务，窗口内的每个字符串都从它取得。</param>
    /// <exception cref="ArgumentNullException"><paramref name="localization"/> 为 <see langword="null"/>。</exception>
    /// <remarks>构造必须在 UI 线程执行；本类不创建线程，也不持有需要释放的资源。</remarks>
    public WelcomeWindow(ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(localization);

        _localization = localization;
        string product = localization.Get(AppStringKeys.Product);

        _failureText = new AtomTextBlock
        {
            IsVisible = false,
            Foreground = Brushes.Firebrick,
            TextWrapping = TextWrapping.Wrap,
        };

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
                    _failureText,
                },
            },
        };

        // ponytail: 欢迎界面是 M1 的占位，内容区将来由身份类 Feature 填充；接入后替换卡片布局并改为按槽位渲染。
    }

    /// <summary>在欢迎窗口内显示启动失败原因。</summary>
    /// <param name="exception">失败异常，消息会原样附在本地化前缀之后。</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> 为 <see langword="null"/>。</exception>
    /// <remarks>
    /// 必须在 UI 线程调用；文案先经 <see cref="ILocalizationService.Get"/> 取 <c>lyre.app.failed</c>，
    /// 缺键（返回键名）时退回中文“启动失败：”，异常消息始终保留，避免启动期只剩日志。
    /// 本方法只更新失败文本并置为可见，不重建窗口布局。
    /// </remarks>
    public void ShowFailure(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string prefix = _localization.Get(AppStringKeys.Failed);
        if (string.Equals(prefix, AppStringKeys.Failed, StringComparison.Ordinal))
            prefix = "启动失败：";

        _failureText.Text = $"{prefix}{Environment.NewLine}{exception.Message}";
        _failureText.IsVisible = true;

        // ponytail: 失败态只是欢迎窗口里的一段文本占位，后续替换为带退出/重试操作的可诊断界面。
    }

    /// <summary>取当前程序集的四段版本号，取不到时退回 <c>0.0.0.0</c>。</summary>
    /// <returns>形如 <c>1.0.0.0</c> 的版本文本。</returns>
    private static string GetAssemblyVersion() =>
        typeof(WelcomeWindow).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
}
