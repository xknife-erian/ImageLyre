using System.Globalization;
using ImageLyre.Contracts;

namespace ImageLyre.Localization;

/// <summary>界面文案的本地化实现：聚合资源源、维护当前语言、缺键回退并广播切换。</summary>
/// <remarks>
/// <para>
/// 解析顺序是先按当前语言逐个问来源，再按 <c>zh-CN</c> 问一遍，两轮都没命中时返回键名并记一条缺翻译诊断；
/// 回退只做一层，不做语言族回退（例如 <c>zh-TW</c> 不会退到 <c>zh-CN</c> 的内容），因为多一层回退会让
/// “这句话为什么是中文”变成必须追踪来源才能回答的问题。
/// </para>
/// <para>
/// 线程与生命周期：<see cref="Get"/> 与 <see cref="AddSource"/> 可从任意线程调用，内部以一把锁保护来源表与当前语言，
/// 查询时先取快照再调用来源，避免来源回调里再进本类造成死锁；本类不持有线程、不启动任务、没有需要释放的资源。
/// </para>
/// </remarks>
public sealed class LocalizationService : ILocalizationService
{
    private static readonly CultureInfo s_fallbackCulture = new("zh-CN");

    private readonly Action<string>? _onDiagnostic;
    private readonly List<ILocalizationSource> _sources = new();
    private readonly object _gate = new();
    private CultureInfo _currentCulture = s_fallbackCulture;

    /// <summary>创建一个本地化服务。</summary>
    /// <param name="onDiagnostic">缺键与来源异常的外送回调；为空时丢弃诊断。</param>
    /// <remarks>
    /// 回调在调用方的线程上同步执行，因此实现方不得在回调里做阻塞操作，也不得回调 <see cref="Get"/> 之外的本类成员，
    /// 否则会在锁外重入并打乱解析顺序。
    /// </remarks>
    public LocalizationService(Action<string>? onDiagnostic = null)
    {
        _onDiagnostic = onDiagnostic;
    }

    /// <inheritdoc />
    public CultureInfo Current
    {
        get
        {
            lock (_gate)
                return _currentCulture;
        }
    }

    /// <inheritdoc />
    public event EventHandler? LanguageChanged;

    /// <inheritdoc />
    public string Get(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        ILocalizationSource[] sources;
        CultureInfo culture;
        lock (_gate)
        {
            sources = _sources.ToArray();
            culture = _currentCulture;
        }

        foreach (var source in sources)
        {
            if (TryGetFromSource(source, key, culture, out var value))
                return value;
        }

        if (!string.Equals(culture.Name, s_fallbackCulture.Name, StringComparison.Ordinal))
        {
            foreach (var source in sources)
            {
                if (TryGetFromSource(source, key, s_fallbackCulture, out var value))
                    return value;
            }
        }

        _onDiagnostic?.Invoke($"缺少翻译：{key}（{culture.Name}）");
        return key;
    }

    /// <summary>向单个来源取键，并把来源抛出的异常降级成一条诊断。</summary>
    /// <remarks>
    /// 隔离的边界在来源与聚合器之间：一个坏来源只能让自己这一轮解析失败，不得中断其余来源的解析，
    /// 也不得把异常抛给界面上正在取文案的调用方，否则一个第三方插件就能让整个工作台变成空白。
    /// </remarks>
    private bool TryGetFromSource(ILocalizationSource source, string key, CultureInfo culture, out string value)
    {
        try
        {
            return source.TryGet(key, culture, out value);
        }
        catch (Exception ex)
        {
            _onDiagnostic?.Invoke($"资源源 {source.GetType().Name} 取键 {key} 失败：{ex.Message}");
            value = string.Empty;
            return false;
        }
    }

    /// <inheritdoc />
    public void AddSource(ILocalizationSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        lock (_gate)
        {
            foreach (var existing in _sources)
            {
                if (ReferenceEquals(existing, source))
                    return;
            }

            _sources.Add(source);
        }
    }

    /// <inheritdoc />
    public Task SetLanguageAsync(string cultureName)
    {
        ArgumentNullException.ThrowIfNull(cultureName);

        // 先解析再进锁：predefinedOnly 保证“任意字符串都被当成语言”这类假成功不会发生，
        // 语言名非法时抛 CultureNotFoundException，此时当前语言与事件都不动。
        var culture = CultureInfo.GetCultureInfo(cultureName, predefinedOnly: true);

        lock (_gate)
        {
            if (string.Equals(_currentCulture.Name, culture.Name, StringComparison.Ordinal))
                return Task.CompletedTask;

            _currentCulture = culture;
        }

        LanguageChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
