using System.Globalization;

namespace ImageLyre.Contracts;

/// <summary>界面文案的本地化入口：聚合各插件的资源源，广播语言切换。</summary>
/// <remarks>
/// <para>
/// 资源键按所有者命名空间划分：壳用 <c>lyre.shell.*</c>，插件用自身 Id 前缀，键名不跨所有者复用。
/// 缺键时先回退 <c>zh-CN</c>，回退仍失败则返回键名并记一条诊断，键名本身不得作为界面文案展示给用户。
/// </para>
/// <para>
/// 切换只广播 <see cref="LanguageChanged"/>，订阅方在回调里刷新自己的文案，窗口不重建；
/// 数字、单位、日期与 CSV 导出格式不随语言变化，否则一次切换就会让新结果与既有表格无法对照。
/// </para>
/// <para>
/// 线程与生命周期：实现必须允许任意线程调用 <see cref="Get"/>，并且只在宿主 UI 线程上触发
/// <see cref="LanguageChanged"/>；实例由宿主在进程启动时创建、贯穿整个进程生命周期，订阅方负责在销毁时退订。
/// </para>
/// </remarks>
public interface ILocalizationService
{
    /// <summary>当前界面语言。</summary>
    /// <remarks>读取本身不触发切换；初始值取宿主启动时的语言，通常是 <c>zh-CN</c>。</remarks>
    CultureInfo Current { get; }

    /// <summary>按资源键取文案。</summary>
    /// <param name="key">资源键，按所有者命名空间书写，例如 <c>lyre.shell.menu.file</c>。</param>
    /// <returns>命中的文案；所有源都缺该键时返回键名本身，并留下一条缺翻译诊断。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> 为 <see langword="null"/>。</exception>
    /// <remarks>可从任意线程调用；已知键的解析结果可以缓存，但缓存必须随语言切换整体失效。</remarks>
    string Get(string key);

    /// <summary>语言切换完成后触发一次，订阅方据此刷新自身文案。</summary>
    /// <remarks>
    /// 在宿主 UI 线程上触发，因此订阅方可以直接改控件；回调内不得再调用 <see cref="SetLanguageAsync"/>，
    /// 否则会形成递归切换。回调抛出的异常由订阅方自己吞掉并记日志，服务不代为实现方兜底。
    /// </remarks>
    event EventHandler? LanguageChanged;

    /// <summary>注册一个资源源。</summary>
    /// <param name="source">资源源实例。</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> 为 <see langword="null"/>。</exception>
    /// <remarks>
    /// 同一实例重复注册幂等，不产生重复解析；注册本身不触发 <see cref="LanguageChanged"/>。
    /// 资源源通常在插件的启动动作里注册，插件停止后其源不再被查询，但服务不主动移除它。
    /// </remarks>
    void AddSource(ILocalizationSource source);

    /// <summary>切换界面语言，完成后触发一次 <see cref="LanguageChanged"/>。</summary>
    /// <param name="cultureName">目标语言名，例如 <c>zh-CN</c> 或 <c>en-US</c>。</param>
    /// <returns>切换完成的任务；目标语言与当前语言相同时不起作用。</returns>
    /// <exception cref="CultureNotFoundException"><paramref name="cultureName"/> 不是可识别的语言名。</exception>
    /// <remarks>
    /// 切换失败时 <see cref="Current"/> 保持原值且不触发事件，调用方据异常决定是否提示用户；
    /// 切换成功后由各订阅方自行刷新文案，服务只保证事件恰好发一次。
    /// </remarks>
    Task SetLanguageAsync(string cultureName);
}
