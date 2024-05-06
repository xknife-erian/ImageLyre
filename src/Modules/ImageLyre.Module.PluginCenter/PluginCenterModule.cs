using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using Cangming.PicoPlugins.Core.Support;
using ImageLyre.Contracts;

namespace ImageLyre.Module.PluginCenter;

/// <summary>插件中心模块：提供插件与区域的只读快照与运行期启停，<b>不含界面</b>。</summary>
/// <remarks>
///     <para>
///         本模块遵循 Module 与 Feature 的分工：Module 只对外提供数据与操作契约，
///         窗口与控件由请求方（Feature）自己创建。因此本工程不引用 Avalonia，也不含任何窗口类型。
///     </para>
///     <para>
///         构造函数只装配；插件管理器经 <see cref="ISupportUsingIoC" /> 的延迟引用在首次使用时解析，
///         区域管理器经 <see cref="ISupportSlotManager" /> 注入。两个注入接口用显式实现区分。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.PluginCenter.Module",
    name: "插件中心",
    alias: "PluginCenter",
    description: "提供插件与区域的快照及运行期启停")]
public sealed class PluginCenterModule : BasePicoModule<IPluginCenter>, IPluginCenter, ISupportSlotManager, ISupportUsingIoC
{
    private Lazy<IIoCManager>? _iocManagerLazy;
    private Lazy<ISlotManager>? _slotManagerLazy;
    private IPluginManager? _pluginManager;
    private Lazy<IPluginCenter>? _centerLazy;

    /// <inheritdoc />
    public event EventHandler? Changed;

    /// <inheritdoc />
    public IReadOnlyList<PluginDescriptor> Plugins =>
        Manager.Plugins
            .Select(plugin =>
            {
                var info = Manager.GetPluginInfo(plugin.GetType());

                return new PluginDescriptor(info.Id, info.Name, info.Version.ToString(), plugin.State.ToString(), info.Description);
            })
            .OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal)
            .ToArray();

    /// <inheritdoc />
    public IReadOnlyList<SlotDescriptor> Slots
    {
        get
        {
            ISlotManager slots = SlotsManager;

            return slots.GetAllSlots()
                .Select(slot => new SlotDescriptor(slot.Name, slots.GetSlotContents(slot.Name).Count))
                .OrderBy(descriptor => descriptor.Name, StringComparer.Ordinal)
                .ToArray();
        }
    }

    private IPluginManager Manager
    {
        get
        {
            if (_pluginManager is not null)
            {
                return _pluginManager;
            }

            IIoCManager ioc = _iocManagerLazy?.Value
                              ?? throw new InvalidOperationException("IoC 管理器未注入，无法解析插件管理器。");

            _pluginManager = ioc.Resolve<IPluginManager>()
                             ?? throw new InvalidOperationException("容器中未注册 IPluginManager。");
            _pluginManager.PluginsChanged += (_, _) => Changed?.Invoke(this, EventArgs.Empty);

            return _pluginManager;
        }
    }

    private ISlotManager SlotsManager =>
        _slotManagerLazy?.Value
        ?? throw new InvalidOperationException("SlotManager 未注入：宿主必须在读取区域快照之前调用 InjectSlotManager。");

    /// <inheritdoc />
    Task<IPicoPlugin> ISupportKernel<IIoCManager>.InjectAsync(Lazy<IIoCManager> iocManagerLazy)
    {
        _iocManagerLazy = iocManagerLazy;

        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <inheritdoc />
    Task<IPicoPlugin> ISupportKernel<ISlotManager>.InjectAsync(Lazy<ISlotManager> slotManagerLazy)
    {
        _slotManagerLazy = slotManagerLazy;

        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <inheritdoc />
    public override Lazy<IPluginCenter> Build(params object[] args) => _centerLazy ??= new Lazy<IPluginCenter>(() => this);

    /// <inheritdoc />
    public void Refresh() => Changed?.Invoke(this, EventArgs.Empty);

    /// <inheritdoc />
    public Task<bool> SetEnabledAsync(string pluginId, bool enabled) =>
        enabled ? Manager.EnablePluginAsync(pluginId) : Manager.DisablePluginAsync(pluginId);

    /// <inheritdoc />
    public Task<bool> StartAsync(string pluginId) => Manager.StartPluginAsync(pluginId);

    /// <inheritdoc />
    public Task<bool> StopAsync(string pluginId) => Manager.StopPluginAsync(pluginId);

    /// <inheritdoc />
    protected override Task<bool> OnStartAsync() => Task.FromResult(true);
}
