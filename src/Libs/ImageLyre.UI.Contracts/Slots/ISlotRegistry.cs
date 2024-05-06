using Cangming.PicoPlugins.Core.Features;

namespace ImageLyre.UI.Contracts.Slots;

/// <summary>
///     槽位注册表：既是贡献者，也是容器。
/// </summary>
/// <remarks>
///     <para>
///         实现沧溟的 <see cref="IAttachmentRegistry" />，因此宿主的 <c>RegisterFeatureAttachments</c> 无需改动；
///         在此之上额外提供描述符、摘除、所有者上下文、收口报告与快照五项能力。
///     </para>
///     <para>
///         槽位不设中心清单：谁拥有界面区域，谁调用 <see cref="ContributeSlot(SlotDescriptor)" /> 贡献槽位；
///         谁要挂内容，谁调用 <see cref="IAttachmentRegistry.AttachToSlot" /> 附着。附着到尚未贡献的槽位不抛异常，
///         而是进入待解析状态，等对应槽位被贡献时冲刷。
///     </para>
/// </remarks>
public interface ISlotRegistry : IAttachmentRegistry
{
    /// <summary>贡献一个带描述符的槽位。</summary>
    /// <param name="descriptor">槽位描述符。</param>
    /// <exception cref="InvalidOperationException">同名槽位已存在。</exception>
    void ContributeSlot(SlotDescriptor descriptor);

    /// <summary>按所有者的实例标识摘除其全部附件，含尚未解析的待附着项。</summary>
    /// <param name="ownerInstance">所有者的实例标识。</param>
    /// <returns>被摘除的附件数量。</returns>
    int Detach(string ownerInstance);

    /// <summary>
    ///     建立所有者上下文：框架遍历 Feature 调用 <c>RegisterAttachments</c> 时由宿主包一层，
    ///     期间附着的附件自动带上该所有者。
    /// </summary>
    /// <param name="ownerId">所有者标识，同时作为类型与实例。</param>
    /// <returns>释放后恢复上一个所有者上下文。</returns>
    IDisposable BeginOwner(string ownerId);

    /// <summary>按类型与实例建立所有者上下文，类型用于诊断归组，实例用于生命周期释放。</summary>
    /// <param name="ownerType">所有者类型标识，通常是 Feature 类型名。</param>
    /// <param name="ownerInstance">Feature 实例标识。</param>
    /// <returns>释放后恢复上一个所有者上下文。</returns>
    IDisposable BeginOwner(string ownerType, string ownerInstance);

    /// <summary>收口：解析剩余待定槽位、汇总诊断，并把槽位标记为已关闭。</summary>
    /// <remarks>幂等：重复调用返回同一份报告。</remarks>
    /// <returns>槽位清单与诊断。</returns>
    SlotDiagnostics CompleteRegistration();

    /// <summary>只读快照，供插件中心与诊断展示；包含已声明与仍待解析的槽位。</summary>
    /// <returns>槽位快照列表。</returns>
    IReadOnlyList<SlotSnapshot> Snapshot();

    /// <summary>登记一条槽位诊断。</summary>
    /// <remarks>
    ///     供渲染层回报运行期问题（例如内容附件的工厂抛异常），与附着阶段的校验共用同一份报告，
    ///     插件中心据此可见。
    /// </remarks>
    /// <param name="issue">诊断条目。</param>
    void ReportIssue(SlotIssue issue);
}
