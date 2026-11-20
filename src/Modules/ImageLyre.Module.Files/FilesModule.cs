using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Contracts.Files;

namespace ImageLyre.Module.Files;

/// <summary>外部变更通知：把系统的文件监视统一成一条事件流，供打开中的文档判断是否被外部改动。</summary>
/// <remarks>
///     <para>
///         每个 watcher 标识对应一个 <see cref="FileSystemWatcher"/>；重复注册同一标识会先停掉旧的再建新的，
///         避免同一路径被两条监视重复上报。暂停只停止转发事件，不销毁监视器，恢复后不需要重新注册。
///     </para>
///     <para>
///         线程模型：事件由系统线程触发，本类不切换线程；订阅方（界面）自行封送回界面线程。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.Files.Module",
    name: "文件",
    alias: "Files",
    description: "外部文件变更通知")]
public sealed class FilesModule : BasePicoModule<IExternalChangeNotifier>, IExternalChangeNotifier, IDisposable
{
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new(StringComparer.Ordinal);
    private readonly Subject<FileChangeEvent> _fileChanged = new();
    private readonly Lazy<IExternalChangeNotifier> _self;
    private bool _disposed;

    /// <summary>创建文件模块。</summary>
    public FilesModule()
    {
        _self = new Lazy<IExternalChangeNotifier>(() => this);
    }

    /// <inheritdoc />
    public IObservable<FileChangeEvent> FileChanged => _fileChanged.AsObservable();

    /// <inheritdoc />
    public bool IsPaused { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<string> ActiveWatcherIds => _watchers.Keys.ToArray();

    /// <inheritdoc />
    public override Lazy<IExternalChangeNotifier> Build(params object[] args) => _self;

    /// <inheritdoc />
    public void Watch(string path, string watcherId, bool includeSubdirectories = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(watcherId);

        Unwatch(watcherId);

        var directory = Path.GetDirectoryName(path);
        var fileName = Path.GetFileName(path);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            return;
        }

        var watcher = new FileSystemWatcher(directory, string.IsNullOrEmpty(fileName) ? "*.*" : fileName)
        {
            IncludeSubdirectories = includeSubdirectories,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true,
        };

        watcher.Changed += (_, e) => Publish(watcherId, e.FullPath, FileChangeType.Modified);
        watcher.Created += (_, e) => Publish(watcherId, e.FullPath, FileChangeType.Created);
        watcher.Deleted += (_, e) => Publish(watcherId, e.FullPath, FileChangeType.Deleted);
        watcher.Renamed += (_, e) => Publish(watcherId, e.FullPath, FileChangeType.Renamed);

        _watchers[watcherId] = watcher;
    }

    /// <inheritdoc />
    public void Unwatch(string watcherId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(watcherId);

        if (_watchers.Remove(watcherId, out var watcher))
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
    }

    /// <inheritdoc />
    public void PauseAll() => IsPaused = true;

    /// <inheritdoc />
    public void ResumeAll() => IsPaused = false;

    /// <inheritdoc />
    public override void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var watcher in _watchers.Values)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _watchers.Clear();
        _fileChanged.Dispose();
        base.Dispose();
    }

    private void Publish(string watcherId, string fullPath, FileChangeType changeType)
    {
        if (IsPaused)
        {
            return;
        }

        _fileChanged.OnNext(new FileChangeEvent
        {
            WatcherId = watcherId,
            FullPath = fullPath,
            ChangeType = changeType,
        });
    }
}
