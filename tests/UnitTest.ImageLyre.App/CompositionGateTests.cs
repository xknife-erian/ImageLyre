using FluentAssertions;
using ImageLyre.App.Startup;
using Xunit;

namespace ImageLyre.App.Tests;

/// <summary>守住 <see cref="CompositionGate"/> 的“同一进程编排一次、关停一次”契约。</summary>
public sealed class CompositionGateTests
{
    [Fact(DisplayName = "1.同一实例第二次 TryBegin 返回 false")]
    public void Test001()
    {
        // Arrange
        var gate = new CompositionGate();

        // Act
        bool first = gate.TryBegin();
        bool second = gate.TryBegin();

        // Assert
        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    [Fact(DisplayName = "2.同一实例第二次 TryStop 返回 false")]
    public void Test002()
    {
        // Arrange
        var gate = new CompositionGate();

        // Act
        bool first = gate.TryStop();
        bool second = gate.TryStop();

        // Assert
        first.Should().BeTrue();
        second.Should().BeFalse();
    }

    [Fact(DisplayName = "3.不同实例的编排与关停互不影响")]
    public void Test003()
    {
        // Arrange
        var first = new CompositionGate();
        var second = new CompositionGate();

        // Act
        bool firstBegin = first.TryBegin();
        bool secondBegin = second.TryBegin();
        bool firstStop = first.TryStop();
        bool secondStop = second.TryStop();
        bool firstRestart = first.TryBegin();
        bool secondRestart = second.TryBegin();

        // Assert
        firstBegin.Should().BeTrue();
        secondBegin.Should().BeTrue();
        firstStop.Should().BeTrue();
        secondStop.Should().BeTrue();
        firstRestart.Should().BeFalse();
        secondRestart.Should().BeFalse();
    }

    [Fact(DisplayName = "4.Parallel.For 并发下 TryBegin 与 TryStop 各只成功一次")]
    public void Test004()
    {
        // Arrange
        var gate = new CompositionGate();
        int beginCount = 0;
        int stopCount = 0;

        // Act
        Parallel.For(0, 64, _ =>
        {
            if (gate.TryBegin())
                Interlocked.Increment(ref beginCount);
        });

        Parallel.For(0, 64, _ =>
        {
            if (gate.TryStop())
                Interlocked.Increment(ref stopCount);
        });

        // Assert
        beginCount.Should().Be(1);
        stopCount.Should().Be(1);
    }

    [Fact(DisplayName = "5.TryStop 先于 TryBegin 时，后续 TryBegin 不得再开始")]
    public void Test005()
    {
        // Arrange
        var gate = new CompositionGate();

        // Act
        bool stopped = gate.TryStop();
        bool beganAfterStop = gate.TryBegin();
        bool stoppedAgain = gate.TryStop();

        // Assert
        stopped.Should().BeTrue();
        beganAfterStop.Should().BeFalse();
        stoppedAgain.Should().BeFalse();
        gate.IsStopped.Should().BeTrue();
    }
}
