using System;
using System.Collections.Generic;
using System.Windows.Input;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts.Slots;
using ImageLyre.UI.Contracts.Workbench;

namespace ImageLyre.Plugins;

/// <summary>带菜单的 Feature 基类：把 `.plugindesc` 解析、命令绑定与槽位附着收在一处。</summary>
/// <remarks>
///     <para>
///         派生类只做两件事：给一个带 <c>[Plugin]</c> 的类，和一份命令字典。菜单结构写在程序集内嵌的
///         `.plugindesc` 里，基类把它解析成菜单分类附件并挂到菜单槽位——菜单项不在代码里拼。
///     </para>
///     <para>
///         命令字典的键必须与描述文件的 <c>Command</c> 逐字一致（区分大小写）；对不上时菜单项仍然出现，
///         但不可点，并由基类记一条警告，避免"描述改了、实现忘了改"这类静默漂移。
///     </para>
///     <para>
///         只贡献状态栏、工具栏或内容区的 Feature 不必继承本类，直接继承 <see cref="BasePicoFeatures" /> 即可。
///     </para>
/// </remarks>
public abstract class ImageLyreMenuFeature : BasePicoFeatures
{

    private IReadOnlyDictionary<string, ICommand>? _commands;

    /// <summary>菜单槽位名；默认是工作台的主菜单槽。</summary>
    protected virtual string MenuSlotName => WorkbenchSlots.Menu;

    /// <summary>本 Feature 的命令目录：键为命令名，值为命令实例。</summary>
    public IReadOnlyDictionary<string, ICommand> Commands => _commands ??= BuildCommands();

    /// <summary>构造命令字典；键必须与 `.plugindesc` 的 <c>Command</c> 属性逐字一致。</summary>
    /// <returns>命令字典。</returns>
    protected abstract IReadOnlyDictionary<string, ICommand> BuildCommands();

    /// <inheritdoc />
    public override void RegisterAttachments(IAttachmentRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        foreach (var category in PlugindescMenuSource.Read(GetType().Assembly, Commands))
        {
        // 附着前建立所有者作用域：附件的 OwnerId 归本功能，停用时按实例摘除。
        using var owner = (registry as ISlotRegistry)?.BeginOwner(GetType().Name);

            registry.AttachToSlot(MenuSlotName, category);
        }
    }
}
