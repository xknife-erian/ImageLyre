using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>用测试驱动器模拟后续 Pipeline 的结果策略：Degraded 只记录降级，不中断后续 Step。</summary>
public sealed class StartupExecutionTests
{
    [Fact(DisplayName = "1.Degraded 只记录降级而不中断后续 Step")]
    public async Task Test001()
    {
        // Arrange
        var plan = new BootstrapPlan<string>(
        [
            new BootstrapGroup<string>("infrastructure", [new RecordingStep("infrastructure.log", "infrastructure")]),
            new BootstrapGroup<string>(
                "plugins",
                [
                    new RecordingStep(
                        "plugins.start",
                        "plugins",
                        StepOutcome.Degraded,
                        error: new InvalidOperationException("插件 ImageLyre.Module.Broken 启动失败。")),
                ]),
            new BootstrapGroup<string>("host-services", [new RecordingStep("host-services.monitor", "host-services")]),
        ]);

        // Act
        var report = await RunAsync(plan);

        // Assert
        var records = report.Records;
        records.Select(record => record.Id).Should().Equal(
            "infrastructure.log",
            "plugins.start",
            "host-services.monitor");
        records.Select(record => record.Outcome).Should().Equal(
            StepOutcome.Completed,
            StepOutcome.Degraded,
            StepOutcome.Completed);
        report.ForGroup("plugins").Single().ErrorType.Should().Be(nameof(InvalidOperationException));
    }

    // 测试驱动器模拟 P1 之前还没有的 Pipeline：按扁平序列执行，Degraded 继续，Failed/Aborted 停止。
    private static async Task<StartupReport> RunAsync(BootstrapPlan<string> plan)
    {
        var report = new StartupReport();
        foreach (var step in plan.Flatten())
        {
            var result = await step.ExecuteAsync("bootstrap", CancellationToken.None);
            report.Add(new StepRecord(
                step.Id,
                step.Group,
                step.Phase,
                result.Outcome,
                TimeSpan.Zero,
                result.Error?.GetType().Name,
                RolledBack: false));

            if (result.Outcome is StepOutcome.Failed or StepOutcome.Aborted)
            {
                break;
            }
        }

        return report;
    }
}
