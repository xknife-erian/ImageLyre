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
using ImageLyre.UI.Contracts.Workbench;
using Microsoft.Extensions.Logging;

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
    private readonly AtomUIMenu _menu = new();

    internal WorkbenchShell(ISlotManager slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        _slots = slots;

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

        var content = new SlotPanel
        {
            SlotManager = slots,
            SlotName = WorkbenchSlots.Content,
            Orientation = Orientation.Vertical,
        };

        var layout = new DockPanel();
        DockPanel.SetDock(_menu, Dock.Top);
        DockPanel.SetDock(statusBar, Dock.Bottom);
        layout.Children.Add(_menu);
        layout.Children.Add(statusBar);
        layout.Children.Add(content);

        Content = layout;

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
}
