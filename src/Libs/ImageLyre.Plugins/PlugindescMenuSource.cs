using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Cangming.Logs;
using ImageLyre.UI.Contracts.Workbench;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Plugins;

/// <summary>从程序集内嵌的 `.plugindesc` 读取菜单分类，并把命令名绑定到插件提供的命令实例。</summary>
/// <remarks>
///     <para>
///         `.plugindesc` 是菜单的唯一真源，作为嵌入资源编译进程序集，内容一字不改；
///         找不到资源时返回空集合而不是抛异常——"没有菜单的插件"是合法形态。
///     </para>
///     <para>
///         命令名在描述文件里写、实现在 <see cref="ImageLyreMenuFeature.BuildCommands" /> 里给；
///         对不上时该项仍然出现但不可点，并留一条警告日志，方便一眼看出描述与实现脱节。
///     </para>
/// </remarks>
internal static partial class PlugindescMenuSource
{
    private const string ResourceSuffix = ".plugindesc";

    private static readonly ILogger s_logger = LogHub.For(typeof(PlugindescMenuSource));

    /// <summary>读取并绑定菜单分类。</summary>
    /// <param name="assembly">插件程序集。</param>
    /// <param name="commands">命令字典，键与描述文件的 <c>Command</c> 逐字一致。</param>
    /// <returns>已绑定命令的菜单分类。</returns>
    internal static IReadOnlyList<MenuCategoryAttachment> Read(
        Assembly assembly,
        IReadOnlyDictionary<string, ICommand> commands)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(commands);

        var assemblyName = assembly.GetName().Name ?? "(unknown)";
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(ResourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            LogNoDescriptor(s_logger, assemblyName);

            return [];
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            LogUnreadableDescriptor(s_logger, assemblyName, resourceName);

            return [];
        }

        IReadOnlyList<PlugindescCategory> categories;

        try
        {
            categories = PlugindescMenuReader.Read(stream);
        }
        catch (Exception exception) when (exception is System.Xml.XmlException or InvalidOperationException)
        {
            LogParseFailed(s_logger, exception, assemblyName, resourceName);

            return [];
        }

        return categories.Select(category => Bind(category, commands, assemblyName)).ToArray();
    }

    private static MenuCategoryAttachment Bind(
        PlugindescCategory category,
        IReadOnlyDictionary<string, ICommand> commands,
        string assemblyName)
    {
        var groups = new List<MenuGroupAttachment>(category.Groups.Count);

        foreach (var group in category.Groups)
        {
            var items = new List<MenuCommandAttachment>(group.Items.Count);

            foreach (var command in group.Items)
            {
                if (!commands.TryGetValue(command.CommandName, out var instance))
                {
                    LogCommandMissing(s_logger, assemblyName, command.CommandName, command.Header);
                }

                items.Add(new MenuCommandAttachment(command.Header, command.CommandName, instance, command.Order, command.IsEnabled, command.IsVisible));
            }

            groups.Add(new MenuGroupAttachment(group.Header, items));
        }

        return new MenuCategoryAttachment(category.Header, groups);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "程序集 {Assembly} 没有 .plugindesc，按无菜单处理")]
    private static partial void LogNoDescriptor(ILogger logger, string assembly);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "程序集 {Assembly} 的菜单描述资源 {Resource} 不可读，按无菜单处理")]
    private static partial void LogUnreadableDescriptor(ILogger logger, string assembly, string resource);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "程序集 {Assembly} 的菜单描述解析失败，按无菜单处理：{Resource}")]
    private static partial void LogParseFailed(ILogger logger, Exception exception, string assembly, string resource);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "命令名未找到实现：{Assembly} / {CommandName}（菜单项「{Header}」将不可点）")]
    private static partial void LogCommandMissing(ILogger logger, string assembly, string commandName, string header);
}
