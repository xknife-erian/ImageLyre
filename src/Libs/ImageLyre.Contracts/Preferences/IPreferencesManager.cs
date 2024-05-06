
namespace ImageLyre.Contracts.Preferences;

/// <summary>
/// 用户偏好设置管理器。提供持久化的键值存储，支持强类型读写与变更通知。
/// 与 Shell 层的 UI 布局持久化互补：UI 布局负责窗口几何，本接口负责业务逻辑设置。
/// 
/// ⚠️ 设计原则：
/// - 键名采用分层命名（如 "Editor.AutoSaveInterval", "Export.DefaultFormat"）。
/// - 支持 dot 分隔的层级，便于分类与批量导出。
/// - 线程安全：可在后台线程读写，实现负责内部同步与 UI 线程调度通知。
/// </summary>
public interface IPreferencesManager
{
    /// <summary>
    /// 获取当前值，若键不存在则返回 <paramref name="defaultValue"/>。
    /// </summary>
    /// <typeparam name="T">目标类型。实现应支持基本类型、string、enum 及 JSON 序列化对象。</typeparam>
    /// <param name="key">分层键名，如 "Editor.AutoSaveInterval"。</param>
    /// <param name="defaultValue">键不存在时的默认值。</param>
    /// <returns>存储值或默认值。</returns>
    T GetValue<T>(string key, T defaultValue = default!);

    /// <summary>
    /// 设置值并触发变更通知。
    /// 若新值与当前存储值相等（按 <typeparamref name="T"/> 的默认相等语义比较），必须跳过写入与通知。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="key">分层键名。</param>
    /// <param name="value">新值。</param>
    void SetValue<T>(string key, T value);

    /// <summary>
    /// 检查指定键是否存在且非 null。
    /// </summary>
    bool Contains(string key);

    /// <summary>
    /// 删除指定键。若键不存在则静默返回。
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// 获取所有已存储的键名（只读快照）。
    /// </summary>
    IReadOnlyList<string> AllKeys { get; }

    /// <summary>
    /// 当任意偏好设置变更时推送 (key, oldValue, newValue)。
    /// 订阅者可据此刷新 UI 或触发业务逻辑。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="AllKeys"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<(string Key, object? OldValue, object? NewValue)> PreferenceChanged { get; }

    /// <summary>
    /// 将所有待写入的偏好持久化到存储（通常自动调用，但可显式触发）。
    /// 适用于需要确保数据在关键操作前落盘的场景（如断电保护）。
    /// </summary>
    Task FlushAsync();

    /// <summary>
    /// 导出所有偏好设置为 JSON 兼容字典（便于备份、迁移或调试）。
    /// </summary>
    IDictionary<string, object?> ExportAll();

    /// <summary>
    /// 从字典批量导入偏好设置（如恢复备份）。
    /// 导入完成后应触发 <see cref="PreferenceChanged"/> 通知。
    /// </summary>
    /// <param name="preferences">键值字典。</param>
    /// <param name="overwriteExisting">是否覆盖已存在的键。</param>
    void ImportAll(IDictionary<string, object?> preferences, bool overwriteExisting = true);
}
