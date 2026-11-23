using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Contracts.Platform;

namespace ImageLyre.Module.Performance;

/// <summary>性能监控插件的上下文：按声明暴露平台能力，求值推迟到服务实际采样时。</summary>
/// <remarks>
/// <see cref="Capabilities"/> 使用 <c>Use&lt;T&gt;().Value</c>，与 <c>DeclareDependencies</c> 里的
/// <c>Require&lt;IPlatformCapabilities&gt;()</c> 成对出现；构造函数与注入阶段都不读取它。
/// </remarks>
internal sealed class PerformanceModuleContext : BaseModuleContext
{
    /// <summary>上一次启动已就绪的平台能力。</summary>
    /// <remarks>读取时才求值；性能监控服务只通过 <see cref="Func{TResult}"/> 间接使用它。</remarks>
    public IPlatformCapabilities Capabilities => Use<IPlatformCapabilities>().Value;
}
