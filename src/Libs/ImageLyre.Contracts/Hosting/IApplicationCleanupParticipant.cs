namespace ImageLyre.Contracts.Hosting;
/// <summary>
/// 应用退出前清理参与者。
/// 实现此接口的组件将在应用关闭流程中被自动调用，
/// 用于执行状态保存、资源释放等关键操作。
/// </summary>
/// <remarks>
/// 约定：
/// <list type="bullet">
///     <item>在 UI 线程调用。</item>
///     <item>应响应 <see cref="System.Threading.CancellationToken"/>（整体清理受 5 秒超时约束，超时由 Shell 取消）。</item>
///     <item>单个参与者抛出的异常由 Shell 隔离（记录日志）并继续执行其他参与者，不得中断关闭流程。</item>
/// </list>
/// </remarks>
public interface IApplicationCleanupParticipant
{
    /// <summary>
    /// 清理优先级：数值大者先执行；同优先级按注册顺序执行。默认 0。
    /// </summary>
    int Priority => 0;

    /// <summary>
    /// 在应用关闭前执行异步清理逻辑。
    /// 此方法在 UI 线程调用；应响应 <paramref name="cancellationToken"/>（整体 5 秒超时）。
    /// 单个参与者抛出的异常不会中断其他参与者（由 Shell 隔离并记录日志）。
    /// </summary>
    /// <param name="cancellationToken">关闭流程的整体取消令牌（5 秒超时）。</param>
    /// <returns>表示清理完成的任务。</returns>
    Task OnApplicationExitingAsync(CancellationToken cancellationToken);
}
