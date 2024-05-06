namespace ImageLyre.UI.Contracts.Slots;

/// <summary>
///     槽位描述符：由拥有界面区域的一方在贡献槽位时给出。
/// </summary>
/// <remarks>
///     描述符只用于校验与诊断，不暴露给渲染层；渲染层仍按自己的附件词汇过滤。<see cref="Kind" /> 与附件声明的
///     <see cref="ISlotAttachment.Kind" /> 必须一致，否则附体会被拒绝并记 <see cref="SlotIssueKind.KindMismatch" />。
/// </remarks>
/// <param name="Name">全局唯一的槽位名，采用 <c>&lt;区域&gt;[.&lt;用途&gt;][.&lt;方位&gt;]</c> 三段式。</param>
/// <param name="Kind">槽位承载的内容形态。</param>
/// <param name="Capacity">容量，默认多值。</param>
/// <param name="DisplayName">可读名称，用于诊断与插件中心。</param>
/// <param name="IsReserved">
///     是否为预留槽位：宿主把位置留出来，但空是预期状态。诊断对预留槽位的"空"只记信息，
///     对非预留槽位的"空"记 <see cref="SlotIssueKind.EmptySlot" />。
/// </param>
public sealed record SlotDescriptor(
    string Name,
    SlotKind Kind,
    SlotCapacity Capacity = SlotCapacity.Multiple,
    string? DisplayName = null,
    bool IsReserved = false);

/// <summary>
///     槽位快照：插件中心与诊断用的只读视图。
/// </summary>
/// <param name="Name">槽位名。</param>
/// <param name="Kind">内容形态；未声明槽位为 <see cref="SlotKind.Content" />。</param>
/// <param name="Capacity">容量；未声明槽位按多值处理。</param>
/// <param name="Declared">是否已被贡献。</param>
/// <param name="Count">当前附件数。</param>
/// <param name="Owners">附件所有者标识（去重后）。</param>
/// <param name="IsReserved">是否为预留槽位，见 <see cref="SlotDescriptor.IsReserved" />。</param>
public sealed record SlotSnapshot(
    string Name,
    SlotKind Kind,
    SlotCapacity Capacity,
    bool Declared,
    int Count,
    IReadOnlyList<string> Owners,
    bool IsReserved = false);

/// <summary>
///     收口报告的条目类型。
/// </summary>
public enum SlotIssueKind
{
    /// <summary>有附件附着，但对应槽位始终没有被贡献——通常是槽位名写错或提供方未启动。</summary>
    UnresolvedSlot,

    /// <summary>附件形态与槽位声明的形态不一致。</summary>
    KindMismatch,

    /// <summary>单值槽收到第二个附件，后来者被拒绝。</summary>
    CapacityExceeded,

    /// <summary>同一所有者把同一附件实例重复附到同一槽位。</summary>
    DuplicateAttachment,

    /// <summary>收口之后仍有附件附着；运行期动态面板属合法场景，只记警告。</summary>
    AttachedAfterClose,

    /// <summary>槽位已经贡献，但没有任何附件附着，且没有标记为预留。</summary>
    EmptySlot,

    /// <summary>内容工厂抛异常；异常已被隔离为占位，异常类型与首行进本条消息，堆栈进日志。</summary>
    FactoryException,
}

/// <summary>
///     一条槽位诊断。
/// </summary>
/// <param name="Kind">问题类型。</param>
/// <param name="SlotName">相关槽位名。</param>
/// <param name="OwnerId">相关附件所有者，可能为空。</param>
/// <param name="Message">可读说明。</param>
public sealed record SlotIssue(
    SlotIssueKind Kind,
    string SlotName,
    string? OwnerId,
    string Message);

/// <summary>
///     收口报告：槽位清单与全部诊断。
/// </summary>
/// <param name="Slots">槽位快照。</param>
/// <param name="Issues">诊断条目。</param>
public sealed record SlotDiagnostics(
    IReadOnlyList<SlotSnapshot> Slots,
    IReadOnlyList<SlotIssue> Issues)
{
    /// <summary>是否存在阻断性诊断（未解析槽位或类型不匹配）。</summary>
    public bool HasBlockingIssues => Issues.Any(issue =>
        issue.Kind is SlotIssueKind.UnresolvedSlot or SlotIssueKind.KindMismatch);
}
