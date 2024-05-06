using Cangming.PicoPlugins.Core.Features;

namespace ImageLyre.UI.Contracts.Slots;

/// <summary>
///     附件：在沧溟的空标记接口之上补"形态"、"所有者"与"排序权重"三项。
/// </summary>
/// <remarks>
///     沧溟的 <see cref="IFeatureAttachment" /> 是空标记，注释明确"附着物是渲染层读得懂的形状，由各落地包在自己的路线内定义"，
///     因此本仓每种附件都实现本接口。所有者标识不要求 Feature 手填：管理器在 <c>RegisterAttachments</c> 期间
///     通过 <see cref="ISlotRegistry.BeginOwner" /> 建立上下文后自动补齐。
/// </remarks>
public interface ISlotAttachment : IFeatureAttachment
{
    /// <summary>附件形态，必须与目标槽位声明的 <see cref="SlotDescriptor.Kind" /> 一致。</summary>
    SlotKind Kind { get; }

    /// <summary>附件所有者标识，由管理器在附着时补齐。</summary>
    string OwnerId { get; set; }

    /// <summary>排序权重，值小者靠前；同一槽位内按本值升序渲染。</summary>
    int Order { get; }
}
