using System.Windows.Input;
using ImageLyre.UI.Contracts.Slots;

namespace ImageLyre.UI.Contracts.Attachments;

/// <summary>
///     状态栏指示器附件。
/// </summary>
public sealed record StatusBarAttachment : ISlotAttachment
{
    /// <summary>显示文本。</summary>
    public required string Text { get; init; }

    /// <summary>悬浮提示。</summary>
    public string? ToolTip { get; init; }

    /// <summary>点击命令，可选；为空时该指示器不可点。</summary>
    public ICommand? Command { get; init; }

    /// <inheritdoc />
    public SlotKind Kind => SlotKind.StatusBar;

    /// <inheritdoc />
    public string OwnerId { get; set; } = string.Empty;

    /// <inheritdoc />
    public int Order { get; init; }
}
