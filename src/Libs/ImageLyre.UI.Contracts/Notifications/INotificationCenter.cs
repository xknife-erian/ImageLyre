using System.Reactive;

namespace ImageLyre.UI.Contracts.Notifications;
/// <summary>
/// 应用内通知中心契约。
/// 
/// 负责管理所有已归档（persistent）的通知历史，提供查询、标记、清除和交互能力。
/// 
/// ⚠️ 设计原则：
/// - 仅存储设置了 <see cref="Notification.IsPersistent"/> = true 的通知。
/// - 所有方法线程安全，可在任意线程调用。
/// - 不直接负责 UI 渲染，但提供数据源供 Shell 实现绑定。
/// - 支持插件或服务主动推送重要事件到通知中心。
/// </summary>
public interface INotificationCenter
{
    /// <summary>
    /// 获取当前所有未清除的通知条目（按时间倒序排列，最新在前）。
    /// 可用于绑定到列表控件（如 ListBox / DataGrid）。
    /// </summary>
    IReadOnlyList<NotificationEntry> Entries { get; }

    /// <summary>
    /// 获取未读通知的数量（用于显示红点角标）。
    /// </summary>
    int UnreadCount { get; }

    /// <summary>
    /// 将一条通知手动添加到通知中心（即使其 IsPersistent=false）。
    /// 适用于系统级事件（如“许可证即将过期”）。
    /// </summary>
    /// <param name="notification">要归档的通知。</param>
    /// <exception cref="ArgumentNullException">notification 为 null。</exception>
    void Archive(Notification notification);

    /// <summary>
    /// 标记指定通知为“已读”。
    /// </summary>
    /// <param name="id">通知的唯一标识（来自 <see cref="NotificationEntry.Id"/>）。</param>
    void MarkAsRead(Guid id);

    /// <summary>
    /// 标记所有通知为“已读”。
    /// </summary>
    void MarkAllAsRead();

    /// <summary>
    /// 从通知中心移除指定通知。
    /// </summary>
    /// <param name="id">通知 ID。</param>
    void Remove(Guid id);

    /// <summary>
    /// 清空所有通知历史。
    /// </summary>
    void ClearAll();

    /// <summary>
    /// 当通知列表或未读数发生变化时推送（Unit 变更信号）。
    /// 便于 UI 层响应式更新（如 Avalonia 的 ReactiveUI 或 INotifyPropertyChanged）。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="Entries"/>、<see cref="UnreadCount"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<Unit> CollectionChanged { get; }
}

/// <summary>
/// 通知中心中的单个通知条目。
/// 包含原始通知内容及生命周期状态。
/// </summary>
public record NotificationEntry
{
    /// <summary>
    /// 唯一标识符，用于后续操作（如标记已读、删除）。
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// 原始通知内容。
    /// </summary>
    public required Notification Notification { get; init; }

    /// <summary>
    /// 通知被创建的时间（UTC）。
    /// </summary>
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// 是否已被用户标记为“已读”。
    /// 新增条目默认为 false。
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 格式化后的时间显示文本（如“2分钟前”），由实现方计算。
    /// 可选，便于 UI 直接绑定。
    /// </summary>
    public string? RelativeTimeText { get; init; }
}
