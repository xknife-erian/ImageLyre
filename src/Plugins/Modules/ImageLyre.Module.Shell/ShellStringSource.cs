using System.Globalization;
using ImageLyre.Contracts;

namespace ImageLyre.Module.Shell;

/// <summary>壳自有的文案源：按 <c>lyre.shell.*</c> 键提供 zh-CN 与 en-US 两套文本。</summary>
/// <remarks>
/// <para>
/// 本类只做键到文案的静态映射，不做语言回退、不缓存、不广播；回退与切换由宿主的
/// <see cref="ILocalizationService"/> 统一承担。未命中时返回 <see langword="false"/> 并给出空串，
/// 不把键名或默认文案写进 <c>out</c>，避免来源层偷偷替调用方做决定。
/// </para>
/// <para>
/// 线程与异常：字典在类型初始化后只读，可从任意线程查询；未知键、未知语言与空参数一律返回
/// <see langword="false"/>，不抛异常。M1 只有中英两套，新增语言时同步扩字典与测试。
/// </para>
/// </remarks>
internal sealed class ShellStringSource : ILocalizationSource
{
    // ponytail: M1 只有 zh-CN 与 en-US 两套文案；新增语言或迁移到卫星程序集时同步扩字典与测试。
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> s_texts =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["zh-CN"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ShellStringKeys.MenuFile] = "文件",
                [ShellStringKeys.MenuLanguageZh] = "中文",
                [ShellStringKeys.MenuLanguageEn] = "English",
                [ShellStringKeys.MenuPlaceholderItem] = "（菜单项由插件提供）",
                [ShellStringKeys.StatusCoordinate] = "坐标",
                [ShellStringKeys.StatusPixel] = "像素值",
                [ShellStringKeys.StatusDepth] = "位深",
                [ShellStringKeys.StatusZoom] = "缩放",
                [ShellStringKeys.StatusCalibration] = "校准单位",
                [ShellStringKeys.StatusReady] = "就绪",
                [ShellStringKeys.ViewportEmpty] = "无图像",
                [ShellStringKeys.PanelEmpty] = "尚无测量",
                [ShellStringKeys.WindowTitle] = "ImageLyre 工作台",
            },
            ["en-US"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [ShellStringKeys.MenuFile] = "File",
                [ShellStringKeys.MenuLanguageZh] = "中文",
                [ShellStringKeys.MenuLanguageEn] = "English",
                [ShellStringKeys.MenuPlaceholderItem] = "(Menu items supplied by plugins)",
                [ShellStringKeys.StatusCoordinate] = "Coordinates",
                [ShellStringKeys.StatusPixel] = "Pixel value",
                [ShellStringKeys.StatusDepth] = "Bit depth",
                [ShellStringKeys.StatusZoom] = "Zoom",
                [ShellStringKeys.StatusCalibration] = "Calibration",
                [ShellStringKeys.StatusReady] = "Ready",
                [ShellStringKeys.ViewportEmpty] = "No image",
                [ShellStringKeys.PanelEmpty] = "No measurements",
                [ShellStringKeys.WindowTitle] = "ImageLyre Workbench",
            },
        };

    /// <inheritdoc />
    public bool TryGet(string key, CultureInfo culture, out string value)
    {
        if (key is not null &&
            culture is not null &&
            s_texts.TryGetValue(culture.Name, out IReadOnlyDictionary<string, string>? texts) &&
            texts.TryGetValue(key, out string? text))
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
