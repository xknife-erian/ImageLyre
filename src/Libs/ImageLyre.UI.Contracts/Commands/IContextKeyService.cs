namespace ImageLyre.UI.Contracts.Commands;

/// <summary>
/// 全局上下文键服务。命令可用性表达式（when）的数据源。
/// 键命名规范：dot 分层（如 "editor.hasFocus"、"document.isDirty"、"user.isAdmin"）。
/// when 表达式语法（由 Shell 实现求值，契约只定义形状）：
/// 支持 &amp;&amp; / || / ! / == / != / 括号；true/false/null 字面量。
/// </summary>
public interface IContextKeyService
{
    /// <summary>
    /// 设置或更新指定上下文键的值。
    /// </summary>
    /// <param name="key">上下文键名（dot 分层，如 "editor.hasFocus"）。</param>
    /// <param name="value">键值；null 表示无值（等价于未设置，读取返回 null）。</param>
    void SetContextKey(string key, object? value);

    /// <summary>
    /// 读取指定上下文键的当前值；键不存在或值为 null 时返回 null。
    /// </summary>
    /// <param name="key">上下文键名。</param>
    object? GetContextKey(string key);

    /// <summary>移除上下文键。</summary>
    /// <param name="key">上下文键名。</param>
    void RemoveContextKey(string key);

    /// <summary>任意键变更时推送 (key, newValue)。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：BehaviorSubject（订阅即推当前值，无值则无快照可回放）；hot。</remarks>
    IObservable<(string Key, object? Value)> ContextKeyChanged { get; }
}
