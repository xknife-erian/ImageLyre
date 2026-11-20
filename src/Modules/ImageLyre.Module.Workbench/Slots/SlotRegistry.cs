using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts.Slots;

namespace ImageLyre.Module.Workbench.Slots;

/// <summary>槽位注册表：既是沧溟的 <see cref="ISlotManager"/>，也是本仓的 <see cref="ISlotRegistry"/> 与变更源。</summary>
/// <remarks>
///     <para>
///         实现两套接口是为了让渲染层不动：沧溟的 <c>SlotPanel</c> 只认 <see cref="ISlotManager"/>，
///         而"停用插件即撤"需要按所有者摘除、待解析冲刷与诊断收口，这些能力沧溟的默认实现没有。
///     </para>
///     <para>
///         线程模型：注册与附着发生在插件启动阶段，视为单线程；变更事件在引发变化的那条线程上同步触发，
///         渲染层负责切回界面线程。所有公开成员只允许在注册阶段与界面线程调用。
///     </para>
/// </remarks>
public sealed class SlotRegistry : ISlotManager, ISlotRegistry, ISlotChangeSource
{
    private readonly Dictionary<string, SlotState> _slots = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<PendingAttachment>> _pending = new(StringComparer.Ordinal);
    private readonly List<SlotIssue> _issues = [];
    private readonly Stack<OwnerScope> _owners = new();
    private bool _closed;

    /// <inheritdoc />
    public event EventHandler<SlotContentChangedEventArgs>? SlotContentChanged;

    /// <inheritdoc />
    public event EventHandler<SlotChange>? Changed;

    private string CurrentOwnerType => _owners.Count > 0 ? _owners.Peek().OwnerType : "(unknown)";

    private string CurrentOwnerInstance => _owners.Count > 0 ? _owners.Peek().OwnerInstance : "(unknown)";

    /// <inheritdoc />
    public void ContributeSlot(string slotName, string? displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);

        ContributeSlot(new SlotDescriptor(slotName, SlotKind.Content, SlotCapacity.Multiple, displayName));
    }

    /// <inheritdoc />
    public void ContributeSlot(SlotDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (_slots.ContainsKey(descriptor.Name))
        {
            throw new InvalidOperationException($"槽位 {descriptor.Name} 已被贡献，不能重复声明。");
        }

        _slots[descriptor.Name] = new SlotState(descriptor);
        FlushPending(descriptor.Name);
    }

    /// <inheritdoc />
    public void AttachToSlot(string slotName, IFeatureAttachment attachment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);
        ArgumentNullException.ThrowIfNull(attachment);

        var ownerType = CurrentOwnerType;
        var ownerInstance = CurrentOwnerInstance;

        if (attachment is ISlotAttachment slotAttachment && string.IsNullOrEmpty(slotAttachment.OwnerId))
        {
            slotAttachment.OwnerId = ownerInstance;
        }

        if (!_slots.TryGetValue(slotName, out var slot))
        {
            if (!_pending.TryGetValue(slotName, out var queue))
            {
                queue = [];
                _pending[slotName] = queue;
            }

            queue.Add(new PendingAttachment(attachment, ownerType, ownerInstance));
            return;
        }

        AttachCore(slot, attachment, ownerType, ownerInstance);
    }

    /// <inheritdoc />
    public int Detach(string ownerInstance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerInstance);

        var removed = 0;
        foreach (var slot in _slots.Values)
        {
            var gone = slot.Entries.Where(e => string.Equals(e.OwnerInstance, ownerInstance, StringComparison.Ordinal)).ToArray();
            if (gone.Length == 0)
            {
                continue;
            }

            foreach (var entry in gone)
            {
                slot.Entries.Remove(entry);
            }

            removed += gone.Length;
            RaiseChanged(slot.Descriptor.Name, [], gone);
        }

        foreach (var queue in _pending.Values)
        {
            removed += queue.RemoveAll(p => string.Equals(p.OwnerInstance, ownerInstance, StringComparison.Ordinal));
        }

        return removed;
    }

    /// <inheritdoc />
    public IDisposable BeginOwner(string ownerId) => BeginOwner(ownerId, ownerId);

    /// <inheritdoc />
    public IDisposable BeginOwner(string ownerType, string ownerInstance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerType);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerInstance);

        var scope = new OwnerScope(_owners, ownerType, ownerInstance);
        _owners.Push(scope);
        return scope;
    }

    /// <inheritdoc />
    public SlotDiagnostics CompleteRegistration()
    {
        _closed = true;

        foreach (var (name, queue) in _pending)
        {
            if (queue.Count == 0)
            {
                continue;
            }

            _issues.Add(new SlotIssue(SlotIssueKind.UnresolvedSlot, name, queue[0].OwnerType,
                $"槽位 {name} 从未被贡献，{queue.Count} 个附件处于待解析状态。"));
        }

        foreach (var slot in _slots.Values)
        {
            if (slot.Entries.Count == 0 && !slot.Descriptor.IsReserved)
            {
                _issues.Add(new SlotIssue(SlotIssueKind.EmptySlot, slot.Descriptor.Name, null,
                    $"槽位 {slot.Descriptor.Name} 已贡献但没有任何附件。"));
            }
        }

        return new SlotDiagnostics(Snapshot(), _issues.ToArray());
    }

    /// <inheritdoc />
    public IReadOnlyList<SlotSnapshot> Snapshot()
    {
        var names = _slots.Keys
            .Concat(_pending.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(n => n, StringComparer.Ordinal);

        return names.Select(name =>
        {
            if (_slots.TryGetValue(name, out var slot))
            {
                var owners = slot.Entries.Select(e => e.OwnerId).Distinct(StringComparer.Ordinal).ToArray();
                return new SlotSnapshot(name, slot.Descriptor.Kind, slot.Descriptor.Capacity, Declared: true,
                    slot.Entries.Count, owners, slot.Descriptor.IsReserved);
            }

            var pendingCount = _pending[name].Count;
            return new SlotSnapshot(name, SlotKind.Content, SlotCapacity.Multiple, Declared: false, pendingCount, []);
        }).ToArray();
    }

    /// <inheritdoc />
    public void ReportIssue(SlotIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        _issues.Add(issue);
    }

    /// <inheritdoc />
    public IReadOnlyList<SlotAttachmentEntry> GetAttachments(string slotName)
    {
        if (!_slots.TryGetValue(slotName, out var slot))
        {
            return [];
        }

        return slot.Entries.ToArray();
    }

    /// <inheritdoc />
    public IImmutableList<SlotInfo> GetAllSlots() =>
        _slots.Values
            .Select(s => new SlotInfo(s.Descriptor.Name, s.Descriptor.DisplayName ?? s.Descriptor.Name))
            .ToImmutableList();

    /// <inheritdoc />
    public IReadOnlyList<IFeatureAttachment> GetSlotContents(string slotName)
    {
        if (!_slots.TryGetValue(slotName, out var slot))
        {
            return [];
        }

        return slot.Entries
            .OrderBy(e => e.Attachment is ISlotAttachment a ? a.Order : 0)
            .Select(e => e.Attachment)
            .ToArray();
    }

    /// <inheritdoc />
    public bool TryGetSlot(string slotName, out SlotInfo info)
    {
        if (_slots.TryGetValue(slotName, out var slot))
        {
            info = new SlotInfo(slot.Descriptor.Name, slot.Descriptor.DisplayName ?? slot.Descriptor.Name);
            return true;
        }

        info = null!;
        return false;
    }

    /// <inheritdoc />
    public void RefreshSlot(string slotName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slotName);

        SlotContentChanged?.Invoke(this, new SlotContentChangedEventArgs(slotName));
    }

    private void FlushPending(string slotName)
    {
        if (!_pending.TryGetValue(slotName, out var queue))
        {
            return;
        }

        var slot = _slots[slotName];
        var batch = queue.ToArray();
        queue.Clear();

        foreach (var pending in batch)
        {
            AttachCore(slot, pending.Attachment, pending.OwnerType, pending.OwnerInstance);
        }
    }

    private void AttachCore(SlotState slot, IFeatureAttachment attachment, string ownerType, string ownerInstance)
    {
        if (attachment is ISlotAttachment slotAttachment && slotAttachment.Kind != slot.Descriptor.Kind)
        {
            _issues.Add(new SlotIssue(SlotIssueKind.KindMismatch, slot.Descriptor.Name, ownerType,
                $"附件形态 {slotAttachment.Kind} 与槽位声明的 {slot.Descriptor.Kind} 不一致，已拒绝。"));
            return;
        }

        if (slot.Descriptor.Capacity == SlotCapacity.Exclusive && slot.Entries.Count > 0)
        {
            _issues.Add(new SlotIssue(SlotIssueKind.CapacityExceeded, slot.Descriptor.Name, ownerType,
                "单值槽只接受第一个附件，后来者被拒绝。"));
            return;
        }

        if (slot.Entries.Any(e => ReferenceEquals(e.Attachment, attachment)))
        {
            _issues.Add(new SlotIssue(SlotIssueKind.DuplicateAttachment, slot.Descriptor.Name, ownerType,
                "同一附件实例重复附着到同一槽位，已忽略。"));
            return;
        }

        if (_closed)
        {
            _issues.Add(new SlotIssue(SlotIssueKind.AttachedAfterClose, slot.Descriptor.Name, ownerType,
                "收口之后仍有附件附着，按运行期动态面板处理。"));
        }

        var entry = new SlotAttachmentEntry(slot.Descriptor.Name, ownerType, ownerInstance, attachment);
        slot.Entries.Add(entry);
        RaiseChanged(slot.Descriptor.Name, [entry], []);
    }

    private void RaiseChanged(string slotName, IReadOnlyList<SlotAttachmentEntry> added, IReadOnlyList<SlotAttachmentEntry> removed)
    {
        Changed?.Invoke(this, new SlotChange(slotName, added, removed));
        SlotContentChanged?.Invoke(this, new SlotContentChangedEventArgs(slotName));
    }

    private sealed class SlotState(SlotDescriptor descriptor)
    {
        internal SlotDescriptor Descriptor { get; } = descriptor;

        internal List<SlotAttachmentEntry> Entries { get; } = [];
    }

    private sealed record PendingAttachment(IFeatureAttachment Attachment, string OwnerType, string OwnerInstance);

    private sealed class OwnerScope(Stack<OwnerScope> stack, string ownerType, string ownerInstance) : IDisposable
    {
        internal string OwnerType { get; } = ownerType;

        internal string OwnerInstance { get; } = ownerInstance;

        public void Dispose()
        {
            if (stack.Count > 0 && ReferenceEquals(stack.Peek(), this))
            {
                stack.Pop();
            }
        }
    }
}
