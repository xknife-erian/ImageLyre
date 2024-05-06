using System.Globalization;

namespace ImageLyre.UI.Contracts.Localization;

/// <summary>
/// 应用程序本地化服务：以 resx 资源与区域性文化为核心，统一提供 UI 文本查询与语言动态切换。
/// </summary>
/// <remarks>
/// 该契约是本地化的唯一入口，默认实现见 Cangming.Shell.Defaults.Localization 的 <c>ResourceManagerLocalizationService</c>（宿主无关，资源由宿主注入）。
/// 日志不经过本服务：日志一律保持英文并结构化，仅面向用户展示的文案经 <see cref="GetString(string)"/> 翻译。
/// </remarks>
public interface ILocalizationService
{
    /// <summary>
    /// 获取当前 UI 语言（驱动资源查找的区域性），例如 <c>"zh-CN"</c>、<c>"en-US"</c>。
    /// </summary>
    CultureInfo CurrentUICulture { get; }

    /// <summary>
    /// 获取当前数字/日期/货币格式文化（驱动 <see cref="GetString(string, object[])"/> 的参数格式化）。
    /// </summary>
    /// <remarks>
    /// 通常与 <see cref="CurrentUICulture"/> 为同一语言，但二者语义独立，为将来「英文界面 + 其他数字格式」组合保留空间。
    /// </remarks>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// 获取所有可用语言（按本族语名称排序），供 UI 下拉选择。
    /// </summary>
    IReadOnlyList<CultureInfo> AvailableCultures { get; }

    /// <summary>
    /// 语言切换时推送新语言，宿主或 UI 借此自动刷新已显示的文本。
    /// </summary>
    /// <remarks>
    /// 触发线程：<see cref="SetCulture"/> 的调用线程（消费者自行调度到 UI 线程）；回放：无（配合快照属性 <see cref="CurrentUICulture"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<CultureInfo> CultureChanged { get; }

    /// <summary>
    /// 根据键名获取当前 UI 语言下的本地化字符串。
    /// </summary>
    /// <remarks>
    /// 键不存在时返回原始键名（便于调试与降级展示），不抛异常、不返回空串。
    /// </remarks>
    /// <param name="key">资源键，如 <c>"File_Save"</c>。</param>
    /// <returns>本地化后的字符串；缺失时返回 <paramref name="key"/> 本身。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 null 时抛出。</exception>
    string GetString(string key);

    /// <summary>
    /// 根据键名与参数获取格式化后的本地化字符串。
    /// </summary>
    /// <param name="key">资源键。</param>
    /// <param name="args">格式化参数；其中的数字与日期按 <see cref="CurrentCulture"/> 格式化。</param>
    /// <returns>格式化后的本地化字符串。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 null 时抛出。</exception>
    /// <exception cref="FormatException">资源值含非法格式占位符，或 <paramref name="args"/> 与占位符不匹配时抛出。</exception>
    string GetString(string key, params object[] args);

    /// <summary>
    /// 切换应用语言：同时更新 UI 语言与数字/日期文化，并推送 <see cref="CultureChanged"/>。
    /// </summary>
    /// <param name="culture">目标语言区域性，如 <c>new CultureInfo("en-US")</c>。</param>
    /// <exception cref="ArgumentNullException"><paramref name="culture"/> 为 null 时抛出。</exception>
    /// <exception cref="ArgumentException"><paramref name="culture"/> 不在 <see cref="AvailableCultures"/> 中时抛出。</exception>
    void SetCulture(CultureInfo culture);
}
