namespace ImageLyre.UI.Contracts.Notifications;

/// <summary>
///     应用内统一通知服务。
///     提供两种核心通知模式：
///     1. **模态对话框（Modal Dialogs）**：阻塞用户操作，需明确响应（如确认删除）。
///     2. **非模态提示（Non-Modal Notifications）**：不打断工作流，自动消失或可忽略（如“保存成功”）。
///     ⚠️ 设计说明：
///     - 所有方法线程安全，可在后台线程调用。
///     - 具体呈现形式（Avalonia MessageBox / Toast / Web Snackbar）由 Shell 实现决定（允许差异，不影响契约语义）。
///     - 鼓励优先使用非模态通知以提升用户体验；仅在必要时使用模态对话框。
/// </summary>
public interface INotificationService
{
#region 🗨️ 非模态通知（推荐优先使用）

    /// <summary>
    ///     显示一条临时成功提示（如“导出完成”），自动淡出。
    /// </summary>
    void NotifySuccess(string message);

    /// <summary>
    ///     显示一条临时警告提示（如“文件已损坏”），自动淡出。
    /// </summary>
    void NotifyWarning(string message);

    /// <summary>
    ///     显示一条临时错误提示（如“权限不足”），自动淡出。
    /// </summary>
    void NotifyError(string message);

    /// <summary>
    ///     显示一条普通信息提示（如“正在连接。..”），通常持久显示直至被覆盖或清除。
    /// </summary>
    void NotifyInfo(string message);

    /// <summary>
    ///     发布一条高度自定义的非模态通知（支持图标、持续时间、操作按钮等）。
    /// </summary>
    void Publish(Notification notification);

#endregion
#region ⛔ 模态对话框（谨慎使用）

    /// <summary>
    ///     显示一个标准模态消息框，并等待用户响应。
    ///     此方法会阻塞调用线程（在 UI 线程上表现为挂起当前操作）。
    /// </summary>
    /// <param name="settings">消息框配置。</param>
    /// <returns>用户选择的操作结果。</returns>
    OperationResult ShowMessageBox(ModalDialogSettings settings);

    /// <summary>
    ///     异步显示模态消息框（推荐用于 MVVM 命令中）。
    ///     避免直接阻塞 UI 线程，符合现代异步编程模型。
    /// </summary>
    Task<OperationResult> ShowMessageBoxAsync(ModalDialogSettings settings);

#endregion
#region 🧹 管理与控制

    /// <summary>
    ///     清除所有当前活跃的非模态通知（不影响模态对话框）。
    /// </summary>
    void ClearActiveNotifications();

    /// <summary>
    ///     打开通知中心（如有），查看历史通知。
    /// </summary>
    void OpenNotificationCenter();
}

/// <summary>
///     表示通知的语义类型，用于决定图标、颜色和行为策略。
/// </summary>
public enum NotificationType
{
    /// <summary>普通信息提示（蓝色/中性色）。</summary>
    Information,

    /// <summary>成功操作反馈（绿色 + ✅）。</summary>
    Success,

    /// <summary>潜在风险或非致命问题（橙色 + ⚠️）。</summary>
    Warning,

    /// <summary>错误或失败状态（红色 + ❌）。</summary>
    Error,

    /// <summary>自定义样式，由实现方根据上下文决定外观。</summary>
    Custom
}

/// <summary>
///     代表一条可配置的非模态通知。
///     适用于 Toast、Snackbar、状态栏提示、通知中心条目等。
/// </summary>
public record Notification
{
    /// <summary>
    ///     要显示的消息文本。必须非空。
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    ///     通知的语义类型，默认为 <see cref="NotificationType.Information" />。
    /// </summary>
    public NotificationType Type { get; init; } = NotificationType.Information;

    /// <summary>
    ///     通知自动关闭前的持续时间。
    ///     若为 <see langword="null" />，则表示持久显示（需手动关闭或被覆盖）。
    ///     建议值：3~10 秒。
    /// </summary>
    public TimeSpan? Duration { get; init; }

    /// <summary>
    ///     是否将此通知存入“通知中心”供后续查阅。
    ///     默认为 <see langword="false" />（临时提示不归档）。
    /// </summary>
    public bool IsPersistent { get; init; }

    /// <summary>
    ///     当用户手动关闭通知时触发的回调。
    ///     可用于清理资源或记录行为。
    /// </summary>
    public Action? OnDismissed { get; init; }

    /// <summary>
    ///     附加的交互式操作按钮（如“重试”、“查看详情”）。
    ///     最多建议 2 个，避免 UI 拥挤。
    /// </summary>
    public NotificationAction[]? Actions { get; init; }

    /// <summary>
    ///     可选的上下文对象，用于携带额外数据（如异常、文档 ID、任务句柄）。
    ///     通知实现方可据此提供深度链接或诊断信息。
    /// </summary>
    public object? Context { get; init; }
}

/// <summary>
///     表示通知中的一个可点击操作按钮。
/// </summary>
public record NotificationAction
{
    /// <summary>
    ///     按钮显示文本（如“重试”、“忽略”）。
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    ///     用户点击按钮时执行的操作。
    ///     注意：应在 UI 线程上安全调用。
    /// </summary>
    public required Action Handler { get; init; }
}

/// <summary>
///     模态消息框的用户操作结果。
///     与 Windows MessageBox 返回值兼容，便于迁移。
/// </summary>
public enum OperationResult
{
    /// <summary>无结果（例如对话框被强制关闭）。</summary>
    None = 0,

    /// <summary>用户点击了“确定”。</summary>
    OK = 1,

    /// <summary>用户点击了“取消”。</summary>
    Cancel = 2,

    /// <summary>用户点击了“是”。</summary>
    Yes = 6,

    /// <summary>用户点击了“否”。</summary>
    No = 7
}

/// <summary>
///     模态消息框的按钮布局选项。
/// </summary>
public enum ModalDialogButton
{
    /// <summary>仅显示“确定”按钮。</summary>
    OK,

    /// <summary>显示“确定”和“取消”按钮。</summary>
    OKCancel,

    /// <summary>显示“是”和“否”按钮。</summary>
    YesNo,

    /// <summary>显示“是”、“否”和“取消”按钮。</summary>
    YesNoCancel
}

/// <summary>
///     模态消息框的图标类型。
///     已移除过时或平台特定的别名（如 Hand/Stop），保留语义化名称。
/// </summary>
public enum ModalDialogImage
{
    /// <summary>不显示任何图标。</summary>
    None,

    /// <summary>错误图标（红色圆圈 + X）。</summary>
    Error,

    /// <summary>警告图标（黄色三角形 + !）。</summary>
    Warning,

    /// <summary>信息图标（蓝色圆圈 + i）。</summary>
    Information,

    /// <summary>
    ///     问号图标（⚠️ 不推荐使用）。
    ///     微软官方已不建议在新应用中使用问号图标，
    ///     因其语义模糊且易与帮助图标混淆。
    ///     仅保留用于向后兼容。
    /// </summary>
    Question
}

/// <summary>
///     模态消息框的完整配置。
/// </summary>
public record ModalDialogSettings
{
    /// <summary>
    ///     消息框标题（Caption）。
    /// </summary>
    public string Caption { get; init; } = string.Empty;

    /// <summary>
    ///     消息正文文本。
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    ///     按钮布局。
    /// </summary>
    public ModalDialogButton Button { get; init; } = ModalDialogButton.OK;

    /// <summary>
    ///     图标类型。
    /// </summary>
    public ModalDialogImage Icon { get; init; } = ModalDialogImage.None;

    /// <summary>
    ///     默认选中的按钮（用于带倒计时的自动确认场景）。
    /// </summary>
    public OperationResult DefaultResult { get; init; } = OperationResult.None;

    /// <summary>
    ///     是否显示倒计时（例如“5秒后自动关闭”）。
    ///     通常与 <see cref="DefaultResult" /> 配合使用。
    /// </summary>
    public bool ShowCountdown { get; init; }
}
#endregion
