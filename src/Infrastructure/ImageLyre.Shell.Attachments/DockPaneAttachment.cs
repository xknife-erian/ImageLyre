using Avalonia.Controls;
using Cangming.PicoPlugins.Core.Features;
using Cangming.Toolkit.Ordering;

namespace ImageLyre.Shell.Attachments;

/// <summary>辅助面板中的一页：标题显示为页签，内容挂到面板容器里。</summary>
/// <param name="Title">页签标题，不可为空，也不经本地化服务解析——显示文本由贡献方在构造前取好。</param>
/// <param name="Content">页内容控件，随窗口销毁回收；同一控件不得同时挂到两个面板页。</param>
/// <remarks>
/// <para>
/// 本类型属于应用词汇而非基座词汇：它只在 ImageLyre 的壳里有意义，因此不进 cangming；
/// 它承载 Avalonia 控件、实现机制与排序契约，因此也不能进 <c>ImageLyre.Shell.Contracts</c>——
/// 契约层一旦引用 UI 类型，基线 §10.2 的准入判定当场不成立。
/// </para>
/// <para>
/// 排序与容量：面板槽位允许多项，cangming 渲染器对 <c>OrderType.NoPreference</c> 一律返回相等、
/// 底层又是不稳定的就地排序，因此每个附着物都必须显式写 <see cref="Order"/>；M1 没有容量准入策略，
/// 同时挂两项也不会被拒绝，面板按 <see cref="Order"/> 呈现。
/// </para>
/// <para>
/// 生命周期：M1 不提供运行期摘除，内容控件与窗口同生命周期，关窗时由控件树统一回收；
/// 贡献方不得在附着后继续修改 <see cref="Content"/> 的父级，否则渲染层重读时会把它从旧容器里摘走。
/// </para>
/// </remarks>
public sealed record DockPaneAttachment(string Title, Control Content)
    : IFeatureAttachment, IOrderable
{
    /// <summary>面板内的排序位置；M1 要求每个附着物显式赋值，不依赖渲染器的相对顺序。</summary>
    public OrderRule Order { get; set; } = new(OrderType.NoPreference);
}
