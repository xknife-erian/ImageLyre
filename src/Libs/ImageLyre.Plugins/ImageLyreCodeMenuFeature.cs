using System;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts.Slots;
using ImageLyre.UI.Contracts.Workbench;

namespace ImageLyre.Plugins;

/// <summary>官方插件的菜单基类：菜单在代码里声明，`code-first`。</summary>
/// <remarks>
///     <para>
///         本仓有两条菜单装配路径：官方插件用本类在代码里直接声明菜单结构，结构与命令同处一文件，
///         编译期就能发现命令名写错；第三方插件用 <see cref="ImageLyreMenuFeature" /> 读内嵌
///         `.plugindesc`，好处是不改代码就能改菜单。
///     </para>
///     <para>
///         两条路径产出同一种数据附件（分类 → 分组 → 菜单项），投影层不区分来源。
///     </para>
/// </remarks>
public abstract class ImageLyreCodeMenuFeature : BasePicoFeatures
{

    /// <summary>菜单槽位名；默认是工作台的主菜单槽。</summary>
    protected virtual string MenuSlotName => WorkbenchSlots.Menu;

    /// <summary>把一组菜单项挂到一个顶层菜单下。</summary>
    /// <param name="registry">附着注册器。</param>
    /// <param name="category">顶层菜单标题，例如「帮助」。</param>
    /// <param name="items">菜单项。</param>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null" />。</exception>
    /// <exception cref="ArgumentException"><paramref name="category" /> 为空，或 <paramref name="items" /> 为空。</exception>
    protected void AttachMenu(
        IAttachmentRegistry registry,
        string category,
        params MenuCommandAttachment[] items)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Length == 0)
        {
            throw new ArgumentException("菜单至少需要一个菜单项。", nameof(items));
        }

        // 附着前建立所有者作用域：附件的 OwnerId 归本功能，停用时按实例摘除。
        using var owner = (registry as ISlotRegistry)?.BeginOwner(GetType().Name);

        registry.AttachToSlot(
            MenuSlotName,
            new MenuCategoryAttachment(category, [new MenuGroupAttachment(string.Empty, items)]));
    }
}
