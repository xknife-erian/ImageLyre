namespace ImageLyre.UI.Contracts.Busy;

/// <summary>
/// 忙指示器类型，支持位标志组合。
/// </summary>
[Flags]
public enum BusyIndicatorType
{
    /// <summary>
    ///     无
    /// </summary>
    Default = 1 << 0,

    /// <summary>
    ///     旋转指示器（spinner）
    /// </summary>
    Spinner = 1 << 1,

    /// <summary>
    ///     进度条
    /// </summary>
    Progress = 1 << 2,

    /// <summary>
    ///     状态栏提示
    /// </summary>
    StatusBarTip = 1 << 3
}
