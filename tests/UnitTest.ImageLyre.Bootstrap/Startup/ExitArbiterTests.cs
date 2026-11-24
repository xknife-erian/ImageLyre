using System.Collections.Concurrent;
using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住启动关停的并发契约：关闭动作只执行一次、返回同一个任务、停止可先于开始、超时有日志且异常不被吞掉。</summary>
public sealed class ExitArbiterTests
{
    [Fact(DisplayName = "1.64 个并发 StopAsync 只执行一次关闭动作并返回同一个任务")]
    public async Task Test001()
    {
        // Arrange
        int calls = 0;
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var arbiter = new ExitArbiter(
            async _ => {
                Interlocked.Increment(ref calls);
                started.TrySetResult();
                await release.Task;
            },
            TimeSpan.FromSeconds(30),
            NullLogger<ExitArbiter>.Instance);
        var results = new ConcurrentBag<Task>();

        // Act
        Parallel.For(0, 64, _ => results.Add(arbiter.StopAsync()));
        await started.Task;
        release.SetResult();
        await Task.WhenAll(results);

        // Assert
        calls.Should().Be(1);
        Task first = results.First();
        results.Should().AllSatisfy(task => task.Should().BeSameAs(first));
    }

    [Fact(DisplayName = "2.StopAsync 先于 TryBegin 时关闭动作执行一次，之后 TryBegin 一律返回 false")]
    public async Task Test002()
    {
        // Arrange
        int calls = 0;
        var arbiter = new ExitArbiter(
            _ => {
                Interlocked.Increment(ref calls);
                return Task.CompletedTask;
            },
            TimeSpan.FromSeconds(5),
            NullLogger<ExitArbiter>.Instance);

        // Act
        await arbiter.StopAsync();

        // Assert
        calls.Should().Be(1);
        arbiter.IsStopped.Should().BeTrue();
        arbiter.TryBegin().Should().BeFalse();
    }

    [Fact(DisplayName = "3.关闭动作超出超时时间时记录警告并抛出 TimeoutException")]
    public async Task Test003()
    {
        // Arrange
        int calls = 0;
        var loggerFactory = new RecordingLoggerFactory();
        var arbiter = new ExitArbiter(
            async cancellationToken => {
                Interlocked.Increment(ref calls);
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            },
            TimeSpan.FromMilliseconds(100),
            loggerFactory.CreateLogger<ExitArbiter>());

        // Act
        Task first = arbiter.StopAsync();

        // Assert
        await first.Awaiting(task => task).Should().ThrowAsync<TimeoutException>();
        calls.Should().Be(1);
        loggerFactory.Messages.Should().Contain(message => message.Contains("超时", StringComparison.Ordinal));
        arbiter.StopAsync().Should().BeSameAs(first);
        calls.Should().Be(1);
    }

    [Fact(DisplayName = "4.关闭动作抛出的原始异常原样传播，重复 StopAsync 不重跑动作且返回同一任务")]
    public async Task Test004()
    {
        // Arrange
        int calls = 0;
        var failure = new InvalidOperationException("插件停止失败");
        var arbiter = new ExitArbiter(
            _ => {
                Interlocked.Increment(ref calls);
                throw failure;
            },
            TimeSpan.FromSeconds(5),
            NullLogger<ExitArbiter>.Instance);

        // Act
        Task first = arbiter.StopAsync();
        Task second = arbiter.StopAsync();

        // Assert
        second.Should().BeSameAs(first);
        (await first.Awaiting(task => task).Should().ThrowAsync<InvalidOperationException>())
            .Which.Should().BeSameAs(failure);
        calls.Should().Be(1);
    }

    [Fact(DisplayName = "5.TryBegin 只成功一次，关停后再调用返回 false")]
    public async Task Test005()
    {
        // Arrange
        var arbiter = new ExitArbiter(
            _ => Task.CompletedTask,
            TimeSpan.FromSeconds(5),
            NullLogger<ExitArbiter>.Instance);

        // Act & Assert
        arbiter.TryBegin().Should().BeTrue();
        arbiter.TryBegin().Should().BeFalse();

        await arbiter.StopAsync();

        arbiter.TryBegin().Should().BeFalse();
    }
}
