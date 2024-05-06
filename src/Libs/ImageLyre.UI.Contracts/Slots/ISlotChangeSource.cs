using Cangming.PicoPlugins.Core.Features;

namespace ImageLyre.UI.Contracts.Slots;

/// <summary>
///     槽位里的一条附着记录：附着物加上它的来源，供按所有者摘除与投影记账。
/// </summary>
/// <remarks>
///     记录不可变，<see cref="OwnerId" /> 是诊断归组用的类型标识，<see cref="OwnerInstance" /> 是生命周期实例标识，
///     两者在附着时刻由 <see cref="ISlotRegistry.BeginOwner" /> 确定，之后不随上下文变化；同一附件实例在同一槽位只出现一次。
/// </remarks>
/// <param name="SlotName">槽位名。</param>
/// <param name="OwnerId">所有者类型标识；未声明所有者时为 <c>(unknown)</c>。</param>
/// <param name="OwnerInstance">所有者实例标识，供按实例释放；未声明所有者时等于 <paramref name="OwnerId" />。</param>
/// <param name="Attachment">槽位里的附着物实例。</param>
public sealed record SlotAttachmentEntry(
    string SlotName,
    string OwnerId,
    string OwnerInstance,
    IFeatureAttachment Attachment);

/// <summary>
///     槽位结构变化的增量：只包含本次真正加入与移除的条目，不表达"重新读取"。
/// </summary>
/// <remarks>
///     <para>
///         一次变化至少有一侧非空；<see cref="Added" /> 按入列顺序给出，<see cref="Removed" /> 按移除前的槽位顺序给出，
///         因此消费者可以用它精确地增删自己的投影节点，而不必重读整个槽位。
///     </para>
///     <para>
///         附着物自身的状态（如菜单项是否可用）不在这里传播：那属于附着物的属性变更通知，与结构变化是两条通道。
///     </para>
/// </remarks>
/// <param name="SlotName">发生结构变化的槽位名。</param>
/// <param name="Added">本次加入的条目。</param>
/// <param name="Removed">本次移除的条目。</param>
public sealed record SlotChange(
    string SlotName,
    IReadOnlyList<SlotAttachmentEntry> Added,
    IReadOnlyList<SlotAttachmentEntry> Removed);

/// <summary>
///     槽位结构变化的增量流：替代"槽位内容变了，去重新读一遍"的粗粒度通知。
/// </summary>
/// <remarks>
///     <para>
///         实现方在附着、按所有者摘除、待解析桶冲刷三个时刻发事件；只有真正进入或离开槽位的条目才出现在增量里，
///         被类型或容量校验拒绝的附着不产生任何事件。
///     </para>
///     <para>
///         线程：事件在引发变化的那条线程上同步触发，管理器不切换线程、不合并事件；界面消费者应在唯一的封送点
///         把增量切回界面线程后再改可视对象，避免各订阅方各自调度造成乱序。
///     </para>
///     <para>
///         失败隔离：订阅方抛出的异常不被管理器捕获，会沿触发线程向上传播；需要容错的消费者自行包裹，
///         但不能因此吞掉后续增量，管理器在事件返回后即认为本次变化已通知完毕。
///     </para>
///     <para>
///         生命周期：管理器存活期间订阅有效；订阅方应在自身销毁时退订，否则会拖住管理器持有的引用。
///         <see cref="GetAttachments" /> 返回调用时刻的快照，只供初始化、诊断与检查使用，不能当作变化检测手段。
///     </para>
/// </remarks>
public interface ISlotChangeSource
{
    /// <summary>槽位结构变化；只报告加入与移除，不报告"需要重读"。</summary>
    event EventHandler<SlotChange>? Changed;

    /// <summary>取指定槽位当前的附着条目快照（含来源），顺序与渲染顺序一致。</summary>
    /// <param name="slotName">槽位名；未贡献或不存在时返回空列表。</param>
    /// <returns>附着条目快照；调用方修改返回值不影响管理器。</returns>
    IReadOnlyList<SlotAttachmentEntry> GetAttachments(string slotName);
}
