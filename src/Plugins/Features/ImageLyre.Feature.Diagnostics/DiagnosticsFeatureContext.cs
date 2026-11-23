using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Contracts;
using ImageLyre.Contracts.Diagnostics;
using ImageLyre.Contracts.Platform;

namespace ImageLyre.Feature.Diagnostics;

/// <summary>诊断 Feature 的上下文：暴露两个能力契约，并持有宿主 IoC 的延迟引用。</summary>
/// <remarks>
/// <see cref="Platform"/> 与 <see cref="Performance"/> 使用 <c>Use&lt;T&gt;().Value</c>，与
/// <c>DeclareDependencies</c> 里的两个 <c>Require&lt;T&gt;()</c> 成对出现；构造函数与注入阶段都不读取它们。
/// <see cref="Localization"/> 只在 <c>OnStartAsync</c> 与附着阶段求值，失败时直接报错，不降级为无文案。
/// </remarks>
internal sealed class DiagnosticsFeatureContext : BaseFeatureContext
{
    private Lazy<IIoCManager>? _ioc;

    /// <summary>由 <see cref="DiagnosticsFeature"/> 的 IoC 注入点转发进来，此时还没到启动。</summary>
    /// <param name="ioc">宿主容器的延迟访问器，不可为 <see langword="null"/>。</param>
    public void SetIoC(Lazy<IIoCManager> ioc)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _ioc = ioc;
    }

    /// <summary>平台能力契约；读取时才求值。</summary>
    public IPlatformCapabilities Platform => Use<IPlatformCapabilities>().Value;

    /// <summary>性能监控契约；读取时才求值。</summary>
    public IPerformanceMonitor Performance => Use<IPerformanceMonitor>().Value;

    /// <summary>宿主注册的本地化服务；IoC 未注入或容器里没有该服务时抛出。</summary>
    /// <exception cref="InvalidOperationException">IoC 未注入，或宿主未注册 <see cref="ILocalizationService"/>。</exception>
    public ILocalizationService Localization => _ioc?.Value.Resolve<ILocalizationService>()
        ?? throw new InvalidOperationException(
            "宿主未注册 ILocalizationService：诊断 Feature 无法取得本地化服务，请检查 Bootstrap 容器的注册。");
}
