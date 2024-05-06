using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace ImageLyre.Plugins;

/// <summary>菜单项描述：从 `.plugindesc` 的 <c>Focal</c> 元素读出的原始数据。</summary>
/// <param name="Header">菜单项标题。</param>
/// <param name="CommandName">命令名。</param>
/// <param name="Order">排序值。</param>
/// <param name="IsEnabled">是否可用。</param>
/// <param name="IsVisible">是否可见。</param>
internal sealed record PlugindescCommand(string Header, string CommandName, int Order, bool IsEnabled, bool IsVisible);

/// <summary>菜单分组描述：从 <c>Group</c> 元素读出的原始数据。</summary>
/// <param name="Header">分组标题，可为空。</param>
/// <param name="Items">分组内的菜单项。</param>
internal sealed record PlugindescGroup(string Header, IReadOnlyList<PlugindescCommand> Items);

/// <summary>菜单分类描述：从 <c>Category</c> 元素读出的原始数据。</summary>
/// <param name="Header">顶层菜单标题。</param>
/// <param name="Groups">该菜单下的分组。</param>
internal sealed record PlugindescCategory(string Header, IReadOnlyList<PlugindescGroup> Groups);

/// <summary>解析 `.plugindesc` 文档，把 XML 转成菜单描述。</summary>
/// <remarks>
///     只做"文档 → 数据"，不绑定命令、不认槽位、不依赖插件环境；解析失败由调用方按"无菜单"处理，
///     因为描述文件写错不该让插件启动失败。
/// </remarks>
internal static class PlugindescMenuReader
{
    private const string DisabledValue = "false";

    /// <summary>从 XML 流读取菜单分类。</summary>
    /// <param name="stream">`.plugindesc` 资源流。</param>
    /// <returns>菜单分类；文档没有任何分类时为空集合。</returns>
    /// <exception cref="System.Xml.XmlException">XML 格式非法。</exception>
    internal static IReadOnlyList<PlugindescCategory> Read(System.IO.Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        var document = XDocument.Load(stream);
        var categories = new List<PlugindescCategory>();

        foreach (var plugin in document.Descendants("Plugin"))
        {
            if (IsDisabled(plugin))
            {
                continue;
            }

            foreach (var category in plugin.Elements("Category"))
            {
                var header = AttributeOf(category, "Header");

                if (string.IsNullOrWhiteSpace(header))
                {
                    continue;
                }

                var groups = category.Elements("Group")
                    .Select(ReadGroup)
                    .ToArray();

                categories.Add(new PlugindescCategory(header, groups));
            }
        }

        return categories;
    }

    private static PlugindescGroup ReadGroup(XElement group)
    {
        var items = group.Elements("Focal")
            .Select(ReadCommand)
            .Where(command => command is not null)
            .Cast<PlugindescCommand>()
            .ToArray();

        return new PlugindescGroup(AttributeOf(group, "Header") ?? string.Empty, items);
    }

    private static PlugindescCommand? ReadCommand(XElement focal)
    {
        var header = AttributeOf(focal, "Header");
        var commandName = AttributeOf(focal, "Command");

        if (string.IsNullOrWhiteSpace(header) || string.IsNullOrWhiteSpace(commandName))
        {
            return null;
        }

        var order = int.TryParse(AttributeOf(focal, "Order"), out var parsed) ? parsed : 0;
        var isEnabled = !string.Equals(AttributeOf(focal, "Enable"), DisabledValue, StringComparison.OrdinalIgnoreCase);

        var visibility = AttributeOf(focal, "Visibility");
        var isVisible = !string.Equals(visibility, "Hidden", StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(visibility, "Collapsed", StringComparison.OrdinalIgnoreCase);

        return new PlugindescCommand(header, commandName, order, isEnabled, isVisible);
    }

    private static bool IsDisabled(XElement plugin) =>
        string.Equals(AttributeOf(plugin, "IsEnable"), DisabledValue, StringComparison.OrdinalIgnoreCase);

    private static string? AttributeOf(XElement element, string name) => element.Attribute(name)?.Value.Trim();
}
