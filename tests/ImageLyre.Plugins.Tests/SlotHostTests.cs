using Avalonia.Controls;
using ImageLyre.Module.Workbench.Slots;
using ImageLyre.UI.Contracts.Attachments;
using ImageLyre.UI.Contracts.Slots;
using Xunit;

namespace ImageLyre.Plugins.Tests;

/// <summary>位置宿主：从注册表投影内容、随按所有者摘除清空、销毁后不再跟随变化。</summary>
public sealed class SlotHostTests
{
    private const string Slot = "ImageLyre.Test.HostSlot";

    [Fact]
    public void 附着前宿主不可见且内容为空()
    {
        // Arrange
        var registry = new SlotRegistry();
        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.Content));

        // Act
        using var host = new SlotHost(registry, Slot);

        // Assert
        Assert.Null(host.Content);
        Assert.False(host.IsVisible);
    }

    [Fact]
    public void 贡献位置并附着内容附件后宿主显示该控件()
    {
        // Arrange
        var registry = new SlotRegistry();
        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.Content));
        var content = new Border();
        using var host = new SlotHost(registry, Slot);

        // Act
        registry.AttachToSlot(Slot, ContentAttachment.FromFactory(() => content));

        // Assert
        Assert.Same(content, host.Content);
        Assert.True(host.IsVisible);
    }

    [Fact]
    public void 按所有者摘除后宿主内容被清空()
    {
        // Arrange
        var registry = new SlotRegistry();
        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.Content));
        var content = new Border();
        var created = 0;
        using var host = new SlotHost(registry, Slot);
        using (registry.BeginOwner("Feature.Host"))
        {
            registry.AttachToSlot(Slot, ContentAttachment.FromFactory(() =>
            {
                created++;
                return content;
            }));
        }

        // Act
        var removed = registry.Detach("Feature.Host");

        // Assert
        Assert.Equal(1, removed);
        // 工厂被调用过一次，说明摘除前宿主确实投影过这份内容，下面的"清空"不是空转。
        Assert.Equal(1, created);
        Assert.Null(host.Content);
        Assert.False(host.IsVisible);
    }

    [Fact]
    public void 宿主销毁后不再跟随槽位变化()
    {
        // Arrange
        var registry = new SlotRegistry();
        registry.ContributeSlot(new SlotDescriptor(Slot, SlotKind.Content));
        var original = new Border();
        var later = new Border();
        using var host = new SlotHost(registry, Slot);
        using (registry.BeginOwner("Feature.A"))
        {
            registry.AttachToSlot(Slot, ContentAttachment.FromFactory(() => original));
        }

        // Act：销毁后再附着、再摘除，宿主都不应重投影
        host.Dispose();
        using (registry.BeginOwner("Feature.B"))
        {
            registry.AttachToSlot(Slot, ContentAttachment.FromFactory(() => later));
        }

        registry.Detach("Feature.A");

        // Assert
        Assert.Same(original, host.Content);
        Assert.True(host.IsVisible);
    }
}
