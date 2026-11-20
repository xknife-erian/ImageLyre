using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.UI.Contracts.Notifications;
using ContractNotification = ImageLyre.UI.Contracts.Notifications.Notification;
using ImageLyre.UI.Contracts.Output;

namespace ImageLyre.Module.Notifications;

/// <summary>通知与输出：一条通知在一处产生、两处可见；输出通道按名字归组，供面板按通道渲染。</summary>
/// <remarks>
///     <para>
///         两个契约不共用一个类：通知中心的条目有已读与删除语义，输出通道是有序文本流，
///         混在一起会让"清空通知"和"清空输出"互相牵连。
///     </para>
///     <para>
///         线程模型：所有成员可在任意线程调用，变更流在调用线程上推送；渲染层自行切回界面线程。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.Notifications.Module",
    name: "通知与输出",
    alias: "Notifications",
    description: "通知中心与输出通道")]
public sealed class NotificationsModule : BasePicoModule<INotificationCenter>, INotificationCenter, IDisposable
{
    private readonly List<NotificationEntry> _entries = [];
    private readonly Subject<Unit> _collectionChanged = new();
    private readonly Lazy<INotificationCenter> _self;

    /// <summary>创建通知模块。</summary>
    public NotificationsModule()
    {
        _self = new Lazy<INotificationCenter>(() => this);
        Output = new OutputChannelManager();
    }

    /// <summary>获取输出通道管理器。</summary>
    public IOutputChannelManager Output { get; }

    /// <inheritdoc />
    public IReadOnlyList<NotificationEntry> Entries => _entries.ToArray();

    /// <inheritdoc />
    public IObservable<Unit> CollectionChanged => _collectionChanged.AsObservable();

    /// <inheritdoc />
    public override Lazy<INotificationCenter> Build(params object[] args) => _self;

    /// <inheritdoc />
    public int UnreadCount => _entries.Count(e => !e.IsRead);

    /// <inheritdoc />
    public void Archive(ContractNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        _entries.Add(new NotificationEntry { Notification = notification });
        _collectionChanged.OnNext(Unit.Default);
    }

    /// <inheritdoc />
    public void MarkAsRead(Guid id)
    {
        var entry = _entries.FirstOrDefault(e => e.Id == id);
        if (entry is null || entry.IsRead)
        {
            return;
        }

        entry.IsRead = true;
        _collectionChanged.OnNext(Unit.Default);
    }

    /// <inheritdoc />
    public void MarkAllAsRead()
    {
        if (_entries.TrueForAll(e => e.IsRead))
        {
            return;
        }

        foreach (var entry in _entries)
        {
            entry.IsRead = true;
        }

        _collectionChanged.OnNext(Unit.Default);
    }

    /// <inheritdoc />
    public void Remove(Guid id)
    {
        if (_entries.RemoveAll(e => e.Id == id) > 0)
        {
            _collectionChanged.OnNext(Unit.Default);
        }
    }

    /// <inheritdoc />
    public void ClearAll()
    {
        if (_entries.Count == 0)
        {
            return;
        }

        _entries.Clear();
        _collectionChanged.OnNext(Unit.Default);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _collectionChanged.Dispose();
        ((IDisposable)Output).Dispose();
        base.Dispose();
    }

    /// <summary>把一条通知追加到语义分类，语义只影响呈现，不改变存储。</summary>
    /// <param name="message">通知文本。</param>
    /// <param name="type">语义类型。</param>
    public void Notify(string message, NotificationType type = NotificationType.Information) =>
        Archive(new ContractNotification { Message = message, Type = type, IsPersistent = true });

    /// <summary>输出通道管理器：按名字归组的有序文本流。</summary>
    public sealed class OutputChannelManager : IOutputChannelManager, IDisposable
    {
        private readonly Dictionary<string, OutputChannel> _channels = new(StringComparer.Ordinal);
        private readonly Subject<Unit> _channelsChanged = new();
        private readonly Subject<string> _showRequested = new();
        private IReadOnlyList<IOutputChannel>? _snapshot;

        /// <inheritdoc />
        public IReadOnlyList<IOutputChannel> Channels => _snapshot ??= _channels.Values.Cast<IOutputChannel>().ToArray();

        /// <inheritdoc />
        public IObservable<Unit> ChannelsChanged => _channelsChanged.AsObservable();

        /// <summary>获取请求显示某通道的流；壳订阅它把面板切到该通道。</summary>
        public IObservable<string> ShowRequested => _showRequested.AsObservable();

        /// <inheritdoc />
        public IOutputChannel GetOrCreateChannel(string id, string name) => (IOutputChannel)CreateChannel(id, name);

        /// <inheritdoc />
        public IOutputChannel? GetChannel(string id) =>
            _channels.TryGetValue(id, out var channel) ? channel : null;

        /// <summary>创建或取回同名通道。</summary>
        /// <param name="id">通道标识。</param>
        /// <param name="name">显示名。</param>
        /// <returns>通道实例。</returns>
        public IOutputChannel CreateChannel(string id, string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            if (_channels.TryGetValue(id, out var existing))
            {
                return existing;
            }

            var channel = new OutputChannel(id, name, _showRequested);
            _channels[id] = channel;
            _snapshot = null;
            _channelsChanged.OnNext(Unit.Default);
            return channel;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (var channel in _channels.Values)
            {
                ((IDisposable)channel).Dispose();
            }

            _channelsChanged.Dispose();
            _showRequested.Dispose();
        }

        /// <inheritdoc />
        public bool RemoveChannel(string id)
        {
            if (!_channels.Remove(id))
            {
                return false;
            }

            _snapshot = null;
            _channelsChanged.OnNext(Unit.Default);
            return true;
        }
    }

    /// <summary>输出通道：按追加顺序累积文本，并向前推增量片段。</summary>
    private sealed class OutputChannel(string id, string name, Subject<string> showRequested) : IOutputChannel, IDisposable
    {
        private readonly Subject<string> _textAppended = new();
        private readonly StringBuilder _buffer = new();

        public string Id { get; } = id;

        public string Name { get; } = name;

        public IObservable<string> TextAppended => _textAppended.AsObservable();

        /// <summary>获取当前累积的全文，供面板一次性读取。</summary>
        public string Text => _buffer.ToString();

        public void Append(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _buffer.Append(text);
            _textAppended.OnNext(text);
        }

        public void Clear() => _buffer.Clear();

        public void Show() => showRequested.OnNext(Id);

        /// <inheritdoc />
        public void Dispose() => _textAppended.Dispose();
    }
}
