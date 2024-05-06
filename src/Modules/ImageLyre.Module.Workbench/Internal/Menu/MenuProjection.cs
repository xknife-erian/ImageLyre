using System;
using System.Collections.Generic;
using System.Linq;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts;

namespace ImageLyre.Module.Workbench.Internal.Menu;

/// <summary>菜单投影：把槽位里的菜单附件按标题合并成渲染用的菜单树。</summary>
/// <remarks>
///     <para>
///         模型照沧溟 Studio 与 Mercury 的做法：Feature 附着的是分类、分组与菜单项三层数据，
///         不是控件；同名分类、同名分组按标题合并，因此多个 Feature 往「帮助」里加项只出现一个「帮助」。
///     </para>
///     <para>
///         当前实现每次槽位变化整棵重建。菜单项数量在首版很少，重建成本可以忽略；
///         等到菜单项上百时再改成按容器增量维护，那时才需要引入稳定容器。
///     </para>
/// </remarks>
internal sealed class MenuProjection
{
    private static readonly string[] s_baseCategories = ["文件", "帮助"];

    private readonly ISlotManager _slots;

    /// <summary>创建投影。</summary>
    /// <param name="slots">槽位管理器，不可为 <see langword="null" />。</param>
    internal MenuProjection(ISlotManager slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        _slots = slots;
    }

    /// <summary>按当前槽位内容生成菜单树。</summary>
    /// <returns>已按标题合并的分类集合，顺序为先基准分类、后其余分类。</returns>
    internal IReadOnlyList<MenuCategoryModel> Build()
    {
        var categories = new List<MenuCategoryModel>();
        var byHeader = new Dictionary<string, MenuCategoryModel>(StringComparer.Ordinal);

        // 基准分类由壳自己提供：没有任何 Feature 贡献时，「文件」「帮助」仍然存在。
        foreach (var header in s_baseCategories)
        {
            var model = new MenuCategoryModel(header);
            byHeader[header] = model;
            categories.Add(model);
        }

        foreach (var source in _slots.GetSlotContents(WorkbenchSlots.Menu).OfType<MenuCategoryAttachment>())
        {
            if (!byHeader.TryGetValue(source.Header, out var category))
            {
                category = new MenuCategoryModel(source.Header);
                byHeader[source.Header] = category;
                categories.Add(category);
            }

            category.Merge(source);
        }

        return categories;
    }
}

/// <summary>合并后的菜单分类。</summary>
internal sealed class MenuCategoryModel
{
    private readonly Dictionary<string, MenuGroupModel> _groupsByHeader = new(StringComparer.Ordinal);

    internal MenuCategoryModel(string header) => Header = header;

    /// <summary>顶层菜单标题。</summary>
    internal string Header { get; }

    /// <summary>合并后的分组。</summary>
    internal List<MenuGroupModel> Groups { get; } = [];

    /// <summary>把一份分类附件并入当前模型。</summary>
    /// <param name="source">来源附件。</param>
    internal void Merge(MenuCategoryAttachment source)
    {
        foreach (var group in source.Groups)
        {
            if (!_groupsByHeader.TryGetValue(group.Header, out var model))
            {
                model = new MenuGroupModel(group.Header);
                _groupsByHeader[group.Header] = model;
                Groups.Add(model);
            }

            model.AddRange(group.Items.Where(item => item.IsVisible));
        }
    }
}

/// <summary>合并后的菜单分组；<see cref="Header" /> 为空表示菜单项直接挂在上一级。</summary>
internal sealed class MenuGroupModel
{
    internal MenuGroupModel(string header) => Header = header;

    /// <summary>分组标题。</summary>
    internal string Header { get; }

    /// <summary>分组内的菜单项。</summary>
    internal List<MenuCommandAttachment> Items { get; } = [];

    /// <summary>追加菜单项。</summary>
    /// <param name="items">待追加的菜单项。</param>
    internal void AddRange(IEnumerable<MenuCommandAttachment> items) => Items.AddRange(items);
}
