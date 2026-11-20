using System;
using System.Linq;
using ImageLyre.Module.Notifications;
using ImageLyre.UI.Contracts.Notifications;
using ImageLyre.UI.Contracts.Output;
using Xunit;
using ContractNotification = ImageLyre.UI.Contracts.Notifications.Notification;

namespace ImageLyre.Plugins.Tests;

/// <summary>通知中心与输出通道的硬行为：归档、已读、清理与通道文本累积。</summary>
public sealed class NotificationsModuleTests
{
    [Fact]
    public void 归档后可标记已读并统计未读数()
    {
        var module = new NotificationsModule();
        module.Archive(new ContractNotification { Message = "已打开 sample.tif", IsPersistent = true });

        var entry = Assert.Single(module.Entries);
        Assert.False(entry.IsRead);
        Assert.Equal(1, module.UnreadCount);

        module.MarkAsRead(entry.Id);

        Assert.True(module.Entries[0].IsRead);
        Assert.Equal(0, module.UnreadCount);
    }

    [Fact]
    public void 全标记已读后再清空会回到零条()
    {
        var module = new NotificationsModule();
        module.Notify("一", NotificationType.Information);
        module.Notify("二", NotificationType.Warning);

        module.MarkAllAsRead();
        Assert.All(module.Entries, e => Assert.True(e.IsRead));

        module.ClearAll();
        Assert.Empty(module.Entries);
    }

    [Fact]
    public void 输出通道按名字唯一并推出文本增量()
    {
        var module = new NotificationsModule();
        var channel = module.Output.GetOrCreateChannel("build", "构建");
        var again = module.Output.GetOrCreateChannel("build", "构建");
        Assert.Same(channel, again);

        var observed = string.Empty;
        using var subscription = channel.TextAppended.Subscribe(text => observed += text);

        channel.Append("第一行");
        channel.Append("第二行");

        Assert.Equal("第一行第二行", observed);
        Assert.Single(module.Output.Channels);
    }

    [Fact]
    public void 移除通道后列表里不再出现()
    {
        var module = new NotificationsModule();
        module.Output.GetOrCreateChannel("build", "构建");

        Assert.True(module.Output.RemoveChannel("build"));

        Assert.DoesNotContain(module.Output.Channels, c => c.Id == "build");
        Assert.False(module.Output.RemoveChannel("build"));
    }
}
