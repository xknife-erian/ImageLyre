using System;
using System.Collections.Generic;
using System.Linq;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using ImageLyre.Contracts.Hosting;
using ImageLyre.UI.Contracts.Slots;
using ImageLyre.UI.Contracts.Workbench;

namespace ImageLyre.Feature.PluginCenter.Internal;

/// <summary>插件中心窗口：左侧插件与运行期启停，右侧区域（槽位）清单与槽位诊断。</summary>
/// <remarks>
///     <para>
///         插件快照与启停来自 <see cref="IPluginCenter" />；槽位诊断来自界面层的 <see cref="ISlotRegistry" />，
///         因为诊断属于界面位置的概念，不进无界面的插件契约。
///     </para>
///     <para>注册表缺席时两块清单都显示为空，窗口在任何宿主里都能打开。</para>
/// </remarks>
internal sealed class PluginCenterWindow : AtomUIWindow
{
    private readonly IPluginCenter _center;
    private readonly ISlotRegistry? _slots;

    private readonly ListBox _pluginList = new();
    private readonly ListBox _slotList = new();
    private readonly ListBox _issueList = new();
    private readonly TextBlock _status = new() { Text = "就绪" };

    private List<string> _pluginIds = [];

    internal PluginCenterWindow(IPluginCenter center, ISlotRegistry? slots)
    {
        _center = center;
        _slots = slots;

        Title = "插件中心";
        Width = 980;
        Height = 620;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        Content = BuildLayout();
        Refresh();

        _center.Changed += (_, _) => Refresh();
        Closed += (_, _) => _center.Changed -= OnCenterChanged;
    }

    private void OnCenterChanged(object? sender, EventArgs args) => Refresh();

    private DockPanel BuildLayout()
    {
        var pluginPanel = new DockPanel();

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 8, 0, 0),
        };

        buttons.Children.Add(CreateButton("刷新", () => _center.Refresh()));
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

        var issueBorder = new Border
        {
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.LightGray,
            MaxHeight = 160,
            Child = _issueList,
        };
        DockPanel.SetDock(issueBorder, Dock.Bottom);
        slotPanel.Children.Add(issueBorder);

        var issueTitle = new TextBlock { Text = "槽位诊断", FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 12, 0, 6) };
        DockPanel.SetDock(issueTitle, Dock.Bottom);
        slotPanel.Children.Add(issueTitle);

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

    /// <summary>重新读取插件快照与槽位诊断。</summary>
    private void Refresh()
    {
        var selectedIndex = _pluginList.SelectedIndex;

        var plugins = _center.Plugins;
        _pluginIds = plugins.Select(plugin => plugin.Id).ToList();
        _pluginList.ItemsSource = plugins
            .Select(plugin => $"{plugin.Name}  ·  {plugin.Id}  ·  v{plugin.Version}  ·  {plugin.State}")
            .ToArray();
        _pluginList.SelectedIndex = selectedIndex >= 0 && selectedIndex < _pluginIds.Count ? selectedIndex : (plugins.Count > 0 ? 0 : -1);

        var diagnostics = _slots?.CompleteRegistration() ?? new SlotDiagnostics([], []);

        _slotList.ItemsSource = diagnostics.Slots
            .Select(slot => $"{slot.Name}  ·  {slot.Kind}  ·  {slot.Capacity}  ·  {slot.Count} 项  ·  {(slot.Declared ? "已声明" : "未声明")}")
            .ToArray();

        _issueList.ItemsSource = diagnostics.Issues
            .GroupBy(issue => issue.Kind)
            .Select(group => $"{group.Key}  ·  {group.Count()} 条")
            .ToArray();

        _status.Text = $"插件 {plugins.Count} 个 · 区域 {diagnostics.Slots.Count} 个 · 诊断 {diagnostics.Issues.Count} 条";
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

        var result = await _center.SetEnabledAsync(pluginId, enabled).ConfigureAwait(true);
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

        var result = await _center.StartAsync(pluginId).ConfigureAwait(true);
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

        var result = await _center.StopAsync(pluginId).ConfigureAwait(true);
        _status.Text = $"停止 {pluginId}：{(result ? "成功" : "状态不允许")}";
        Refresh();
    }
}
