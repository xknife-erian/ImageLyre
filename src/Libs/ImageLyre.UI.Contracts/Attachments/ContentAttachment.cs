using Avalonia.Controls;
using ImageLyre.UI.Contracts.Slots;

namespace ImageLyre.UI.Contracts.Attachments;

/// <summary>
///     内容附件：以工厂延迟创建控件，保留"点菜单才建面板"的语义。
/// </summary>
/// <remarks>
///     普通内容直接使用本类型；状态栏等具名区域的内容由派生类型固定 <see cref="Kind" />，
///     复用同一套渲染与释放路径。
/// </remarks>
public record ContentAttachment : ISlotAttachment
{
    /// <summary>控件工厂，渲染时才调用；返回的控件由渲染层放进目标区域。</summary>
    public required Func<Control> Create { get; init; }

    /// <summary>用工厂构造内容附件。</summary>
    /// <param name="create">控件工厂。</param>
    /// <returns>内容附件。</returns>
    public static ContentAttachment FromFactory(Func<Control> create)
    {
        ArgumentNullException.ThrowIfNull(create);

        return new ContentAttachment { Create = create };
    }

    /// <inheritdoc />
    /// <remarks>默认挂在内容槽位；具名区域的内容子类在这里固定为对应形态。</remarks>
    public virtual SlotKind Kind => SlotKind.Content;

    /// <inheritdoc />
    public string OwnerId { get; set; } = string.Empty;

    /// <inheritdoc />
    public int Order { get; init; }
}
