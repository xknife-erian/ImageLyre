namespace ImageLyre.UI.Contracts.Busy;

/// <summary>
/// 忙任务的进度快照。
/// </summary>
/// <param name="Id">关联的忙任务 ID。</param>
/// <param name="CurrentProgress">当前进度值（应在 ProgressRange 范围内）。</param>
/// <param name="Message">当前状态消息（可为空）。</param>
public record struct BusyRecord(Guid Id, uint CurrentProgress, string Message)
{
    /// <summary>
    /// 创建一个表示“完成”的进度记录（哨兵值）。
    /// 仅用于本地进度展示语义（如 UI 将进度条置为 100%），
    /// 不得作为 <see cref="IBusyManager.ReportProgress"/> 的参数传入——完成状态必须通过
    /// <see cref="IBusyManager.ReportCompleted"/> / <see cref="IBusyManager.ReportFailed"/> 表达。
    /// </summary>
    /// <param name="message">日志或状态文本</param>
    public static BusyRecord Finish(string message = "") =>
        new(Guid.Empty, uint.MaxValue, message);
}