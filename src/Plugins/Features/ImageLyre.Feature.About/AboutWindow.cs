using System.Reflection;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using ImageLyre.Contracts;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace ImageLyre.Feature.About;

/// <summary>「关于」窗口：产品、版本、运行时、操作系统与许可证，全部为只读信息。</summary>
/// <remarks>
/// <para>
/// 窗口由 Feature 在菜单点击时创建，不进入槽位、不参与附着；关闭即释放，不缓存控件。
/// 语言切换只改标签与标题的 <c>Text</c>/<c>Title</c>，不重建窗口。
/// </para>
/// <para>
/// 基类用 AtomUI 的 <c>Window</c>：本窗当前没有 Popup，但它与壳窗口保持同一条 UI 路线，
/// 将来若要在这里加下拉或对话框，不必再换基类。
/// </para>
/// </remarks>
internal sealed class AboutWindow : AtomUIWindow
{
    private readonly ILocalizationService _localization;
    private readonly AtomTextBlock _productLabel;
    private readonly AtomTextBlock _versionLabel;
    private readonly AtomTextBlock _runtimeLabel;
    private readonly AtomTextBlock _osLabel;
    private readonly AtomTextBlock _licenseLabel;
    private readonly AtomTextBlock _copyright;
    private readonly AtomButton _closeButton;

    /// <summary>创建一个只读的关于窗口。</summary>
    /// <param name="localization">本地化服务，用于取标签文案并订阅语言切换。</param>
    /// <exception cref="ArgumentNullException"><paramref name="localization"/> 为 <see langword="null"/>。</exception>
    public AboutWindow(ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(localization);
        _localization = localization;

        Title = _localization.Get(AboutStringKeys.WindowTitle);
        Width = 520;
        Height = 360;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        _productLabel = CreateLabel();
        _versionLabel = CreateLabel();
        _runtimeLabel = CreateLabel();
        _osLabel = CreateLabel();
        _licenseLabel = CreateLabel();
        _copyright = new AtomTextBlock
        {
            Opacity = 0.6,
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap,
        };

        _closeButton = new AtomButton
        {
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        _closeButton.Click += (_, _) => Close();

        var panel = new StackPanel
        {
            Margin = new Thickness(24),
            Spacing = 8,
        };

        panel.Children.Add(new AtomTextBlock
        {
            Text = "ImageLyre",
            FontSize = 22,
            FontWeight = FontWeight.SemiBold,
        });
        panel.Children.Add(CreateRow(_productLabel, "ImageLyre"));
        panel.Children.Add(CreateRow(_versionLabel, ResolveVersion()));
        panel.Children.Add(CreateRow(_runtimeLabel, RuntimeInformation.FrameworkDescription));
        panel.Children.Add(CreateRow(_osLabel, RuntimeInformation.OSDescription));
        panel.Children.Add(CreateRow(_licenseLabel, _localization.Get(AboutStringKeys.LicenseName)));
        panel.Children.Add(_copyright);
        panel.Children.Add(_closeButton);

        Content = panel;
        RefreshTexts();

        _localization.LanguageChanged += OnLanguageChanged;
        Closed += OnClosed;
    }

    private static AtomTextBlock CreateLabel() => new()
    {
        Width = 110,
        Opacity = 0.7,
        VerticalAlignment = VerticalAlignment.Top,
        TextWrapping = TextWrapping.Wrap,
    };

    private static Control CreateRow(AtomTextBlock label, string value)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("120,*"),
        };

        grid.Children.Add(label);

        var valueBlock = new AtomTextBlock
        {
            Text = value,
            TextWrapping = TextWrapping.Wrap,
        };
        Grid.SetColumn(valueBlock, 1);
        grid.Children.Add(valueBlock);

        return grid;
    }

    private static string ResolveVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? typeof(AboutWindow).Assembly;
        string? informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        return string.IsNullOrWhiteSpace(informational)
                   ? assembly.GetName().Version?.ToString() ?? "unknown"
                   : informational;
    }

    private void OnLanguageChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(RefreshTexts);

    private void OnClosed(object? sender, EventArgs e) =>
        _localization.LanguageChanged -= OnLanguageChanged;

    private void RefreshTexts()
    {
        Title = _localization.Get(AboutStringKeys.WindowTitle);
        _productLabel.Text = _localization.Get(AboutStringKeys.LabelProduct);
        _versionLabel.Text = _localization.Get(AboutStringKeys.LabelVersion);
        _runtimeLabel.Text = _localization.Get(AboutStringKeys.LabelRuntime);
        _osLabel.Text = _localization.Get(AboutStringKeys.LabelOperatingSystem);
        _licenseLabel.Text = _localization.Get(AboutStringKeys.LabelLicense);
        _copyright.Text = _localization.Get(AboutStringKeys.Copyright);
        _closeButton.Content = _localization.Get(AboutStringKeys.Close);
    }
}
