using System;
using System.Collections.Generic;
using System.Linq;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Shell.Contracts;

namespace ImageLyre.Module.PluginCenter.Internal;

/// <summary>插件中心窗口：左侧插件与运行期启停，右侧区域（槽位）清单。</summary>
/// <remarks>
///     窗口直接读写沧溟的插件管理器与区域管理器，不经过任何中间缓存：
///     启停按钮调用的就是管理器的方法，刷新按钮重新读一次列表，避免界面与真实状态脱节。
/// </remarks>
internal sealed class PluginCenterWindow : AtomUIWindow, IPluginCenter
{
    private readonly IPluginManager _pluginManager;
    private readonly ISlotManager _slots;

    private readonly ListBox _pluginList = new();
    private readonly ListBox _slotList = new();
    private readonly TextBlock _status = new() { Text = "就绪" };

    private List<string> _pluginIds = [];

    internal PluginCenterWindow(IPluginManager pluginManager, ISlotManager slots)
    {
        _pluginManager = pluginManager;
        _slots = slots;

        Title = "插件中心";
        Width = 980;
        Height = 620;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = BuildLayout();
        Refresh();
    }

    /// <inheritdoc />
    public Window CreateWindow() => this;

    private DockPanel BuildLayout()
    {
        var pluginPanel = new DockPanel();

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 8, 0, 0),
        };

        buttons.Children.Add(CreateButton("刷新", Refresh));
        buttons.Children.Add(CreateButton("启用", () => SetEnabledAsync(true)));
        buttons.Children.Add(CreateButton("禁用", () => SetEnabledAsync(false)));
        buttons.Children.Add(CreateButton("启动", StartAsync));
        buttons.Children.Add(CreateButton("停止", StopAsync));

        DockPanel.SetDock(buttons, Dock.Bottom);
        pluginPanel.Children.Add(buttons);
        pluginPanel.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            Child = _pluginList,
        });

        var slotPanel = new DockPanel();
        var slotTitle = new TextBlock { Text = "区域（槽位）", FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 6) };
        DockPanel.SetDock(slotTitle, Dock.Top);
        slotPanel.Children.Add(slotTitle);
        slotPanel.Children.Add(new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            Child = _slotList,
        });

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("2*,8,1*"),
            Margin = new Thickness(12),
        };
        Grid.SetColumn(pluginPanel, 0);
        Grid.SetColumn(slotPanel, 2);
        grid.Children.Add(pluginPanel);
        grid.Children.Add(slotPanel);

        var layout = new DockPanel();
        var title = new TextBlock
        {
            Text = "插件中心",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(12, 12, 12, 0),
        };
        DockPanel.SetDock(title, Dock.Top);

        var footer = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = Brushes.LightGray,
            Padding = new Thickness(12, 6),
            Child = _status,
        };
        DockPanel.SetDock(footer, Dock.Bottom);

        layout.Children.Add(title);
        layout.Children.Add(footer);
        layout.Children.Add(grid);

        return layout;
    }

    private static Button CreateButton(string text, Action onClick)
    {
        var button = new Button { Content = text, MinWidth = 64 };
        button.Click += (_, _) => onClick();
        return button;
    }

    /// <summary>重新读取插件与区域列表。</summary>
    private void Refresh()
    {
        var selectedIndex = _pluginList.SelectedIndex;

        _pluginIds = [];
        var rows = new List<string>();

        foreach (var plugin in _pluginManager.Plugins.OrderBy(plugin => plugin.GetType().Name, StringComparer.Ordinal))
        {
            var info = _pluginManager.GetPluginInfo(plugin.GetType());
            _pluginIds.Add(info.Id);
            rows.Add($"{info.Name}  ·  {info.Id}  ·  v{info.Version}  ·  {plugin.State}");
        }

        _pluginList.ItemsSource = rows;
        _pluginList.SelectedIndex = selectedIndex >= 0 && selectedIndex < rows.Count ? selectedIndex : (rows.Count > 0 ? 0 : -1);

        _slotList.ItemsSource = _slots.GetAllSlots()
            .OrderBy(slot => slot.Name, StringComparer.Ordinal)
            .Select(slot => $"{slot.Name}  ·  {_slots.GetSlotContents(slot.Name).Count} 项")
            .ToArray();

        _status.Text = $"插件 {rows.Count} 个 · 区域 {_slots.GetAllSlots().Count} 个";
    }

    private string? SelectedPluginId =>
        _pluginList.SelectedIndex >= 0 && _pluginList.SelectedIndex < _pluginIds.Count
            ? _pluginIds[_pluginList.SelectedIndex]
            : null;

    private async void SetEnabledAsync(bool enabled)
    {
        var pluginId = SelectedPluginId;

        if (pluginId is null)
        {
            _status.Text = "先选中一个插件";
            return;
        }

        var result = enabled
            ? await _pluginManager.EnablePluginAsync(pluginId).ConfigureAwait(true)
            : await _pluginManager.DisablePluginAsync(pluginId).ConfigureAwait(true);

        _status.Text = $"{(enabled ? "启用" : "禁用")} {pluginId}：{(result ? "成功" : "状态不允许")}";
        Refresh();
    }

    private async void StartAsync()
    {
        var pluginId = SelectedPluginId;

        if (pluginId is null)
        {
            _status.Text = "先选中一个插件";
            return;
        }

        var result = await _pluginManager.StartPluginAsync(pluginId).ConfigureAwait(true);
        _status.Text = $"启动 {pluginId}：{(result ? "成功" : "状态不允许")}";
        Refresh();
    }

    private async void StopAsync()
    {
        var pluginId = SelectedPluginId;

        if (pluginId is null)
        {
            _status.Text = "先选中一个插件";
            return;
        }

        var result = await _pluginManager.StopPluginAsync(pluginId).ConfigureAwait(true);
        _status.Text = $"停止 {pluginId}：{(result ? "成功" : "状态不允许")}";
        Refresh();
    }
}
