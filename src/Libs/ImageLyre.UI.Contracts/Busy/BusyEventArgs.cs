namespace ImageLyre.UI.Contracts.Busy;

/// <summary>
/// 忙状态事件参数：携带与本次忙状态事件关联的业务数据。
/// </summary>
/// <typeparam name="T">事件载荷类型（如 <see cref="BusyIndicator"/>、<see cref="BusyRecord"/>、<see cref="System.Guid"/>）。</typeparam>
public class BusyEventArgs<T> : EventArgs
{
    /// <summary>
    /// 创建忙状态事件参数。
    /// </summary>
    /// <param name="data">与事件关联的业务数据。</param>
    public BusyEventArgs(T data)
    {
        Data = data;
    }

    /// <summary>
    /// 与本次忙状态事件关联的业务数据。
    /// </summary>
    public T Data { get; }
}
