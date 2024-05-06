using System.Reactive;
using ImageLyre.UI.Contracts.Commands;

namespace ImageLyre.UI.Contracts.Keybinding;

/// <summary>
/// 快捷键映射表：命令 ↔ 组合键的声明表、用户覆盖与冲突诊断。
/// 数据流：默认绑定（<see cref="CommandDescriptor.DefaultShortcut"/>）→ 用户覆盖（本接口）→ 冲突检测。
/// </summary>
public interface IKeybindingMap
{
    /// <summary>全部生效绑定（默认 + 用户覆盖合并后）。</summary>
    IReadOnlyList<KeybindingEntry> GetAll();

    /// <summary>查询指定命令的当前绑定；无绑定返回 null。</summary>
    /// <param name="commandId">命令唯一标识（如 "File.Save"）。</param>
    KeybindingEntry? FindByCommand(string commandId);

    /// <summary>查询指定组合键绑定的命令；无绑定返回 null。</summary>
    /// <param name="chord">组合键序列（支持多段，如 "Ctrl+K Ctrl+S"）。</param>
    KeybindingEntry? FindByChord(KeyChord chord);

    /// <summary>
    /// 整体替换默认绑定（宿主从命令注册中心的 DefaultShortcut 装配后调用）。
    /// 输入中 CommandId 重复抛 <see cref="ArgumentException"/>。
    /// </summary>
    /// <param name="entries">完整默认绑定集。</param>
    void LoadDefaults(IReadOnlyList<KeybindingEntry> entries);

    /// <summary>
    /// 设置用户覆盖绑定；binding 为 null 表示恢复默认。
    /// 若目标组合键已被其他命令占用，必须抛 <see cref="InvalidOperationException"/>
    /// （先解除占用再绑定；冲突是确定性错误，不做静默覆盖）。
    /// </summary>
    /// <param name="commandId">命令唯一标识。</param>
    /// <param name="binding">新的绑定；null 表示恢复默认绑定。</param>
    /// <exception cref="InvalidOperationException">目标组合键已被其他命令占用（确定性错误，不做静默覆盖）。</exception>
    void SetUserKeybinding(string commandId, KeyChord? binding);

    /// <summary>绑定集合变更时推送。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：无（配合快照查询 GetAll/FindByXxx 使用，订阅后仅推后续变更）；hot。</remarks>
    IObservable<Unit> KeybindingsChanged { get; }
}

/// <summary>单条绑定条目。</summary>
public record KeybindingEntry
{
    /// <summary>命令唯一标识（如 "File.Save"）。</summary>
    public required string CommandId { get; init; }

    /// <summary>绑定的组合键序列（支持多段）。</summary>
    public required KeyChord Chord { get; init; }

    /// <summary>可选 when 上下文键表达式（与 <see cref="CommandDescriptor.When"/> 同语法）；null 表示无条件。</summary>
    public string? When { get; init; }

    /// <summary>true = 用户覆盖；false = 默认绑定。</summary>
    public bool IsUserOverride { get; init; }
}
