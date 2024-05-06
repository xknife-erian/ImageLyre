using System.ComponentModel;
using ImageLyre.UI.Contracts.Slots;

namespace ImageLyre.UI.Contracts.Attachments;

/// <summary>
///     文档页附件：以工厂延迟创建视图模型，由宿主壳打开到工作区。
/// </summary>
public sealed record DocumentAttachment : ISlotAttachment
{
    /// <summary>文档标题。</summary>
    public required string Title { get; init; }

    /// <summary>内容标识，用于重复打开时复用同一页。</summary>
    public string? ContentId { get; init; }

    /// <summary>视图模型工厂。</summary>
    public required Func<INotifyPropertyChanged> Create { get; init; }

    /// <inheritdoc />
    public SlotKind Kind => SlotKind.Document;

    /// <inheritdoc />
    public string OwnerId { get; set; } = string.Empty;

    /// <inheritdoc />
    public int Order { get; init; }
}
