using System.Reactive.Linq;
using FluentAssertions;
using ImageLyre.Contracts.Diagnostics;
using ImageLyre.Contracts.Platform;
using ImageLyre.Module.Performance.Internal;
using ImageLyre.Module.Platform.Internal;
using Xunit;

namespace ImageLyre.Module.Performance.Tests;

/// <summary>守住性能快照的时间、资源口径、CPU 区间与构造函数零依赖求值。</summary>
public sealed class PerformanceMonitorTests
{
    [Fact(DisplayName = "1.快照时间使用 UTC，工作集、托管堆、线程数与运行时长均为正")]
    public void Test001()
    {
        // Arrange
        var monitor = CreateMonitor();

        // Act
        PerformanceSnapshot snapshot = monitor.GetPerformanceSnapshot();

        // Assert
        snapshot.Timestamp.Offset.Should().Be(TimeSpan.Zero);
        snapshot.WorkingSetMb.Should().BeGreaterThan(0);
        snapshot.ManagedHeapMb.Should().BeGreaterThan(0);
        snapshot.ThreadCount.Should().BeGreaterThan(0);
        snapshot.Uptime.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact(DisplayName = "2.首次 CPU 为 0，连续第二次落在 0–100 之间")]
    public void Test002()
    {
        // Arrange
        var monitor = CreateMonitor();

        // Act
        PerformanceSnapshot first = monitor.GetPerformanceSnapshot();
        PerformanceSnapshot second = monitor.GetPerformanceSnapshot();

        // Assert
        first.CpuPercent.Should().Be(0);
        second.CpuPercent.Should().BeInRange(0, 100);
    }

    [Fact(DisplayName = "3.SnapshotUpdated 订阅后不产生任何元素")]
    public void Test003()
    {
        // Arrange
        var monitor = CreateMonitor();
        var observed = new List<PerformanceSnapshot>();

        // Act
        using var subscription = monitor.SnapshotUpdated.Subscribe(observed.Add);

        // Assert
        observed.Should().BeEmpty();
    }

    [Fact(DisplayName = "4.构造函数不读取平台能力依赖")]
    public void Test004()
    {
        // Arrange
        bool invoked = false;

        // Act
        _ = new PerformanceMonitor(() =>
        {
            invoked = true;
            return new PlatformCapabilities(OperatingSystemKind.Windows);
        });

        // Assert
        invoked.Should().BeFalse();
    }

    private static PerformanceMonitor CreateMonitor() =>
        new(() => new PlatformCapabilities(OperatingSystemKind.Windows));
}
