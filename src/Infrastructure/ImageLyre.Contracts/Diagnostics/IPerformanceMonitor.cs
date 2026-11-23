namespace ImageLyre.Contracts.Diagnostics;

/// <summary>性能监控能力：应用级性能快照的采集入口。</summary>
/// <remarks>
/// <para>
/// 与日志职责分离：本接口只回答「此刻的资源占用是多少」，日志与问题面板的条目由各自的契约承载，
/// 把两者混在一个接口里会让诊断面板被迫依赖采样实现。
/// </para>
/// <para>
/// 线程与生命周期：<see cref="GetPerformanceSnapshot"/> 可从任意线程调用且不得抛异常，取不到某项指标时以 0 表示；
/// <see cref="SnapshotUpdated"/> 若由后台线程推送，订阅方自行调度到界面线程。实例由能力插件注册，调用方不释放。
/// </para>
/// <para>
/// 本接口抄录自 cangming 的 `Cangming.Platform.Contracts`（0.1.9.1122）并按本产品口径裁剪：
/// 去掉了快照里的文档数与 Rail 数两项——它们是壳的运行时概念，M1 没有来源，将来需要时按「只增不改」补回。
/// </para>
/// </remarks>
public interface IPerformanceMonitor
{
    /// <summary>采集一次性能快照。</summary>
    /// <returns>调用时刻的指标快照；实现可按需计算，不必持有后台采样线程。</returns>
    /// <remarks>
    /// 该方法是同步调用，实现不得在其中做长耗时统计；需要在界面刷新时周期读取的调用方自行控制频率。
    /// </remarks>
    PerformanceSnapshot GetPerformanceSnapshot();

    /// <summary>快照更新时推送。</summary>
    /// <remarks>按需计算型实现可以返回一条永不触发的流；订阅方不得把「没有事件」当成监控失效。</remarks>
    IObservable<PerformanceSnapshot> SnapshotUpdated { get; }
}

/// <summary>一个时刻的应用性能快照。</summary>
/// <remarks>
/// 数值口径固定：内存为兆字节，CPU 为 0 到 100 的百分比，时长为自进程启动起的累计值；
/// 单位与语言不随界面语言变化，换算与舍入由呈现方负责。
/// </remarks>
public sealed record PerformanceSnapshot
{
    /// <summary>采样时刻，统一用 UTC 记录。</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>进程工作集内存，单位兆字节。</summary>
    public double WorkingSetMb { get; init; }

    /// <summary>托管堆已用内存，单位兆字节。</summary>
    public double ManagedHeapMb { get; init; }

    /// <summary>CPU 使用率，取值 0 到 100；平台无法提供时为 0。</summary>
    public double CpuPercent { get; init; }

    /// <summary>当前线程数。</summary>
    public int ThreadCount { get; init; }

    /// <summary>自进程启动以来的运行时长。</summary>
    public TimeSpan Uptime { get; init; }
}
