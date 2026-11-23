using Cangming.PicoPlugins.Core.Modules;

namespace ImageLyre.Module.Platform;

/// <summary>平台能力插件的上下文：M1 没有模块依赖，也不缓存额外状态。</summary>
/// <remarks>
/// 平台模块只对外提供 <see cref="ImageLyre.Contracts.Platform.IPlatformCapabilities"/>，不消费其它契约，
/// 因此上下文保持空；每个插件一个实例，构造函数只装配，不读取任何依赖。
/// </remarks>
internal sealed class PlatformModuleContext : BaseModuleContext
{
}
