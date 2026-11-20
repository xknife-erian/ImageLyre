namespace ImageLyre.UI.Contracts.Workbench;

/// <summary>工作台消耗与提供的区域名称，是壳与插件之间唯一的字符串契约。</summary>
/// <remarks>
///     容器模块负责贡献这些区域，Feature 只向已存在的区域附着；两侧都不认识对方的类型，
///     因此新增区域意味着改这里而不是改某个实现。
/// </remarks>
public static class WorkbenchSlots
{
    /// <summary>菜单区域：各 Feature 在此附着菜单分类，由工作台按标题合并成菜单树。</summary>
    public const string Menu = "ImageLyre.Menu";

    /// <summary>主内容区域，后续由文档与视口填充。</summary>
    public const string Content = "ImageLyre.Content";

    /// <summary>状态栏区域，默认横向排列。</summary>
    public const string StatusBar = "ImageLyre.StatusBar";
}
