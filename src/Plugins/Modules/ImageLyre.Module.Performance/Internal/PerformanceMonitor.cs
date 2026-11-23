using System.Diagnostics;
using System.Reactive.Linq;
using ImageLyre.Contracts.Diagnostics;
using ImageLyre.Contracts.Platform;

namespace ImageLyre.Module.Performance.Internal;

/// <summary>按需采样的性能监控实现：进程、GC 与 CPU 差值，取不到时按契约填 0。</summary>
/// <remarks>
/// <para>
/// 每次快照都现取 <see cref="Process"/> 与 <see cref="GC"/>，不启动后台线程；CPU 由两次调用之间的
/// <see cref="Process.TotalProcessorTime"/> 差值除以墙钟时间与逻辑核数得到，因此首次调用没有基线，返回 0。
/// </para>
/// <para>
/// 平台能力通过构造时传入的 <see cref="Func{TResult}"/> 在采样时读取，构造函数本身不触发求值；
/// 当前系统为 Unknown 时 CPU 记为 0。任一指标读取失败都只让该项为 0，不向调用方抛异常。
/// </para>
/// </remarks>
internal sealed class PerformanceMonitor : IPerformanceMonitor
{
    private readonly Func<IPlatformCapabilities> _capabilities;
    private readonly object _cpuGate = new();
    private DateTimeOffset? _lastCpuSampleAt;
    private TimeSpan _lastProcessorTime;

    /// <summary>创建一个按需采样的性能监控器。</summary>
    /// <param name="capabilities">平台能力的延迟访问器，不得为 <see langword="null"/>；构造函数不调用它。</param>
    internal PerformanceMonitor(Func<IPlatformCapabilities> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        _capabilities = capabilities;
    }

    /// <summary>采集一次性能快照。</summary>
    /// <returns>调用时刻的快照；单个指标失败时该项为 0，时间戳始终为 UTC。</returns>
    public PerformanceSnapshot GetPerformanceSnapshot()
    {
        DateTimeOffset timestamp = DateTimeOffset.UtcNow;
        var snapshot = new PerformanceSnapshot { Timestamp = timestamp };

        using Process? process = TryGetCurrentProcess();
        if (process is not null)
        {
            snapshot = snapshot with { WorkingSetMb = TryRead(() => ToMegabytes(process.WorkingSet64)) };
            snapshot = snapshot with { ThreadCount = TryRead(() => process.Threads.Count) };
            snapshot = snapshot with { Uptime = TryRead(() => NonNegative(timestamp - process.StartTime.ToUniversalTime())) };
            snapshot = snapshot with { CpuPercent = TryRead(() => NextCpuPercent(process, timestamp)) };
        }

        snapshot = snapshot with { ManagedHeapMb = TryRead(() => ToMegabytes(GC.GetTotalMemory(false))) };
        return snapshot;
    }

    /// <summary>永不触发的快照流：M1 按需采样，没有后台更新。</summary>
    public IObservable<PerformanceSnapshot> SnapshotUpdated => Observable.Never<PerformanceSnapshot>();

    /// <summary>计算本次 CPU 百分比；首次调用建立基线并返回 0。</summary>
    /// <param name="process">当前进程。</param>
    /// <param name="now">本次采样时间。</param>
    /// <returns>0 到 100 之间的 CPU 百分比。</returns>
    private double NextCpuPercent(Process process, DateTimeOffset now)
    {
        if (_capabilities().CurrentOS == OperatingSystemKind.Unknown)
            return 0;

        TimeSpan processorTime = process.TotalProcessorTime;

        lock (_cpuGate)
        {
            if (_lastCpuSampleAt is null)
            {
                // ponytail: 首次调用没有基线，按契约返回 0；第二次起才有 TotalProcessorTime 差值可算。
                _lastCpuSampleAt = now;
                _lastProcessorTime = processorTime;
                return 0;
            }

            TimeSpan wall = now - _lastCpuSampleAt.Value;
            TimeSpan cpu = processorTime - _lastProcessorTime;
            _lastCpuSampleAt = now;
            _lastProcessorTime = processorTime;

            if (wall <= TimeSpan.Zero)
                return 0;

            double percent = cpu.TotalMilliseconds / (wall.TotalMilliseconds * Environment.ProcessorCount) * 100d;
            return Math.Clamp(percent, 0d, 100d);
        }
    }

    /// <summary>取当前进程；取不到时返回 <see langword="null"/>。</summary>
    private static Process? TryGetCurrentProcess()
    {
        try
        {
            return Process.GetCurrentProcess();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>字节转兆字节，负数按 0 处理。</summary>
    private static double ToMegabytes(long bytes) => Math.Max(0, bytes) / (1024d * 1024d);

    /// <summary>把负的运行时长收敛到 0。</summary>
    private static TimeSpan NonNegative(TimeSpan value) => value > TimeSpan.Zero ? value : TimeSpan.Zero;

    /// <summary>读取单个指标，失败时返回类型的默认值。</summary>
    private static T TryRead<T>(Func<T> read)
    {
        try
        {
            return read();
        }
        catch
        {
            return default!;
        }
    }
}
