namespace ImageLyre.UI.Contracts.Theming;
/// <summary>
/// 应用程序主题（皮肤）管理器。
/// 支持动态切换浅色、深色或其他自定义主题。
/// </summary>
public interface IThemeManager
{
    /// <summary>
    /// 获取当前激活的主题名称（如 "Light", "Dark", "BlueSky"）。
    /// </summary>
    string CurrentThemeName { get; }

    /// <summary>
    /// 应用指定名称的主题。
    /// 若主题未注册，必须抛 <see cref="InvalidOperationException"/>（不允许静默回退到默认主题）。
    /// </summary>
    /// <param name="themeName">主题名称，必须非 null 且非空。</param>
    /// <exception cref="ArgumentException">当 themeName 为 null 或空时抛出。</exception>
    /// <exception cref="InvalidOperationException">当指定主题未注册时抛出。</exception>
    void ApplyTheme(string themeName);

    /// <summary>
    /// 注册一个新的主题资源。
    /// 通常在应用启动时由插件或配置模块调用。
    /// </summary>
    /// <param name="themeName">主题唯一标识名。</param>
    /// <param name="resourceDictionaryUri">主题资源字典的 URI（如 "/Themes/Dark.xaml"）。</param>
    /// <exception cref="ArgumentException">参数无效时抛出。</exception>
    void RegisterTheme(string themeName, string resourceDictionaryUri);

    /// <summary>
    /// 获取所有已注册的主题名称列表。
    /// </summary>
    IReadOnlyList<string> RegisteredThemes { get; }
}
