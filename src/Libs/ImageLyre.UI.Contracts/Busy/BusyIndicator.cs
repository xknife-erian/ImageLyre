namespace ImageLyre.UI.Contracts.Busy;

/// <summary>
///     忙指示器配置。
///     描述一个忙任务的可视化表现形式。
/// </summary>
public class BusyIndicator
{
    /// <summary>
    ///     忙指示器的唯一标识（自动生成）。
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    ///     忙指示器的类型（可组合，如同时显示旋转图标和状态栏消息）。
    /// </summary>
    public required BusyIndicatorType Type { get; init; }

    /// <summary>
    ///     初始消息文本。
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    ///     进度值的有效范围（默认 0~100）。
    /// </summary>
    public (double, double) ProgressRange { get; init; } = new(0d, 100d);

    /// <summary>
    ///     当前进度值（只读，由外部通过 ReportProgress 更新）。
    /// </summary>
    public uint CurrentProgress { get; set; }
}