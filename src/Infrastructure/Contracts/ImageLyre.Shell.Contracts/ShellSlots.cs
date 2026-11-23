namespace ImageLyre.Shell.Contracts;

/// <summary>壳贡献的一级界面槽位名，是 Feature 与壳之间唯一的字符串契约。</summary>
/// <remarks>
/// <para>
/// Feature 引用本程序集取槽位名，不引用壳插件，因此替换壳实现不影响任何 Feature；
/// 新增一级槽位属于设计变更，必须先修订 `DESIGN-0001`，再改这里。
/// </para>
/// <para>
/// 名字按区域角色命名而不是按位置：没有 `left`、`right`、`bottom`，这样 Results 从右侧挪到下方时，
/// 槽位名与插件代码都不必改。命名一律小写点分，不使用缩写。
/// </para>
/// </remarks>
public static class ShellSlots
{
    /// <summary>工具栏，承载 <c>ToolbarButtonAttachment</c>，允许多项，按显式 <c>Order</c> 排序。</summary>
    public const string Toolbar = "lyre.shell.toolbar";

    /// <summary>图像视口，承载 <c>ContentAttachment</c>，M1 只允许一项，第二项被忽略并记一条警告。</summary>
    public const string Viewport = "lyre.shell.viewport";

    /// <summary>辅助面板，承载 <c>DockPaneAttachment</c>，允许多项，按显式 <c>Order</c> 排序。</summary>
    public const string Panel = "lyre.shell.panel";

    /// <summary>状态栏中段，承载插件贡献的读数片段（<c>StatusBarAttachment</c> 一类），允许多项，按显式 <c>Order</c> 排序。</summary>
    /// <remarks>
    /// 状态栏段位不是固定语义：每一段读数都可能由独立插件提供，壳只保留左右两侧的固定占位；
    /// M1 的五段规格占位由壳显示，等第一批真实读数插件接入后撤掉。
    /// </remarks>
    public const string StatusBar = "lyre.shell.statusbar";
}
