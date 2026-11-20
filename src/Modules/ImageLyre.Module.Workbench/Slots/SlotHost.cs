using System;
using System.Linq;
using Avalonia.Controls;
using ImageLyre.UI.Contracts.Slots;
using LyreContentAttachment = ImageLyre.UI.Contracts.Attachments.ContentAttachment;

namespace ImageLyre.Module.Workbench.Slots;

/// <summary>按位置名渲染内容的宿主控件：谁开位置，谁就给自己的每个子位置摆一个。</summary>
/// <remarks>
///     <para>
///         这是嵌套能成立的关键控件。面板功能在构建自己的界面时，为开出的每个子位置各摆一个
///         <see cref="SlotHost" />，别的功能往那个位置附着内容后，宿主自己订阅变化并投影出来，
///         面板不需要认识附着者的类型。
///     </para>
///     <para>
///         宿主只认第一个 <c>ContentAttachment</c>：单值位置够用，多值位置按附件顺序取第一个可见内容，
///         与"一屏一块主内容"的口径一致。位置本身的形态与容量校验发生在注册表，不在这里重复。
///     </para>
/// </remarks>
public sealed class SlotHost : ContentControl, IDisposable
{
    private readonly ISlotRegistry _registry;
    private readonly ISlotChangeSource? _changes;
    private readonly string _slotName;
    private bool _disposed;

    /// <summary>创建一个位置宿主。</summary>
    /// <param name="registry">槽位注册表。</param>
    /// <param name="slotName">要渲染的位置名。</param>
    public SlotHost(ISlotRegistry registry, string slotName)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);

        _registry = registry;
        _slotName = slotName;
        _changes = registry as ISlotChangeSource;

        if (_changes is not null)
        {
            _changes.Changed += OnChanged;
        }

        Render();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_changes is not null)
        {
            _changes.Changed -= OnChanged;
        }
    }

    private void OnChanged(object? sender, SlotChange e)
    {
        if (string.Equals(e.SlotName, _slotName, StringComparison.Ordinal))
        {
            Render();
        }
    }

    private void Render()
    {
        var attachment = (_changes?.GetAttachments(_slotName) ?? [])
            .Select(entry => entry.Attachment)
            .OfType<LyreContentAttachment>()
            .FirstOrDefault();

        Content = attachment?.Create();
        IsVisible = Content is not null;

        // 位置存在但暂时没人附着是正常状态，按预留位置处理，不报空槽。
        if (attachment is null && !IsReserved())
        {
            IsVisible = false;
        }
    }

    private bool IsReserved() => _registry.Snapshot().Any(s => s.Name == _slotName && s.IsReserved);
}
