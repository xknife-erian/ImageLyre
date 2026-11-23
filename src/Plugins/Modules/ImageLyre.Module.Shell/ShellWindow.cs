using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Cangming.PicoPlugins.Avalonias.Slots;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Module.Shell.Internal;
using ImageLyre.Shell.Contracts;
using AtomMenu = AtomUI.Desktop.Controls.Menu;
using AtomMenuItem = AtomUI.Desktop.Controls.MenuItem;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace ImageLyre.Module.Shell;

/// <summary>M1 的壳窗口：菜单栏、工具栏容器、视口容器、辅助面板容器与状态栏五段占位。</summary>
/// <remarks>
/// <para>
/// 窗口由 <see cref="ShellModule.Build"/> 在 Avalonia 就绪后构造，构造期先贡献三个一级槽位，再创建三个共享同一
/// <see cref="ISlotManager"/> 的 <c>SlotPanel</c>；因此“取到壳体”等价于“槽位已贡献”，宿主随后才能附着 Feature。
/// </para>
/// <para>
/// 布局只是骨架：视口与辅助面板没有内容时分别显示“无图像”与“尚无测量”空态，工具栏默认为空；
/// 菜单栏的八个顶层按设计固定，M1 只启用 File→退出，其余菜单项置灰。壳不订阅本地化事件，文案是 M1 的中文字面量，
/// T15 再把它们换成 <c>lyre.shell.*</c> 资源键。
/// </para>
/// <para>
/// 生命周期：窗口关闭后由宿主调用 <see cref="Dispose"/> 退订三个面板的槽位事件；M1 不提供运行期摘除附着物，
/// 控件树随窗口销毁由 Avalonia 统一回收。
/// </para>
/// </remarks>
public sealed class ShellWindow : Window, IWorkbenchShell
{
    private readonly SlotPanel _toolbarPanel;
    private readonly SlotPanel _viewportPanel;
    private readonly SlotPanel _panel;

    /// <summary>按已注入的上下文构造壳窗口并贡献三个一级槽位。</summary>
    /// <param name="context">壳插件上下文，必须已完成槽位管理器注入。</param>
    /// <exception cref="ArgumentNullException"><paramref name="context"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException">槽位管理器尚未注入，或其实现不包含 <see cref="IAttachmentRegistry"/>。</exception>
    /// <remarks>
    /// 构造必须在 UI 线程执行，因为 <c>SlotPanel</c> 会创建控件并订阅槽位事件；
    /// 三个 <c>SlotPanel</c> 共用 <paramref name="context"/> 里的同一个 <see cref="ISlotManager"/> 实例，不各自创建。
    /// </remarks>
    internal ShellWindow(ShellModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        ISlotManager slotManager = context.SlotManager;
        ContributeSlots(context.Registry);

        // ponytail: 壳文案资源化归 T15，键前缀 lyre.shell.*；M1 先用中文字面量。
        Title = "ImageLyre 工作台";
        Width = 1280;
        Height = 800;
        MinWidth = 960;
        MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        _toolbarPanel = CreateToolbarPanel(slotManager);
        _viewportPanel = new ViewportSlotPanel(slotManager);
        _panel = new DockPaneSlotPanel(slotManager);

        // ponytail: M1 不含语言切换入口，T15 在标题栏补。
        Content = BuildLayout();
    }

    /// <summary>退订三个面板的槽位内容变化事件，释放对槽位管理器的引用。</summary>
    /// <remarks>
    /// 控件树在窗口关闭时由 Avalonia 回收，本方法只负责事件退订；重复调用把已置空的
    /// <see cref="SlotPanel.SlotManager"/> 再置空一次，因此幂等。宿主不在关闭后继续访问壳体。
    /// </remarks>
    public void Dispose()
    {
        _toolbarPanel.SlotManager = null;
        _viewportPanel.SlotManager = null;
        _panel.SlotManager = null;
    }

    /// <summary>贡献工具栏、视口与辅助面板三个一级槽位。</summary>
    /// <param name="registry">槽位管理器提供的注册面。</param>
    /// <remarks>
    /// 三个槽位只在窗口构造期各贡献一次；<see cref="ShellModule.Build"/> 用 <see cref="Lazy{T}"/> 缓存窗口，
    /// 重复取壳不会重复执行这里，因此不会触发 <c>SlotManager</c> 的重名异常。
    /// </remarks>
    private static void ContributeSlots(IAttachmentRegistry registry)
    {
        registry.ContributeSlot(ShellSlots.Toolbar, "工具栏");
        registry.ContributeSlot(ShellSlots.Viewport, "图像视口");
        registry.ContributeSlot(ShellSlots.Panel, "辅助面板");
    }

    /// <summary>创建绑定工具栏槽位的面板。</summary>
    /// <param name="slotManager">与另外两个面板共享的槽位管理器。</param>
    /// <returns>横向排列的工具栏槽位面板。</returns>
    private static SlotPanel CreateToolbarPanel(ISlotManager slotManager) => new()
    {
        SlotName = ShellSlots.Toolbar,
        SlotManager = slotManager,
        Orientation = Orientation.Horizontal,
        MinHeight = 36,
    };

    /// <summary>拼装窗口的 Dock 布局：菜单与工具栏在上、状态栏在下、视口与辅助面板填满中间。</summary>
    /// <returns>窗口根控件。</returns>
    /// <remarks>
    /// 辅助面板固定占 320 像素宽，布局变更只改这里；M1 不提供可拖拽分隔条，也不提供面板收起。
    /// </remarks>
    private Control BuildLayout()
    {
        AtomMenu menu = BuildMenu();

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

    /// <summary>构造八个顶层菜单；M1 只有 File→退出可用，其余项置灰并标注阶段。</summary>
    /// <returns>菜单栏控件。</returns>
    /// <remarks>
    /// 命令注册表、键位与可用条件归 G2，届时菜单项改为从命令表生成；现在不预留命令对象，
    /// 也不把菜单项的文本放进本地化服务。
    /// </remarks>
    private AtomMenu BuildMenu()
    {
        // ponytail: 菜单项置灰，M1 只启用 File→退出；命令注册表与其余命令归 G2。
        var menu = new AtomMenu();
        menu.Items.Add(CreateTopLevel("File", CreateExitItem()));
        menu.Items.Add(CreateTopLevel("Edit", CreateDisabledItem("（M1 未接入）")));
        menu.Items.Add(CreateTopLevel("Image", CreateDisabledItem("（M1 未接入）")));
        menu.Items.Add(CreateTopLevel("Process", CreateDisabledItem("（M1 未接入）")));
        menu.Items.Add(CreateTopLevel("Analyze", CreateDisabledItem("（M1 未接入）")));
        menu.Items.Add(CreateTopLevel("Plugins", CreateDisabledItem("（G2 接入插件面板）")));
        menu.Items.Add(CreateTopLevel("Window", CreateDisabledItem("（M1 未接入）")));
        menu.Items.Add(CreateTopLevel("Help", CreateDisabledItem("（T15 接入关于）")));

        return menu;
    }

    /// <summary>创建一个带单个子项的顶层菜单。</summary>
    /// <param name="header">顶层菜单文本。</param>
    /// <param name="children">子项。</param>
    /// <returns>顶层菜单项。</returns>
    private static AtomMenuItem CreateTopLevel(string header, params Control[] children)
    {
        var item = new AtomMenuItem { Header = header };
        foreach (Control child in children)
            item.Items.Add(child);

        return item;
    }

    /// <summary>创建可用的退出项，点击后关闭主窗体。</summary>
    /// <returns>退出菜单项。</returns>
    private AtomMenuItem CreateExitItem()
    {
        var item = new AtomMenuItem { Header = "退出" };
        item.Click += (_, _) => Close();

        return item;
    }

    /// <summary>创建一个置灰的菜单项。</summary>
    /// <param name="header">菜单项文本。</param>
    /// <returns>不可用的菜单项。</returns>
    private static AtomMenuItem CreateDisabledItem(string header) => new()
    {
        Header = header,
        IsEnabled = false,
    };

    /// <summary>构造状态栏五段占位与最右侧的临时消息区。</summary>
    /// <returns>状态栏控件。</returns>
    /// <remarks>
    /// 五段依次是坐标、像素值、位深、缩放与校准单位；M1 没有图像生产者，因此全部显示为占位值。
    /// 段位值是固定语义，不走槽位；将来接真实状态时改这里的绑定，不改槽位契约。
    /// </remarks>
    private static Border BuildStatusBar()
    {
        // ponytail: 状态栏五段只是占位，M1 没有图像生产者；真实状态绑定随图像能力接入。
        var segments = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 18,
        };

        foreach (string text in new[] { "坐标 --", "像素值 --", "位深 --", "缩放 --", "校准单位 --" })
        {
            segments.Children.Add(new AtomTextBlock
            {
                Text = text,
                Opacity = 0.7,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }

        var transientMessage = new AtomTextBlock
        {
            Text = "就绪",
            Opacity = 0.7,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*"),
        };

        grid.Children.Add(segments);
        Grid.SetColumn(transientMessage, 1);
        grid.Children.Add(transientMessage);

        return new Border
        {
            Child = grid,
            Padding = new Thickness(12, 4),
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = Brushes.LightGray,
        };
    }
}
