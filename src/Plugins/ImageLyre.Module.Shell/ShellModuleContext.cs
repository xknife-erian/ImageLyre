using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;

namespace ImageLyre.Module.Shell;

/// <summary>壳插件自己的上下文：保存槽位管理器的延迟引用，并给出贡献槽位所需的注册面。</summary>
/// <remarks>
/// <para>
/// 依赖声明集中在 <c>ShellModule.DeclareDependencies</c>，本类只提供取用入口；壳不消费任何模块契约，
/// 因此除了槽位管理器没有别的属性。上下文与模块一一对应，不共用实例，也不外传。
/// </para>
/// <para>
/// 时序：注入阶段只保存 <see cref="Lazy{T}"/>，第一次读取 <see cref="SlotManager"/> 或 <see cref="Registry"/>
/// 才求值；<c>ShellModule.Build</c> 与 <see cref="ShellWindow"/> 的构造都发生在注入之后，因此合法。
/// </para>
/// </remarks>
internal sealed class ShellModuleContext : BaseModuleContext
{
    private Lazy<ISlotManager>? _slots;

    /// <summary>接收 <see cref="ISupportSlotManager"/> 注入的槽位管理器延迟引用。</summary>
    /// <param name="slots">槽位管理器延迟引用，不得为 <see langword="null"/>。</param>
    /// <exception cref="ArgumentNullException"><paramref name="slots"/> 为 <see langword="null"/>。</exception>
    /// <remarks>只保存引用，不求值；求值点留在 <see cref="SlotManager"/> 与 <see cref="Registry"/> 的读取处。</remarks>
    public void SetSlotManager(Lazy<ISlotManager> slots)
    {
        ArgumentNullException.ThrowIfNull(slots);
        _slots = slots;
    }

    /// <summary>注入的槽位管理器；尚未注入或延迟引用返回 <see langword="null"/> 时抛出。</summary>
    /// <remarks>求值后返回的就是宿主注入的同一个实例，三个 <c>SlotPanel</c> 必须共享它。</remarks>
    /// <exception cref="InvalidOperationException">尚未注入，或延迟引用返回 <see langword="null"/>。</exception>
    public ISlotManager SlotManager => _slots?.Value
        ?? throw new InvalidOperationException(
            "SlotManager 尚未注入：ShellWindow 只能在宿主调用 InjectSlotManager 之后构造。");

    /// <summary>贡献槽位用的注册面，由同一个槽位管理器强转而来。</summary>
    /// <remarks>
    /// 实现不是 <see cref="IAttachmentRegistry"/> 时直接报错，不降级为“无槽位”状态，
    /// 因为那样窗口会看起来正常，但所有 Feature 附着都会失败。
    /// </remarks>
    /// <exception cref="InvalidOperationException">槽位管理器未注入，或其实现不包含 <see cref="IAttachmentRegistry"/>。</exception>
    public IAttachmentRegistry Registry => SlotManager as IAttachmentRegistry
        ?? throw new InvalidOperationException(
            "SlotManager 的实现没有 IAttachmentRegistry，无法贡献槽位；请注入同时实现 ISlotManager 与 IAttachmentRegistry 的实例。");
}
