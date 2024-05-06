using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;

namespace ImageLyre.Module.Workbench;

/// <summary>工作台容器的上下文：只持有 Slot 管理器，窗口只从这里取依赖。</summary>
/// <remarks>Slot 管理器经 <see cref="ISupportSlotManager" /> 注入；两侧都只保存延迟引用，首次访问才求值。</remarks>
internal sealed class WorkbenchModuleContext : BaseModuleContext
{
    private Lazy<ISlotManager>? _slotManagerLazy;

    /// <summary>Slot 管理器；宿主未注入时抛出可读异常。</summary>
    internal ISlotManager Slots => _slotManagerLazy?.Value
                                   ?? throw new InvalidOperationException(
                                       "SlotManager 未注入：宿主必须在 CreateShell 之前调用 InjectSlotManager。");

    /// <summary>接住 Slot 管理器的延迟引用。</summary>
    /// <param name="slotManagerLazy">Slot 管理器延迟引用。</param>
    internal void SetSlotManager(Lazy<ISlotManager> slotManagerLazy)
    {
        ArgumentNullException.ThrowIfNull(slotManagerLazy);
        _slotManagerLazy = slotManagerLazy;
    }
}
