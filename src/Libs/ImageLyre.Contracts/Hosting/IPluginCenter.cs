using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageLyre.Contracts.Hosting;

/// <summary>插件中心契约：插件与区域的只读快照，以及运行期的启停操作。</summary>
/// <remarks>
///     <para>
///         本契约<b>不含任何界面类型</b>：它只描述数据与操作，窗口由界面侧的功能自己创建。
///         因此提供方可以是纯服务模块，引用方可以是任意宿主。
///     </para>
///     <para>所有操作失败都以返回值表示，不抛异常——状态不允许是常态，不是异常路径。</para>
/// </remarks>
public interface IPluginCenter
{
    /// <summary>当前已加载插件的快照，按名称排序。</summary>
    IReadOnlyList<PluginDescriptor> Plugins { get; }

    /// <summary>当前已贡献区域及其附着数量，按名称排序。</summary>
    IReadOnlyList<SlotDescriptor> Slots { get; }

    /// <summary>重新读取插件与区域快照后触发。</summary>
    event EventHandler? Changed;

    /// <summary>重新读取快照并触发 <see cref="Changed" />。</summary>
    void Refresh();

    /// <summary>恢复或撤销某个插件的准入。</summary>
    /// <param name="pluginId">插件标识。</param>
    /// <param name="enabled">为真表示恢复准入。</param>
    /// <returns>状态迁移成功返回 <see langword="true" />。</returns>
    Task<bool> SetEnabledAsync(string pluginId, bool enabled);

    /// <summary>启动单个插件。</summary>
    /// <param name="pluginId">插件标识。</param>
    /// <returns>迁移到运行态返回 <see langword="true" />。</returns>
    Task<bool> StartAsync(string pluginId);

    /// <summary>停止单个插件。</summary>
    /// <param name="pluginId">插件标识。</param>
    /// <returns>原先在运行且已停止返回 <see langword="true" />。</returns>
    Task<bool> StopAsync(string pluginId);
}

/// <summary>插件快照。</summary>
/// <param name="Id">插件标识。</param>
/// <param name="Name">显示名。</param>
/// <param name="Version">版本。</param>
/// <param name="State">运行状态。</param>
/// <param name="Description">描述。</param>
public sealed record PluginDescriptor(string Id, string Name, string Version, string State, string Description);

/// <summary>区域快照。</summary>
/// <param name="Name">区域名。</param>
/// <param name="AttachmentCount">附着数量。</param>
public sealed record SlotDescriptor(string Name, int AttachmentCount);
