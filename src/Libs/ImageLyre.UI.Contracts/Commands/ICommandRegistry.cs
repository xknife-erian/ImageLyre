using System.Reactive;
using ImageLyre.UI.Contracts.Keybinding;

namespace ImageLyre.UI.Contracts.Commands;

/// <summary>
/// 全局命令注册中心。提供命令的注册、发现、执行与状态查询。
/// 支持工具栏、菜单、命令面板（Command Palette）、快捷键统一绑定。
/// 
/// ⚠️ 设计原则：
/// - 命令与具体 UI 解耦：同一命令可同时绑定到菜单项、工具栏按钮和快捷键。
/// - 权限感知：命令可用性应受 <c>ImageLyre.Contracts.Security.IAuthorizationContext</c> 影响（由实现层负责）。
/// - 异步执行：命令可能触发 I/O 或长时间运算，支持 CancellationToken。
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// 注册一个具名命令。
    /// 若 CommandId 已存在，必须抛 <see cref="InvalidOperationException"/>（不允许静默覆盖）。
    /// </summary>
    /// <param name="descriptor">命令描述符（含 ID、显示名、图标、执行逻辑等）。</param>
    /// <returns>注销令牌：Dispose 即注销该命令（幂等），等价于 <see cref="UnregisterCommand"/>。</returns>
    /// <exception cref="ArgumentNullException">descriptor 为 null。</exception>
    /// <exception cref="InvalidOperationException">CommandId 已存在（重复注册）。</exception>
    IDisposable RegisterCommand(CommandDescriptor descriptor);

    /// <summary>
    /// 注销指定命令。若命令不存在则静默返回。
    /// </summary>
    /// <param name="commandId">命令唯一标识，如 "File.Save"。</param>
    void UnregisterCommand(string commandId);

    /// <summary>
    /// 获取所有当前可用的命令（已考虑权限过滤与 CanExecute 状态）。
    /// 用于填充命令面板、菜单生成器或工具栏配置器。
    /// </summary>
    IReadOnlyList<CommandDescriptor> GetAvailableCommands();

    /// <summary>
    /// 按命令 ID 异步执行。若命令不存在或当前不可执行，应抛出异常或返回失败结果。
    /// </summary>
    /// <param name="commandId">命令 ID。</param>
    /// <param name="parameter">可选执行参数（如当前选中对象）。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>命令执行结果。若命令无需返回值，可为空对象。</returns>
    /// <exception cref="ArgumentException">commandId 不存在或无效。</exception>
    /// <exception cref="InvalidOperationException">当前状态下不可执行。</exception>
    Task<CommandResult> ExecuteAsync(string commandId, object? parameter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查询命令在当前上下文是否可执行（CanExecute）。
    /// 不触发实际执行逻辑。
    /// </summary>
    /// <param name="commandId">命令 ID。</param>
    /// <param name="parameter">执行参数上下文。</param>
    /// <returns>若可执行返回 true；若命令不存在或不可执行返回 false。</returns>
    bool CanExecute(string commandId, object? parameter = null);

    /// <summary>
    /// 查找单个命令描述符（含执行逻辑）。
    /// </summary>
    /// <param name="commandId">命令 ID。</param>
    /// <returns>命令描述符；若不存在返回 null。</returns>
    CommandDescriptor? GetCommand(string commandId);

    /// <summary>
    /// 当可用命令集合（含注册/注销或 CanExecute 状态变化）变更时推送。
    /// </summary>
    /// <remarks>
    /// 触发线程：UI 线程（实现方负责调度）；回放：无（配合快照查询 <see cref="GetAvailableCommands"/> 使用，订阅后仅推后续变更）；hot。
    /// </remarks>
    IObservable<Unit> AvailableCommandsChanged { get; }
}

/// <summary>
/// 全局命令描述符。
/// </summary>
public record CommandDescriptor
{
    /// <summary>
    /// 命令唯一标识符。建议采用分层命名（如 "File.Save", "Edit.Copy", "View.ToggleRail"）。
    /// </summary>
    public required string CommandId { get; init; }

    /// <summary>
    /// 本地化显示名称（如 "保存"）。
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// 可选图标字元/路径（用于菜单和工具栏）。
    /// </summary>
    public string? IconGlyph { get; init; }

    /// <summary>
    /// 命令分类（如 "File", "Edit", "View", "Tools"）。用于菜单分组和命令面板筛选。
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// 默认快捷键组合（如 Ctrl+S、Ctrl+Shift+P）。
    /// 由 <see cref="IHotkeyManager"/> 直接绑定（强类型），无需字符串解析；
    /// 需要从字符串构造时使用 <see cref="KeyCombination.TryParse(string?, out KeyCombination)"/>。
    /// </summary>
    public KeyCombination? DefaultShortcut { get; init; }

    /// <summary>
    /// 命令执行体。参数为传入的 object? parameter。
    /// 若为 null，表示命令不可执行（纯占位/禁用）。
    /// </summary>
    public required Func<object?, CancellationToken, Task<CommandResult>> ExecuteAsync { get; init; }

    /// <summary>
    /// 可选：判断当前上下文是否可执行。
    /// 若为 null，默认始终可执行。
    /// </summary>
    public Func<object?, bool>? CanExecutePredicate { get; init; }


    /// <summary>
    /// 命令描述/提示文本（用于状态栏或工具提示）。
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// 命令执行结果。
/// </summary>
public record CommandResult
{
    /// <summary>
    /// 是否成功执行。
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// 可选返回值（如"打开文件"命令返回文件路径）。
    /// </summary>
    public object? ReturnValue { get; init; }

    /// <summary>
    /// 失败时的错误信息。
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 快捷工厂：成功结果。
    /// </summary>
    /// <param name="returnValue">成功时回传的值</param>
    public static CommandResult Success(object? returnValue = null) => new() { IsSuccess = true, ReturnValue = returnValue };

    /// <summary>
    /// 快捷工厂：失败结果。
    /// </summary>
    /// <param name="errorMessage">失败原因描述</param>
    public static CommandResult Failed(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
