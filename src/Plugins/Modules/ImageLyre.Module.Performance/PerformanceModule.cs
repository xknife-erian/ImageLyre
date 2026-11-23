using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Contracts.Diagnostics;
using ImageLyre.Contracts.Platform;
using ImageLyre.Module.Performance.Internal;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Module.Performance;

/// <summary>M1 的性能监控插件：按需产出进程与 GC 指标快照。</summary>
/// <remarks>
/// <para>
/// 模块硬依赖 <see cref="IPlatformCapabilities"/> 并声明 <c>After&lt;IPlatformCapabilities&gt;()</c>，
/// 因为可采指标与平台能力矩阵同源；依赖缺失时管理器会把本模块钉在 Failed，而不是让采样在运行期才失败。
/// </para>
/// <para>
/// 构造函数只装配上下文；服务实例通过 <see cref="Module"/> 惰性创建，传给监控器的是
/// <c>() =&gt; _context.Capabilities</c>，因此在构造函数与注入阶段都不会求值平台依赖。
/// </para>
/// </remarks>
[Plugin(id: "lyre.capability.performance", name: "性能监控")]
public sealed class PerformanceModule : BasePicoModule<IPerformanceMonitor>
{
    private static readonly ILogger s_logger = LogHub.For(typeof(PerformanceModule));

    private readonly PerformanceModuleContext _context;
    private PerformanceMonitor? _monitor;

    /// <summary>创建一个只完成上下文装配的性能监控插件。</summary>
    /// <remarks>构造函数不读取上下文依赖，也不创建采样器。</remarks>
    public PerformanceModule() : base()
    {
        _context = new PerformanceModuleContext();
        UseContext(_context);
    }

    /// <summary>按需创建并缓存性能监控器。</summary>
    protected override IPerformanceMonitor Module =>
        _monitor ??= new PerformanceMonitor(() => _context.Capabilities);

    /// <summary>声明对平台能力的硬依赖与启动先后。</summary>
    /// <param name="modules">依赖声明构建器。</param>
    /// <remarks>本方法会被读依赖与读顺序各调用一次，只写声明、不读任何 Lazy 值。</remarks>
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules)
    {
        modules.Require<IPlatformCapabilities>();
        modules.After<IPlatformCapabilities>();
    }

    /// <summary>启动时记录性能监控插件已就绪。</summary>
    /// <returns>恒为 <see langword="true"/>。</returns>
    protected override Task<bool> OnStartAsync()
    {
        s_logger.LogInformation("性能监控插件已启动（平台能力依赖已满足）。");
        return Task.FromResult(true);
    }
}
