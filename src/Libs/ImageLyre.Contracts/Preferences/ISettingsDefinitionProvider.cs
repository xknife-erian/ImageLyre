using System.Reactive;

namespace ImageLyre.Contracts.Preferences;

/// <summary>设置值类型。</summary>
public enum SettingsValueType
{
    /// <summary>字符串。</summary>
    Text,
    /// <summary>数值。</summary>
    Number,
    /// <summary>布尔。</summary>
    Boolean,
    /// <summary>枚举（候选值见 <see cref="SettingsDefinition.EnumValues"/>）。</summary>
    Enum,
}

/// <summary>
/// 设置项定义（设置 UI 的数据源，对标 VS Code contributes.configuration）。
/// 与 <c>ImageLyre.Contracts.Preferences.IPreferencesManager</c> 职责分离：Preferences 管值的存储读写，
/// 本接口管设置项的元数据（类型/默认值/范围/分组/枚举候选）。
/// </summary>
public interface ISettingsDefinitionProvider
{
    /// <summary>全部设置项定义。</summary>
    IReadOnlyList<SettingsDefinition> Definitions { get; }

    /// <summary>按 key 查询；不存在返回 null。</summary>
    /// <param name="key">设置键（dot 分层，如 "Editor.AutoSaveInterval"）。</param>
    SettingsDefinition? GetDefinition(string key);

    /// <summary>
    /// 注册或替换一条设置项定义（宿主在启动装配或插件加载时调用）；同 Key 已存在时替换。
    /// 命名对齐 <c>ICommandRegistry.RegisterCommand</c> 惯例。
    /// </summary>
    /// <param name="definition">设置项定义。</param>
    /// <exception cref="ArgumentNullException">definition 为 null。</exception>
    void Register(SettingsDefinition definition);

    /// <summary>
    /// 移除一条设置项定义；不存在静默返回。
    /// </summary>
    /// <param name="key">设置键。</param>
    void Unregister(string key);

    /// <summary>定义集合变更时推送。</summary>
    /// <remarks>触发线程：UI 线程（实现方负责调度）；回放：无（配合快照属性 <see cref="Definitions"/> 使用，订阅后仅推后续变更）；hot。</remarks>
    IObservable<Unit> DefinitionsChanged { get; }
}

/// <summary>单个设置项定义。</summary>
public record SettingsDefinition
{
    /// <summary>设置键（dot 分层，如 "Editor.AutoSaveInterval"）。</summary>
    public required string Key { get; init; }

    /// <summary>本地化显示名（或本地化资源键）。</summary>
    public required string DisplayName { get; init; }

    /// <summary>设置项说明（可空）。</summary>
    public string? Description { get; init; }

    /// <summary>值类型（决定设置 UI 呈现的控件形态）。</summary>
    public required SettingsValueType ValueType { get; init; }

    /// <summary>默认值；类型须与 <see cref="ValueType"/> 匹配。</summary>
    public object? DefaultValue { get; init; }

    /// <summary>UI 分组（如 "编辑器"、"导出"）。</summary>
    public string? Category { get; init; }

    /// <summary>作用域："user" / "workspace"。</summary>
    public string? Scope { get; init; }

    /// <summary><see cref="ValueType"/> = Enum 时的候选值（显示名，按顺序）。</summary>
    public IReadOnlyList<string>? EnumValues { get; init; }

    /// <summary><see cref="ValueType"/> = Number 时的取值范围（含边界）。</summary>
    public (double Min, double Max)? Range { get; init; }
}
