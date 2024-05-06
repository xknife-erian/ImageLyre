namespace ImageLyre.UI.Contracts.Icons;

/// <summary>
///     菜单项图标：<c>.plugindesc</c> 的两种写法都收在这里。
/// </summary>
/// <remarks>
///     只承载"画什么"，不负责"怎么画"：资源路径由渲染层解析成图像，内联文本由渲染层解析成矢量，
///     因此本契约不引任何绘图库。
/// </remarks>
public sealed record MenuIcon
{
    /// <summary>指向插件内 <c>.svg</c> 资源的路径，例如 <c>/Assets/Experiment/StopCompile.svg</c>。</summary>
    public string? SvgPath { get; init; }

    /// <summary>内联 SVG 文本（<c>plugindesc</c> 的 <c>IconCode</c> 子元素）。</summary>
    public string? IconCode { get; init; }

    /// <summary>两种写法都没有时为空图标。</summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(SvgPath) && string.IsNullOrWhiteSpace(IconCode);

    /// <summary>空图标。</summary>
    public static MenuIcon None { get; } = new();
}
