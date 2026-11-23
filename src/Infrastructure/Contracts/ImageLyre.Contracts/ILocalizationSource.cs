using System.Globalization;

namespace ImageLyre.Contracts;

/// <summary>一个插件或壳提供的文案源。</summary>
/// <remarks>
/// <para>
/// 源只回答“这个键在我这里是什么”，不负责回退与缓存：回退到 <c>zh-CN</c>、跨源查找与诊断记录都由
/// <see cref="ILocalizationService"/> 承担，源自己再做一层回退会让两套规则在同一个产品里分叉。
/// </para>
/// <para>
/// 线程与异常：实现必须线程安全、无副作用，并且不得抛异常——查表失败只能返回 <see langword="false"/>；
/// 若实现确实抛了异常，服务会捕获并记诊断，然后继续问下一个源。
/// </para>
/// </remarks>
public interface ILocalizationSource
{
    /// <summary>按键取指定语言的文案。</summary>
    /// <param name="key">资源键。</param>
    /// <param name="culture">目标语言。</param>
    /// <param name="value">命中的文案；未命中时为 <see cref="string.Empty"/>。</param>
    /// <returns>命中时为 <see langword="true"/>。</returns>
    /// <remarks>实现必须对同一输入返回稳定结果，不得在内部按 <see cref="ILocalizationService.Current"/> 再判一次语言。</remarks>
    bool TryGet(string key, CultureInfo culture, out string value);
}
