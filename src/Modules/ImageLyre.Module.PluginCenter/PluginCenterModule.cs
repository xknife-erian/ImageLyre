using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using Cangming.PicoPlugins.Core.Support;
using ImageLyre.Module.PluginCenter.Internal;
using ImageLyre.Shell.Contracts;

namespace ImageLyre.Module.PluginCenter;

/// <summary>插件中心模块：提供插件与区域的运行期查看、启停入口。</summary>
/// <remarks>
///     <para>
///         构造函数只做装配、不读任何服务：模块与功能的依赖一律经 <c>DeclareDependencies</c> 声明，
///         需要宿主容器里的服务时实现 <see cref="ISupportUsingIoC" />，在注入阶段拿到
///         <see cref="IIoCManager" /> 的延迟引用后在 <see cref="Build" /> 里按需解析。
///         直接把 <c>IPluginManager</c> 写进构造函数会形成循环——插件由该管理器构造。
///     </para>
///     <para>
///         区域管理器经 <see cref="ISupportSlotManager" /> 注入，与 IoC 注入分别用显式接口实现，
///         因为两个接口对同一方法声明了不同的参数类型。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.PluginCenter.Module",
    name: "插件中心",
    alias: "PluginCenter",
    description: "查看插件与区域，并可运行期启停插件")]
public sealed class PluginCenterModule : BasePicoModule<IPluginCenter>, ISupportSlotManager, ISupportUsingIoC
{
    private readonly PluginCenterModuleContext _context = new();
    private Lazy<IIoCManager>? _iocManagerLazy;
    private Lazy<IPluginCenter>? _centerLazy;

    /// <summary>创建插件中心模块；构造函数只绑定上下文。</summary>
    public PluginCenterModule() => UseContext(_context);

    /// <inheritdoc />
    Task<IPicoPlugin> ISupportKernel<IIoCManager>.InjectAsync(Lazy<IIoCManager> iocManagerLazy)
    {
        _iocManagerLazy = iocManagerLazy;

        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <inheritdoc />
    Task<IPicoPlugin> ISupportKernel<ISlotManager>.InjectAsync(Lazy<ISlotManager> slotManagerLazy)
    {
        _context.SetSlotManager(slotManagerLazy);

        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <inheritdoc />
    public override Lazy<IPluginCenter> Build(params object[] args) =>
        _centerLazy ??= new Lazy<IPluginCenter>(() =>
        {
            IIoCManager ioc = _iocManagerLazy?.Value
                              ?? throw new InvalidOperationException("IoC 管理器未注入，无法解析插件管理器。");
            IPluginManager manager = ioc.Resolve<IPluginManager>()
                                     ?? throw new InvalidOperationException("容器中未注册 IPluginManager。");

            return new PluginCenterWindow(manager, _context.Slots);
        });

    /// <inheritdoc />
    protected override Task<bool> OnStartAsync() => Task.FromResult(true);
}

/// <summary>插件中心模块的上下文：只保存区域管理器的延迟引用。</summary>
internal sealed class PluginCenterModuleContext : BaseModuleContext
{
    private Lazy<ISlotManager>? _slotManagerLazy;

    /// <summary>区域管理器；宿主未注入时抛出可读异常。</summary>
    internal ISlotManager Slots => _slotManagerLazy?.Value
                                  ?? throw new InvalidOperationException(
                                      "SlotManager 未注入：宿主必须在创建插件中心窗口之前调用 InjectSlotManager。");

    /// <summary>接住区域管理器的延迟引用。</summary>
    /// <param name="slotManagerLazy">区域管理器延迟引用。</param>
    internal void SetSlotManager(Lazy<ISlotManager> slotManagerLazy)
    {
        ArgumentNullException.ThrowIfNull(slotManagerLazy);
        _slotManagerLazy = slotManagerLazy;
    }
}
