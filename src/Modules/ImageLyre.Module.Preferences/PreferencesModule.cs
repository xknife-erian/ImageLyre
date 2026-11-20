using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Contracts.Preferences;
using ImageLyre.UI.Contracts.UserState;

namespace ImageLyre.Module.Preferences;

/// <summary>偏好与习惯：偏好是用户显式设的值，习惯是系统自动记的值，两者共用一个目录但键空间互不相通。</summary>
/// <remarks>
///     <para>
///         两个契约的方法签名完全相同，语义却不同，因此不用一个类实现两个接口：本模块实现
///         <see cref="IPreferencesManager"/>，习惯记忆由 <see cref="UserState"/> 属性给出独立的
///         <see cref="IUserStateMemory"/> 实例，避免一次查询串到另一个存储里。
///     </para>
///     <para>
///         线程模型：读写可在任意线程，落盘用信号量串行；变更流在调用线程上推送，订阅方自行调度。
///     </para>
/// </remarks>
[Plugin(
    id: "ImageLyre.Preferences.Module",
    name: "偏好与习惯",
    alias: "Preferences",
    description: "偏好存值、设置项声明与用户习惯记忆")]
public sealed class PreferencesModule : BasePicoModule<IPreferencesManager>, IPreferencesManager
{
    private readonly Subject<(string Key, object? OldValue, object? NewValue)> _preferenceChanged = new();
    private readonly Subject<string> _valueUpdated = new();
    private readonly SemaphoreSlim _diskLock = new(1, 1);
    private readonly Lazy<IPreferencesManager> _self;
    private Dictionary<string, JsonElement> _preferences = new(StringComparer.Ordinal);
    private Dictionary<string, JsonElement> _userState = new(StringComparer.Ordinal);
    private string? _root;

    /// <summary>创建偏好模块。</summary>
    public PreferencesModule()
    {
        _self = new Lazy<IPreferencesManager>(() => this);
        UserState = new UserStateStore(this);
    }

    /// <summary>获取用户习惯记忆。</summary>
    public IUserStateMemory UserState { get; }

    /// <inheritdoc />
    public IObservable<(string Key, object? OldValue, object? NewValue)> PreferenceChanged => _preferenceChanged.AsObservable();

    /// <inheritdoc />
    public IReadOnlyList<string> AllKeys => _preferences.Keys.ToArray();

    /// <inheritdoc />
    public override Lazy<IPreferencesManager> Build(params object[] args) => _self;

    /// <summary>覆盖落盘根目录，测试用。</summary>
    /// <param name="directory">根目录；为空时回落到本机的应用数据目录。</param>
    public void UseStorageRoot(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        _root = directory;
        Load();
    }

    /// <inheritdoc />
    public T GetValue<T>(string key, T defaultValue = default!)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        EnsureLoaded();
        return _preferences.TryGetValue(key, out var element)
            ? element.Deserialize<T>() ?? defaultValue
            : defaultValue;
    }

    /// <inheritdoc />
    public void SetValue<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        EnsureLoaded();
        var old = _preferences.TryGetValue(key, out var element) ? element.Deserialize<object>() : null;
        _preferences[key] = JsonSerializer.SerializeToElement(value);

        if (!Equals(old, value))
        {
            _preferenceChanged.OnNext((key, old, value));
        }
    }

    /// <inheritdoc />
    public bool Contains(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        EnsureLoaded();
        return _preferences.ContainsKey(key);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        EnsureLoaded();
        if (_preferences.Remove(key, out var old))
        {
            _preferenceChanged.OnNext((key, old.Deserialize<object>(), null));
        }
    }

    /// <inheritdoc />
    public async Task FlushAsync()
    {
        EnsureLoaded();
        await _diskLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await WriteAsync("preferences.json", _preferences).ConfigureAwait(false);
        }
        finally
        {
            _diskLock.Release();
        }
    }

    /// <inheritdoc />
    public IDictionary<string, object?> ExportAll()
    {
        EnsureLoaded();
        return _preferences.ToDictionary(p => p.Key, p => p.Value.Deserialize<object?>(), StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public void ImportAll(IDictionary<string, object?> preferences, bool overwriteExisting = true)
    {
        ArgumentNullException.ThrowIfNull(preferences);

        EnsureLoaded();
        foreach (var (key, value) in preferences)
        {
            if (!overwriteExisting && _preferences.ContainsKey(key))
            {
                continue;
            }

            SetValue(key, value);
        }
    }

    private void EnsureLoaded()
    {
        if (_root is null)
        {
            _root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ImageLyre");
            Load();
        }
    }

    private void Load()
    {
        _preferences = Read("preferences.json");
        _userState = Read("userstate.json");
    }

    private void SaveUserState()
    {
        EnsureLoaded();
        _diskLock.Wait();
        try
        {
            WriteAsync("userstate.json", _userState).GetAwaiter().GetResult();
        }
        finally
        {
            _diskLock.Release();
        }
    }

    private Dictionary<string, JsonElement> Read(string fileName)
    {
        var path = Path.Combine(_root!, fileName);
        if (!File.Exists(path))
        {
            return new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }

        var text = File.ReadAllText(path);
        return string.IsNullOrWhiteSpace(text)
            ? new Dictionary<string, JsonElement>(StringComparer.Ordinal)
            : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(text) ?? new(StringComparer.Ordinal);
    }

    private async Task WriteAsync(string fileName, Dictionary<string, JsonElement> values)
    {
        Directory.CreateDirectory(_root!);
        var path = Path.Combine(_root!, fileName);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(values)).ConfigureAwait(false);
    }

    /// <summary>用户习惯记忆：窗口几何、最近文件与面板可见性这类系统自动记的值。</summary>
    private sealed class UserStateStore(PreferencesModule owner) : IUserStateMemory
    {
        public IObservable<string> ValueUpdated => owner._valueUpdated.AsObservable();

        public T GetValue<T>(string key, T defaultValue = default!)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            owner.EnsureLoaded();
            return owner._userState.TryGetValue(key, out var element)
                ? element.Deserialize<T>() ?? defaultValue
                : defaultValue;
        }

        public void RecordValue<T>(string key, T value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);

            owner.EnsureLoaded();
            owner._userState[key] = JsonSerializer.SerializeToElement(value);
            owner._valueUpdated.OnNext(key);
        }

        public void Save() => owner.SaveUserState();
    }
}
