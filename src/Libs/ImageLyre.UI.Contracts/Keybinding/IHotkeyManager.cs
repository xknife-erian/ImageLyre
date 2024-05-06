namespace ImageLyre.UI.Contracts.Keybinding;
/// <summary>
/// 应用内快捷键管理器（仅在主窗口激活时生效）。
/// 用于注册 Ctrl+S、F5 等标准快捷键。
/// </summary>
public interface IHotkeyManager
{
    /// <summary>
    /// 注册一个应用内快捷键。
    /// 该快捷键仅在主窗口拥有焦点时触发。
    /// </summary>
    /// <param name="id">快捷键的唯一标识符（用于后续注销）。</param>
    /// <param name="keyCombination">快捷键组合（如 Ctrl+Shift+N）。</param>
    /// <param name="handler">触发时执行的操作。</param>
    /// <returns>注销令牌：Dispose 即注销该快捷键（幂等），等价于 <see cref="UnregisterHotkey"/>。</returns>
    /// <exception cref="ArgumentException">参数无效时抛出。</exception>
    /// <exception cref="InvalidOperationException">重复注册相同 id 时抛出。</exception>
    IDisposable RegisterHotkey(string id, KeyCombination keyCombination, Action handler);

    /// <summary>
    /// 注销一个已注册的应用内快捷键。
    /// </summary>
    /// <param name="id">要注销的快捷键标识符。</param>
    /// <returns>是否成功注销。</returns>
    bool UnregisterHotkey(string id);

    /// <summary>
    /// 获取所有已注册的应用内快捷键 ID 列表。
    /// </summary>
    IReadOnlyList<string> RegisteredHotkeys { get; }
}
