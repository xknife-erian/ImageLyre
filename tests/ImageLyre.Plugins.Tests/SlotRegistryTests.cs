using System;
using System.Linq;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Module.Workbench.Slots;
using ImageLyre.UI.Contracts.Slots;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>槽位注册表的四条硬行为：待解析冲刷、按所有者摘除、形态校验与收口诊断。</summary>
public sealed class SlotRegistryTests
{
    private const string Slot = "ImageLyre.Test.Slot";

    [Fact]
    public void 附着到未贡献的槽位先入待解析并在贡献后冲刷()
    {
        var registry = new SlotRegistry();
        var attachment = new TestAttachment(SlotKind.Content);

        registry.AttachToSlot(Slot, attachment);
        Assert.Empty(registry.GetSlotContents(Slot));

        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.Content));

        Assert.Single(registry.GetSlotContents(Slot));
        Assert.Same(attachment, registry.GetSlotContents(Slot)[0]);
    }

    [Fact]
    public void 按所有者摘除只移除该所有者的附件()
    {
        var registry = new SlotRegistry();
        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.Content));

        using (registry.BeginOwner("Feature.A"))
        {
            registry.AttachToSlot(Slot, new TestAttachment(SlotKind.Content));
        }

        using (registry.BeginOwner("Feature.B"))
        {
            registry.AttachToSlot(Slot, new TestAttachment(SlotKind.Content));
        }

        Assert.Equal(2, registry.GetSlotContents(Slot).Count);

        var removed = registry.Detach("Feature.A");

        Assert.Equal(1, removed);
        var remaining = Assert.Single(registry.GetSlotContents(Slot));
        Assert.Equal("Feature.B", ((ISlotAttachment)remaining).OwnerId);
    }

    [Fact]
    public void 形态不符的附件被拒绝并记诊断()
    {
        var registry = new SlotRegistry();
        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.StatusBar));
        registry.AttachToSlot(Slot, new TestAttachment(SlotKind.Content));

        var report = registry.CompleteRegistration();

        Assert.Empty(registry.GetSlotContents(Slot));
        Assert.Contains(report.Issues, i => i.Kind == SlotIssueKind.KindMismatch);
    }

    [Fact]
    public void 收口时未贡献的槽位记为未解析()
    {
        var registry = new SlotRegistry();
        registry.AttachToSlot("ImageLyre.Never.Contributed", new TestAttachment(SlotKind.Content));

        var report = registry.CompleteRegistration();

        Assert.Contains(report.Issues, i => i.Kind == SlotIssueKind.UnresolvedSlot);
        Assert.True(report.HasBlockingIssues);
    }

    private sealed class TestAttachment(SlotKind kind) : IFeatureAttachment, ISlotAttachment
    {
        public SlotKind Kind { get; } = kind;

        public string OwnerId { get; set; } = string.Empty;

        public int Order => 0;
    }
}
