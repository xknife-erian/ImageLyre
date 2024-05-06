using System.ComponentModel;
using ImageLyre.UI.Contracts.Slots;

namespace ImageLyre.UI.Contracts.Attachments;

/// <summary>
///     工具窗格附件。
/// </summary>
public sealed record ToolPaneAttachment : ISlotAttachment
{
    /// <summary>窗格标题。</summary>
    public required string Title { get; init; }

    /// <summary>内容标识，用于重复打开时复用同一窗格。</summary>
    public string? ContentId { get; init; }

    /// <summary>视图模型工厂。</summary>
    public required Func<INotifyPropertyChanged> Create { get; init; }

    /// <inheritdoc />
    public SlotKind Kind => SlotKind.ToolArea;

    /// <inheritdoc />
    public string OwnerId { get; set; } = string.Empty;

    /// <inheritdoc />
    public int Order { get; init; }
}
