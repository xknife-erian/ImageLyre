using System.Globalization;
using ImageLyre.Contracts;

namespace ImageLyre.Presentation.Desktop.Localization;

/// <summary>欢迎窗口使用的桌面入口自有资源键，集中定义以免写入方与资源源各写一份字面量。</summary>
internal static class AppStringKeys
{
    /// <summary>产品名，窗口标题与主标题共用。</summary>
    internal static string Product => "lyre.app.product";

    /// <summary>版本号前缀，后面接程序集版本。</summary>
    internal static string Version => "lyre.app.version";

    /// <summary>欢迎窗口的状态文本。</summary>
    internal static string Status => "lyre.app.status";

    /// <summary>启动失败时的前缀，后面接异常消息。</summary>
    internal static string Failed => "lyre.app.failed";
}

/// <summary>桌面入口自有界面文案源，M1 只提供欢迎窗口用到的四枚资源键。</summary>
/// <remarks>
/// <para>
/// 资源键以 <c>lyre.app.</c> 开头，与壳的 <c>lyre.shell.*</c> 和插件的 Id 前缀分开，因此壳或插件后注册同名键也不会覆盖这里的文案；
/// 本类型只做键到文案的静态映射，不负责回退、缓存或语言切换，那些语义由 <see cref="ILocalizationService"/> 统一承担。
/// </para>
/// <para>
/// 线程与异常：映射在类型初始化时构建后只读，可从任意线程查询；未知键或未支持的语言一律返回 <see langword="false"/>，不抛异常。
/// </para>
/// </remarks>
public sealed class AppStringSource : ILocalizationSource
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> s_texts =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["zh-CN"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [AppStringKeys.Product] = "ImageLyre",
                [AppStringKeys.Version] = "版本",
                [AppStringKeys.Status] = "正在装载插件与主窗体……",
                [AppStringKeys.Failed] = "启动失败：",
            },
            ["en-US"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [AppStringKeys.Product] = "ImageLyre",
                [AppStringKeys.Version] = "Version",
                [AppStringKeys.Status] = "Loading plugins and the main window...",
                [AppStringKeys.Failed] = "Startup failed:",
            },
        };

    /// <inheritdoc />
    public bool TryGet(string key, CultureInfo culture, out string value)
    {
        if (key is not null &&
            culture is not null &&
            s_texts.TryGetValue(culture.Name, out var texts) &&
            texts.TryGetValue(key, out var text))
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
