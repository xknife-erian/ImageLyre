using System.Windows.Input;
using Cangming.PicoPlugins.Core.Features;

namespace ImageLyre.UI.Contracts.Workbench;

/// <summary>菜单项附件：一条菜单项的数据描述，渲染层直接消费它。</summary>
/// <remarks>
///     附件只声明"要有一项、它叫什么、对应哪个命令名"；命令实例由插件在 <c>BuildCommands</c> 里按名字提供，
///     由基类在附着时解析。命令名解析不到时该项仍然出现但不可点，并在日志里留一条诊断——
///     菜单是描述文件的投影，描述写错不该让插件启动失败。
/// </remarks>
public sealed class MenuCommandAttachment : IFeatureAttachment
{
    /// <summary>创建菜单项附件。</summary>
    /// <param name="header">菜单项标题。</param>
    /// <param name="commandName">命令名，须与插件命令字典的键逐字一致。</param>
    /// <param name="command">解析后的命令；为 <see langword="null" /> 表示未解析到。</param>
    /// <param name="order">同一分组内的排序值，越小越靠前。</param>
    /// <param name="isEnabled">是否可用；描述文件里写 <c>Enable="false"</c> 时置假。</param>
    /// <param name="isVisible">是否可见；描述文件里写 <c>Visibility="Hidden"</c> 或 <c>"Collapsed"</c> 时置假。</param>
    /// <exception cref="ArgumentException"><paramref name="header" /> 或 <paramref name="commandName" /> 为空。</exception>
    public MenuCommandAttachment(
        string header,
        string commandName,
        ICommand? command = null,
        int order = 0,
        bool isEnabled = true,
        bool isVisible = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);

        Header = header;
        CommandName = commandName;
        Command = command;
        Order = order;
        IsEnabled = isEnabled;
        IsVisible = isVisible;
    }

    /// <summary>菜单项标题。</summary>
    public string Header { get; }

    /// <summary>命令名。</summary>
    public string CommandName { get; }

    /// <summary>解析后的命令；<see langword="null" /> 表示描述文件里的命令名没有对应实现。</summary>
    public ICommand? Command { get; }

    /// <summary>同一分组内的排序值。</summary>
    public int Order { get; }

    /// <summary>是否可用；菜单渲染时同时要求命令已解析。</summary>
    public bool IsEnabled { get; }

    /// <summary>是否可见；为假时该项不进入菜单。</summary>
    public bool IsVisible { get; }
}

/// <summary>菜单分组附件：把若干菜单项收进一个分组，标题为空时菜单项直接挂在上一级。</summary>
public sealed class MenuGroupAttachment : IFeatureAttachment
{
    /// <summary>创建菜单分组附件。</summary>
    /// <param name="header">分组标题；为空表示不建子菜单，直接挂到上一级。</param>
    /// <param name="items">分组内的菜单项。</param>
    public MenuGroupAttachment(string header, IReadOnlyList<MenuCommandAttachment> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Header = header;
        Items = items;
    }

    /// <summary>分组标题；为空表示菜单项直接挂在上一级。</summary>
    public string Header { get; }

    /// <summary>分组内的菜单项。</summary>
    public IReadOnlyList<MenuCommandAttachment> Items { get; }
}

/// <summary>菜单分类附件：附着到菜单槽位的单位，对应一个顶层菜单。</summary>
/// <remarks>
///     多个 Feature 贡献同名分类时按标题合并：顶层菜单、分组、菜单项各自按标题归并，
///     因此两个功能都往「帮助」里加项时，最终只出现一个「帮助」菜单。
/// </remarks>
public sealed class MenuCategoryAttachment : IFeatureAttachment
{
    /// <summary>创建菜单分类附件。</summary>
    /// <param name="header">顶层菜单标题。</param>
    /// <param name="groups">该菜单下的分组。</param>
    /// <exception cref="ArgumentException"><paramref name="header" /> 为空。</exception>
    public MenuCategoryAttachment(string header, IReadOnlyList<MenuGroupAttachment> groups)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentNullException.ThrowIfNull(groups);

        Header = header;
        Groups = groups;
    }

    /// <summary>顶层菜单标题。</summary>
    public string Header { get; }

    /// <summary>该菜单下的分组。</summary>
    public IReadOnlyList<MenuGroupAttachment> Groups { get; }
}
