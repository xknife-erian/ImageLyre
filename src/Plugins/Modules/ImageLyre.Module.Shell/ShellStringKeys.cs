namespace ImageLyre.Module.Shell;

/// <summary>壳文案的资源键；统一以 <c>lyre.shell.</c> 开头，资源源与界面控件共用这一份定义。</summary>
/// <remarks>新增键时同步扩 <see cref="ShellStringSource"/> 的中英字典与本工程的测试；键名本身是持久契约，不随布局改名。</remarks>
internal static class ShellStringKeys
{
    internal static string MenuPlaceholder => "lyre.shell.menu.placeholder";
    internal static string MenuPlaceholderItem => "lyre.shell.menu.placeholderItem";
    internal static string StatusCoordinate => "lyre.shell.status.coordinate";
    internal static string StatusPixel => "lyre.shell.status.pixel";
    internal static string StatusDepth => "lyre.shell.status.depth";
    internal static string StatusZoom => "lyre.shell.status.zoom";
    internal static string StatusCalibration => "lyre.shell.status.calibration";
    internal static string StatusReady => "lyre.shell.status.ready";
    internal static string ViewportEmpty => "lyre.shell.viewport.empty";
    internal static string PanelEmpty => "lyre.shell.panel.empty";
    internal static string WindowTitle => "lyre.shell.window.title";
    internal static string LanguageZh => "lyre.shell.language.zh";
    internal static string LanguageEn => "lyre.shell.language.en";

    internal static IReadOnlyList<string> All =>
    [
        MenuPlaceholder,
        MenuPlaceholderItem,
        StatusCoordinate,
        StatusPixel,
        StatusDepth,
        StatusZoom,
        StatusCalibration,
        StatusReady,
        ViewportEmpty,
        PanelEmpty,
        WindowTitle,
        LanguageZh,
        LanguageEn,
    ];
}
