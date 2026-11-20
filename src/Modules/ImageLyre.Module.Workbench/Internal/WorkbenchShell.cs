using AtomUIMenu = AtomUI.Desktop.Controls.Menu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Cangming.Logs;
using Cangming.PicoPlugins.Avalonias.Core;
using Cangming.PicoPlugins.Avalonias.Slots;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Module.Workbench.Internal.Menu;
using ImageLyre.Module.Workbench.Slots;
using ImageLyre.UI.Contracts.Workbench;
using Microsoft.Extensions.Logging;

using LyreContentAttachment = ImageLyre.UI.Contracts.Attachments.ContentAttachment;
using System.Collections.Generic;
using ImageLyre.UI.Contracts.Slots;

namespace ImageLyre.Module.Workbench.Internal;

/// <summary>主窗口：菜单、内容与状态栏三个区域，具体内容由 Feature 通过区域附着。</summary>
/// <remarks>
///     <para>
///         菜单结构由<b>投影</b>产生而不是由壳写死：Feature 附着菜单分类（分类→分组→菜单项三层数据），
///         投影按标题合并后渲染成 AtomUI 菜单控件。壳因此不认识任何 Feature 类型，
///         未准入某个 Feature 时它那一项自然消失。
///     </para>
///     <para>
///         窗口与菜单一律用 AtomUI 的控件：本仓没有第二套主题，裸 Avalonia 窗口缺 OverlayLayer，
///         菜单弹层根本建不出来。
///     </para>
/// </remarks>
internal sealed partial class WorkbenchShell : AtomUIWindow
{
    private static readonly ILogger s_logger = LogHub.For<WorkbenchShell>();

    private readonly ISlotManager _slots;
    private readonly Control _contentHost;
    private readonly TextBlock _busyText = new();
    private readonly Border _busyOverlay = new();
    private readonly StackPanel _tabs = new() { Orientation = Orientation.Horizontal, Spacing = 4, IsVisible = false };
    private readonly Dictionary<string, Button> _tabsById = new(StringComparer.Ordinal);
    private readonly AtomUIMenu _menu = new();

    internal WorkbenchShell(ISlotManager slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        _slots = slots;

        // 壳是区域的拥有者：先声明三个区域与形态，功能才能按形态附着；未声明的附着会进诊断而不是静默消失。
        if (slots is ISlotRegistry registry)
        {
            registry.ContributeSlot(new SlotDescriptor(WorkbenchSlots.Menu, SlotKind.Menu, SlotCapacity.Multiple, "主菜单"));
            registry.ContributeSlot(new SlotDescriptor(WorkbenchSlots.StatusBar, SlotKind.StatusBar, SlotCapacity.Multiple, "状态栏"));
            registry.ContributeSlot(new SlotDescriptor(WorkbenchSlots.Content, SlotKind.Content, SlotCapacity.Multiple, "主内容区"));

        }

        Title = "ImageLyre";
        Width = 1280;
        Height = 800;

        var statusPanel = new SlotPanel
        {
            SlotManager = slots,
            SlotName = WorkbenchSlots.StatusBar,
            Orientation = Orientation.Horizontal,
        };

        var statusBar = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = Brushes.LightGray,
            Padding = new Thickness(8, 3),
            Child = statusPanel,
        };

        // 内容区不走沧溟的 SlotPanel：它只认沧溟的附件形状。这里订阅槽位变化，
        // 取本仓 ContentAttachment 的控件工厂直接设给内容宿主，D 批的各态才看得见。
        // 内容区就是一个位置宿主：位置名是壳的锚点，内容由别人附着。
        _contentHost = slots is ISlotRegistry slotRegistry
            ? new SlotHost(slotRegistry, WorkbenchSlots.Content)
            : new ContentControl();
        if (slots is ISlotChangeSource changeSource)
        {
            changeSource.Changed += OnSlotChanged;
        }


        _busyText.HorizontalAlignment = HorizontalAlignment.Center;
        _busyOverlay.Background = Avalonia.Media.Brushes.Transparent;
        _busyOverlay.Child = _busyText;
        _busyOverlay.IsVisible = false;

        var layout = new DockPanel();
        DockPanel.SetDock(_menu, Dock.Top);
        DockPanel.SetDock(statusBar, Dock.Bottom);
        layout.Children.Add(_menu);
        layout.Children.Add(statusBar);
        layout.Children.Add(_contentHost);

        var body = new Grid { Children = { layout, _busyOverlay } };
        var root = new DockPanel();
        DockPanel.SetDock(_tabs, Dock.Top);
        root.Children.Add(_tabs);
        root.Children.Add(body);
        Content = root;

        RebuildMenu();
        slots.SlotContentChanged += (_, args) =>
        {
            if (args.SlotName == WorkbenchSlots.Menu)
            {
                RebuildMenu();
            }
        };
    }

    /// <summary>按投影结果重建菜单树。</summary>
    private void OnSlotChanged(object? sender, SlotChange e)
    {
        if (string.Equals(e.SlotName, WorkbenchSlots.Content, StringComparison.Ordinal))
        {
    
        _busyText.HorizontalAlignment = HorizontalAlignment.Center;
        _busyOverlay.Background = Avalonia.Media.Brushes.Transparent;
        _busyOverlay.Child = _busyText;
        _busyOverlay.IsVisible = false;
        }
    }

    private void RebuildMenu()
    {
        var categories = new MenuProjection(_slots).Build();

        _menu.Items.Clear();
        foreach (var category in categories)
        {
            var categoryItem = new AtomUIMenuItem { Header = category.Header };

            foreach (var group in category.Groups)
            {
                var items = group.Items.OrderBy(item => item.Order).ToArray();

                if (string.IsNullOrEmpty(group.Header))
                {
                    foreach (var item in items)
                    {
                        categoryItem.Items.Add(CreateLeaf(item));
                    }

                    continue;
                }

                var groupItem = new AtomUIMenuItem { Header = group.Header };
                foreach (var item in items)
                {
                    groupItem.Items.Add(CreateLeaf(item));
                }

                categoryItem.Items.Add(groupItem);
            }

            _menu.Items.Add(categoryItem);
        }

        LogMenuRebuilt(s_logger, categories.Count, _menu.Items.Count);
    }

    private static AtomUIMenuItem CreateLeaf(MenuCommandAttachment item) =>
        new()
        {
            Header = item.Header,
            Command = item.Command,
            IsEnabled = item.IsEnabled && item.Command is not null,
        };

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "菜单重建完成：{Categories} 个分类，{Items} 个顶层项")]
    private static partial void LogMenuRebuilt(ILogger logger, int categories, int items);

    /// <inheritdoc />
    public void ShowBusy(string message, uint progress)
    {
        _busyText.Text = progress is 0 or uint.MaxValue ? message : $"{message} · {progress}%";
        _busyOverlay.IsVisible = true;
    }

    /// <inheritdoc />
    public void HideBusy() => _busyOverlay.IsVisible = false;

    /// <inheritdoc />
    public void AddDocumentTab(string contentId, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentId);

        if (_tabsById.ContainsKey(contentId))
        {
            return;
        }

        var tab = new Button { Content = title };
        tab.Bind(Button.FontSizeProperty, new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension("Lyre.Type.Size.S13"));
        _tabsById[contentId] = tab;
        _tabs.Children.Add(tab);
        _tabs.IsVisible = true;
    }

    /// <inheritdoc />
    public void RemoveDocumentTab(string contentId)
    {
        if (_tabsById.Remove(contentId, out var tab))
        {
            _tabs.Children.Remove(tab);
            _tabs.IsVisible = _tabs.Children.Count > 0;
        }
    }
}
