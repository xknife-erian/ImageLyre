using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Cangming.PicoPlugins.Avalonias.Slots;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Contracts;
using ImageLyre.Module.Shell.Internal;
using ImageLyre.Shell.Contracts;
using AtomMenu = AtomUI.Desktop.Controls.Menu;
using AtomMenuItem = AtomUI.Desktop.Controls.MenuItem;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace ImageLyre.Module.Shell;

/// <summary>M1 的壳窗口：菜单栏、工具栏容器、视口容器、辅助面板容器与带状态栏槽位的底栏。</summary>
/// <remarks>
/// <para>
/// 窗口由 <see cref="ShellModule.Build"/> 在 Avalonia 就绪后构造，构造期先贡献四个一级槽位，再创建四个共享同一
/// <see cref="ISlotManager"/> 的 <c>SlotPanel</c>；因此“取到壳体”等价于“槽位已贡献”，宿主随后才能附着 Feature。
/// </para>
/// <para>
/// 布局只是骨架：视口与辅助面板没有内容时分别显示“无图像”与“尚无测量”空态，工具栏默认为空；
/// 菜单栏只保留一个「文件」顶层，下面依次是壳自有的中文/English 语言切换与一个置灰占位项，
/// 其余菜单项将来由命令注册表平台插件与各 Feature 贡献（G2）。语言项之外的菜单项不挂任何点击命令。
/// </para>
/// <para>
/// 文案：所有可见文本经 <see cref="ILocalizationService.Get"/> 取 <c>lyre.shell.*</c> 资源键；语言项只调用
/// <see cref="ILocalizationService.SetLanguageAsync"/>，<see cref="ILocalizationService.LanguageChanged"/> 回调只
/// <c>Post</c> 到 UI 线程逐项改 <c>Text</c>/<c>Header</c>，不重建窗口或控件树。M1 只有中英两套文案，更多语言由后续任务扩资源源。
/// </para>
/// <para>
/// 生命周期：窗口关闭后由宿主调用 <see cref="Dispose"/> 退订四个 <c>SlotPanel</c> 的槽位事件与本地化事件；
/// M1 不提供运行期摘除附着物，控件树随窗口销毁由 Avalonia 统一回收。
/// </para>
/// <para>
/// 基类必须是 <c>AtomUI.Desktop.Controls.Window</c>，不能改回 <c>Avalonia.Controls.Window</c>：
/// Avalonia 原生 Window 在 AtomUI 主题下没有 OverlayLayer，打开「文件」子菜单或任何 Popup 时会抛
/// <c>InvalidOperationException: Unable to create IPopupImpl and no overlay layer is found for the target control</c>。
/// 换 UI 路线或窗口基类前，先确认新基类提供 OverlayLayer。
/// </para>
/// </remarks>
public sealed class ShellWindow : AtomUIWindow, IWorkbenchShell
{
    private readonly ILocalizationService _localization;
    private readonly ISlotManager _slotManager;
    private readonly SlotPanel _toolbarPanel;
    private readonly ViewportSlotPanel _viewportPanel;
    private readonly DockPaneSlotPanel _panel;
    private SlotPanel _statusBarPanel = null!;

    private AtomMenuItem _fileMenu = null!;
    private AtomMenuItem _languageZhItem = null!;
    private AtomMenuItem _languageEnItem = null!;
    private AtomMenuItem _placeholderItem = null!;
    private (string Key, AtomTextBlock Block)[] _statusSegments = [];
    private AtomTextBlock _readyText = null!;
    private bool _disposed;

    /// <summary>按已注入的上下文构造壳窗口并贡献四个一级槽位。</summary>
    /// <param name="context">壳插件上下文，必须已完成槽位管理器与 IoC 注入。</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException">槽位管理器或本地化服务尚未注入。</exception>
    /// <remarks>
    /// 构造必须在 UI 线程执行，因为 <c>SlotPanel</c> 会创建控件并订阅槽位事件；
    /// 四个 <c>SlotPanel</c> 共用 <paramref name="context"/> 里的同一个 <see cref="ISlotManager"/> 实例，不各自创建。
    /// </remarks>
    internal ShellWindow(ShellModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _slotManager = context.SlotManager;
        _localization = context.Localization;
        ContributeSlots(context.Registry);

        Title = _localization.Get(ShellStringKeys.WindowTitle);
        Width = 1280;
        Height = 800;
        MinWidth = 960;
        MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _toolbarPanel = CreateToolbarPanel(_slotManager);
        _viewportPanel = new ViewportSlotPanel(_slotManager, _localization);
        _panel = new DockPaneSlotPanel(_slotManager, _localization);

        AtomMenu menu = BuildMenu();
        Content = BuildLayout(menu);
        _localization.LanguageChanged += OnLanguageChanged;
    }

    /// <summary>退订槽位事件与本地化事件，释放对槽位管理器和本地化服务的引用。</summary>
    /// <remarks>
    /// 控件树在窗口关闭时由 Avalonia 回收，本方法只负责事件退订；重复调用被 <c>_disposed</c> 挡住，
    /// 因此不会重复退订，也不会在关闭后继续刷新文案。
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _localization.LanguageChanged -= OnLanguageChanged;
        _toolbarPanel.SlotManager = null;
        _viewportPanel.SlotManager = null;
        _panel.SlotManager = null;
        _statusBarPanel.SlotManager = null;
    }

    /// <summary>语言切换后把文本刷新投递到 UI 线程，不重建窗口。</summary>
    /// <param name="sender">事件源，本方法不使用。</param>
    /// <param name="e">事件参数，本方法不使用。</param>
    /// <remarks>
    /// <see cref="ILocalizationService.LanguageChanged"/> 可能在任意线程触发，控件只能在 UI 线程改；
    /// <see cref="Dispatcher.UIThread.Post(Action)"/> 排到当前 UI 队列，窗口关闭后由 <c>_disposed</c> 挡住。
    /// </remarks>
    private void OnLanguageChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(RefreshTexts);

    /// <summary>按当前语言逐项改写已有控件的文本，不创建或替换任何控件。</summary>
    /// <remarks>
    /// 「文件」菜单的顶层与语言项、占位项、状态栏五段与就绪消息、视口/面板空态都只改 <c>Text</c>/<c>Header</c>；
    /// 当前语言项通过 <see cref="AtomMenuItem.IsEnabled"/> 置灰，真实视口内容与页签标题属于 Feature，不在这里改写。
    /// </remarks>
    private void RefreshTexts()
    {
        if (_disposed)
            return;

        Title = _localization.Get(ShellStringKeys.WindowTitle);
        _fileMenu.Header = _localization.Get(ShellStringKeys.MenuFile);
        _languageZhItem.Header = _localization.Get(ShellStringKeys.MenuLanguageZh);
        _languageEnItem.Header = _localization.Get(ShellStringKeys.MenuLanguageEn);
        _placeholderItem.Header = _localization.Get(ShellStringKeys.MenuPlaceholderItem);
        RefreshLanguageMenuState();

        foreach ((string key, AtomTextBlock block) in _statusSegments)
            block.Text = FormatStatus(key);

        _readyText.Text = _localization.Get(ShellStringKeys.StatusReady);
        _viewportPanel.RefreshTexts();
        _panel.RefreshTexts();
    }

    /// <summary>把当前语言对应的菜单项置灰，另一项保持可点击。</summary>
    /// <remarks>
    /// AtomUI 的 <c>MenuItem</c> 支持 <see cref="Control.IsEnabled"/>，这里用它表达“当前语言”而不引入新控件；
    /// 当前语言不是中英任意一项时两项都保持可用。
    /// </remarks>
    private void RefreshLanguageMenuState()
    {
        string current = _localization.Current.Name;
        _languageZhItem.IsEnabled = !string.Equals(current, "zh-CN", StringComparison.Ordinal);
        _languageEnItem.IsEnabled = !string.Equals(current, "en-US", StringComparison.Ordinal);
    }

    /// <summary>贡献工具栏、视口、辅助面板与状态栏四个一级槽位。</summary>
    /// <param name="registry">槽位管理器提供的注册面。</param>
    /// <remarks>
    /// 四个槽位只在窗口构造期各贡献一次；<see cref="ShellModule.Build"/> 用 <see cref="Lazy{T}"/> 缓存窗口，
    /// 重复取壳不会重复执行这里，因此不会触发 <c>SlotManager</c> 的重名异常。
    /// </remarks>
    private static void ContributeSlots(IAttachmentRegistry registry)
    {
        registry.ContributeSlot(ShellSlots.Toolbar, "工具栏");
        registry.ContributeSlot(ShellSlots.Viewport, "图像视口");
        registry.ContributeSlot(ShellSlots.Panel, "辅助面板");
        registry.ContributeSlot(ShellSlots.StatusBar, "状态栏");
    }

    /// <summary>创建绑定工具栏槽位的面板。</summary>
    /// <param name="slotManager">与另外三个面板共享的槽位管理器。</param>
    /// <returns>横向排列的工具栏槽位面板。</returns>
    private static SlotPanel CreateToolbarPanel(ISlotManager slotManager) => new()
    {
        SlotName = ShellSlots.Toolbar,
        SlotManager = slotManager,
        Orientation = Orientation.Horizontal,
        MinHeight = 36,
    };

    /// <summary>创建状态栏中段的插件读数槽位。</summary>
    /// <param name="slotManager">与另外三个面板共享的槽位管理器。</param>
    /// <returns>横向排列的状态栏槽位面板。</returns>
    /// <remarks>
    /// 状态栏槽位承载插件贡献的读数片段；M1 没有读数插件，槽位保持为空，左右两侧仍由壳的占位与临时消息构成。
    /// </remarks>
    private static SlotPanel CreateStatusBarPanel(ISlotManager slotManager) => new()
    {
        SlotName = ShellSlots.StatusBar,
        SlotManager = slotManager,
        Orientation = Orientation.Horizontal,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(12, 0),
    };

    /// <summary>拼装窗口的 Dock 布局：菜单与工具栏在上、状态栏在下、视口与辅助面板填满中间。</summary>
    /// <param name="menu">已构造好的「文件」菜单。</param>
    /// <returns>窗口根控件。</returns>
    /// <remarks>
    /// 辅助面板固定占 320 像素宽，布局变更只改这里；M1 不提供可拖拽分隔条，也不提供面板收起。
    /// </remarks>
    private Control BuildLayout(AtomMenu menu)
    {
        var toolbar = new Border
        {
            Child = _toolbarPanel,
            Padding = new Thickness(8, 4),
            BorderThickness = new Thickness(0, 0, 0, 1),
            BorderBrush = Brushes.LightGray,
        };

        var viewport = new Border
        {
            Child = _viewportPanel,
        };

        var panel = new Border
        {
            Child = _panel,
            MinWidth = 320,
            BorderThickness = new Thickness(1, 0, 0, 0),
            BorderBrush = Brushes.LightGray,
        };

        Grid.SetColumn(panel, 1);

        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,320"),
        };

        body.Children.Add(viewport);
        body.Children.Add(panel);

        Border status = BuildStatusBar();

        var root = new DockPanel();
        DockPanel.SetDock(menu, Dock.Top);
        root.Children.Add(menu);
        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);
        DockPanel.SetDock(status, Dock.Bottom);
        root.Children.Add(status);
        root.Children.Add(body);

        return root;
    }

    /// <summary>构造「文件」菜单：语言切换两项加一个置灰占位项，只有语言项挂点击动作。</summary>
    /// <returns>菜单栏控件。</returns>
    /// <remarks>
    /// 菜单条只是壳的容器与外观；「文件」菜单里的语言切换是壳自有的非插件功能，其余菜单项将来由命令注册表
    /// 平台插件与各 Feature 贡献（G2）。M1 不放真实命令菜单，也不在壳里预置八个顶层。
    /// </remarks>
    private AtomMenu BuildMenu()
    {
        // ponytail: 菜单条只是壳的容器与外观，真实菜单项将来由命令注册表平台插件与各 Feature 贡献（G2）；
        // M1 只放一个「文件」顶层、两个语言项与一个置灰占位项。
        _fileMenu = new AtomMenuItem
        {
            Header = _localization.Get(ShellStringKeys.MenuFile),
        };

        // ponytail: 语言菜单项是壳自有的最小实现，只有这两项挂 Click；更多语言或正式语言菜单归后续任务。
        _languageZhItem = new AtomMenuItem
        {
            Header = _localization.Get(ShellStringKeys.MenuLanguageZh),
        };
        _languageZhItem.Click += async (_, _) => await _localization.SetLanguageAsync("zh-CN");

        _languageEnItem = new AtomMenuItem
        {
            Header = _localization.Get(ShellStringKeys.MenuLanguageEn),
        };
        _languageEnItem.Click += async (_, _) => await _localization.SetLanguageAsync("en-US");

        _placeholderItem = new AtomMenuItem
        {
            Header = _localization.Get(ShellStringKeys.MenuPlaceholderItem),
            IsEnabled = false,
        };

        _fileMenu.Items.Add(_languageZhItem);
        _fileMenu.Items.Add(_languageEnItem);
        _fileMenu.Items.Add(_placeholderItem);
        RefreshLanguageMenuState();

        var menu = new AtomMenu();
        menu.Items.Add(_fileMenu);
        return menu;
    }

    /// <summary>构造状态栏：左侧五段规格占位、中间插件读数槽位、右侧临时消息区。</summary>
    /// <returns>状态栏控件。</returns>
    /// <remarks>
    /// 五段依次是坐标、像素值、位深、缩放与校准单位；M1 没有真实读数插件，因此标签后只接占位值。
    /// 中段绑定 <see cref="ShellSlots.StatusBar"/>，供插件贡献读数片段；左右两侧是壳的固定内容。
    /// </remarks>
    private Border BuildStatusBar()
    {
        // ponytail: 五段占位是交互规格 §2 要求的 M1 占位；真实读数由独立插件贡献到 statusbar 槽位，
        // 第一批读数插件接入后撤掉壳的占位。
        var segments = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
        };

        _statusSegments =
        [
            CreateStatusSegment(ShellStringKeys.StatusCoordinate),
            CreateStatusSegment(ShellStringKeys.StatusPixel),
            CreateStatusSegment(ShellStringKeys.StatusDepth),
            CreateStatusSegment(ShellStringKeys.StatusZoom),
            CreateStatusSegment(ShellStringKeys.StatusCalibration),
        ];

        foreach (var (_, block) in _statusSegments)
            segments.Children.Add(block);

        _statusBarPanel = CreateStatusBarPanel(_slotManager);

        _readyText = new AtomTextBlock
        {
            Text = _localization.Get(ShellStringKeys.StatusReady),
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
        };

        grid.Children.Add(segments);
        Grid.SetColumn(_statusBarPanel, 1);
        grid.Children.Add(_statusBarPanel);
        Grid.SetColumn(_readyText, 2);
        grid.Children.Add(_readyText);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(12, 4),
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = Brushes.LightGray,
        };
    }

    /// <summary>创建一个状态栏段的文本控件。</summary>
    /// <param name="labelKey">标签资源键。</param>
    /// <returns>键与该文本控件的配对，供语言切换时原地改 <c>Text</c>。</returns>
    private (string Key, AtomTextBlock Block) CreateStatusSegment(string labelKey) =>
        (labelKey, new AtomTextBlock
        {
            Text = FormatStatus(labelKey),
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center,
        });

    /// <summary>把状态栏标签与占位值拼成显示文本。</summary>
    /// <param name="labelKey">标签资源键。</param>
    /// <returns>形如 <c>坐标 --</c> 的占位文本。</returns>
    private string FormatStatus(string labelKey) => $"{_localization.Get(labelKey)} --";
}
