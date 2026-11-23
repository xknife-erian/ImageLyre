using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Contracts.Platform;
using ImageLyre.Module.Platform.Internal;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Module.Platform;

/// <summary>M1 的平台能力插件：向宿主提供 Windows 实测能力矩阵与永不触发的变更流。</summary>
/// <remarks>
/// <para>
/// 模块只对外提供 <see cref="IPlatformCapabilities"/>，不消费任何模块契约，因此依赖声明保持为空；
/// 构造函数只装配上下文与矩阵实例，不读取上下文依赖，也不探测系统。
/// </para>
/// <para>
/// 生命周期：没有需要启动或停止的服务，<see cref="OnStartAsync"/> 只记录一行启动日志；
/// 不覆盖 public 的 <c>StartServiceAsync</c>/<c>StopServiceAsync</c>/<c>Dispose</c>。
/// </para>
/// </remarks>
[Plugin(id: "lyre.capability.platform", name: "平台能力")]
public sealed class PlatformModule : BasePicoModule<IPlatformCapabilities>
{
    private static readonly ILogger s_logger = LogHub.For(typeof(PlatformModule));

    private readonly PlatformCapabilities _capabilities = new();

    /// <summary>创建一个只完成上下文装配的平台能力插件。</summary>
    /// <remarks>构造函数不读取任何依赖；平台矩阵在字段初始化时按当前系统判定一次。</remarks>
    public PlatformModule() : base()
    {
        UseContext(new PlatformModuleContext());
    }

    /// <summary>对外的平台能力矩阵实例。</summary>
    protected override IPlatformCapabilities Module => _capabilities;

    /// <summary>平台插件没有模块依赖。</summary>
    /// <param name="modules">依赖声明构建器，本方法不使用。</param>
    /// <remarks>方法会被读依赖与读顺序各调用一次，因此保持空声明、不写副作用。</remarks>
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules)
    {
    }

    /// <summary>启动时记录平台能力插件已就绪。</summary>
    /// <returns>恒为 <see langword="true"/>。</returns>
    protected override Task<bool> OnStartAsync()
    {
        s_logger.LogInformation("平台能力插件已启动。");
        return Task.FromResult(true);
    }
}
