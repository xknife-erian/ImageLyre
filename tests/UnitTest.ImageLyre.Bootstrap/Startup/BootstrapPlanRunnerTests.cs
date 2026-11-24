using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住共享执行器：逐 Step 写报告、Degraded 继续、Failed 逆序回滚已完成步骤，并支撑 50 步增长压测。</summary>
public sealed class BootstrapPlanRunnerTests
{
    private static readonly string[] s_groupNames =
        ["infrastructure", "plugins", "host-services", "welcome", "composition", "show", "stop"];

    private static readonly int[] s_growthSizes = [7, 7, 7, 7, 7, 7, 8];

    [Fact(DisplayName = "1.Failed 步骤逆序回滚已完成且可回滚的 Step，后续 Step 不执行且报告含 StepId 与异常类型")]
    public async Task Test001()
    {
        // Arrange
        var executions = new List<string>();
        var rollbacks = new List<string>();
        var plan = new BootstrapPlan<string>(
        [
            new BootstrapGroup<string>(
                "composition",
                [
                    new TrackingStep("composition.1", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                    new TrackingStep("composition.2", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                    new TrackingStep(
                        "composition.3",
                        "composition",
                        StepOutcome.Failed,
                        executions,
                        rollbacks,
                        canRollback: true,
                        error: new InvalidOperationException("壳未装载")),
                    new TrackingStep("composition.4", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                ]),
        ]);
        var report = new StartupReport();
        var runner = new BootstrapPlanRunner<string>(plan, report, NullLogger.Instance);

        // Act
        var act = () => runner.RunAsync("bootstrap", CancellationToken.None);

        // Assert
        var failure = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        failure.Message.Should().Contain("composition.3");
        executions.Should().Equal("composition.1", "composition.2", "composition.3");
        rollbacks.Should().Equal("composition.2", "composition.1");
        report.Records.Select(record => record.Id).Should().Equal("composition.1", "composition.2", "composition.3");
        report.Records.Select(record => record.Outcome).Should().Equal(
            StepOutcome.Completed,
            StepOutcome.Completed,
            StepOutcome.Failed);
        report.Records.Select(record => record.RolledBack).Should().Equal(true, true, false);
        report.Records[2].ErrorType.Should().Be(nameof(InvalidOperationException));
    }

    [Fact(DisplayName = "2.Degraded 步骤记录诊断后继续执行，且不触发任何回滚")]
    public async Task Test002()
    {
        // Arrange
        var executions = new List<string>();
        var rollbacks = new List<string>();
        var degradedDiagnostic = new StepDiagnostic(
            "ImageLyre.Module.Broken",
            "插件启动状态为 Failed，已降级继续。");
        var plan = new BootstrapPlan<string>(
        [
            new BootstrapGroup<string>(
                "plugins",
                [
                    new TrackingStep("plugins.1", "plugins", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                    new TrackingStep(
                        "plugins.2",
                        "plugins",
                        StepOutcome.Degraded,
                        executions,
                        rollbacks,
                        canRollback: true,
                        error: new InvalidOperationException("插件 ImageLyre.Module.Broken 启动失败。"),
                        diagnostics: [degradedDiagnostic]),
                    new TrackingStep("plugins.3", "plugins", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                ]),
        ]);
        var report = new StartupReport();
        var runner = new BootstrapPlanRunner<string>(plan, report, NullLogger.Instance);

        // Act
        await runner.RunAsync("bootstrap", CancellationToken.None);

        // Assert
        executions.Should().Equal("plugins.1", "plugins.2", "plugins.3");
        rollbacks.Should().BeEmpty();
        report.Records.Select(record => record.Outcome).Should().Equal(
            StepOutcome.Completed,
            StepOutcome.Degraded,
            StepOutcome.Completed);
        report.Records[1].ErrorType.Should().Be(nameof(InvalidOperationException));
        report.Records[1].Diagnostics.Should().ContainSingle().Which.Should().BeEquivalentTo(degradedDiagnostic);
        report.Records.Select(record => record.RolledBack).Should().Equal(false, false, false);
    }

    [Fact(DisplayName = "3.50 个 Step 的嵌套 Plan 保持 7 个 5 至 9 项叶组与唯一扁平顺序")]
    public void Test003()
    {
        // Arrange
        var plan = CreatePlan(s_growthSizes, new List<string>(), new List<string>());

        // Act
        var groupNames = plan.Groups.Select(group => group.Name).ToArray();
        var groupSizes = plan.Groups.Select(group => group.Steps.Count).ToArray();
        var ids = plan.Flatten().Select(step => step.Id).ToArray();

        // Assert
        groupNames.Should().Equal(s_groupNames);
        groupSizes.Should().Equal(s_growthSizes);
        groupSizes.Sum().Should().Be(50);
        ids.Should().Equal(CreateExpectedIds(s_growthSizes));
        plan.Invoking(target => target.ValidateGroupCapacity()).Should().NotThrow();
    }

    [Fact(DisplayName = "4.50 步 Plan 在最后一步失败时逆序回滚前 49 个可回滚步骤且报告完整")]
    public async Task Test004()
    {
        // Arrange
        var executions = new List<string>();
        var rollbacks = new List<string>();
        string[] expectedIds = CreateExpectedIds(s_growthSizes);
        var plan = CreatePlan(s_growthSizes, executions, rollbacks, failingId: expectedIds[^1]);
        var report = new StartupReport();
        var runner = new BootstrapPlanRunner<string>(plan, report, NullLogger.Instance);

        // Act
        var act = () => runner.RunAsync("bootstrap", CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Contain(expectedIds[^1]);
        executions.Should().Equal(expectedIds);
        rollbacks.Should().Equal(expectedIds[..^1].Reverse());
        report.Records.Should().HaveCount(50);
        report.Records.Count(record => record.RolledBack).Should().Be(49);
        report.Records[^1].Outcome.Should().Be(StepOutcome.Failed);
        report.ForGroup(s_groupNames[^1]).Select(record => record.Id).Should().Equal(
            expectedIds.TakeLast(s_growthSizes[^1]));
    }

    [Fact(DisplayName = "5.MarkRolledBack 只改回滚标记，保留记录其它字段且重复标记幂等")]
    public void Test005()
    {
        // Arrange
        var record = new StepRecord(
            "composition.1",
            "composition",
            StepPhase.Host,
            StepOutcome.Completed,
            TimeSpan.FromMilliseconds(4),
            ErrorType: null,
            RolledBack: false);
        var report = new StartupReport();
        report.Add(record);

        // Act
        report.MarkRolledBack("composition.1");
        report.MarkRolledBack("composition.1");
        report.MarkRolledBack("composition.missing");

        // Assert
        report.Records.Should().ContainSingle()
            .Which.Should().Be(record with { RolledBack = true });
    }

    [Fact(DisplayName = "6.Step 直接抛非取消异常时记 Failed、逆序回滚已完成步骤并抛带 StepId 的异常")]
    public async Task Test006()
    {
        // Arrange
        var executions = new List<string>();
        var rollbacks = new List<string>();
        var thrown = new InvalidOperationException("壳装载失败");
        var plan = new BootstrapPlan<string>(
        [
            new BootstrapGroup<string>(
                "composition",
                [
                    new TrackingStep("composition.1", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                    new TrackingStep(
                        "composition.2",
                        "composition",
                        StepOutcome.Completed,
                        executions,
                        rollbacks,
                        canRollback: true,
                        executeException: thrown),
                    new TrackingStep("composition.3", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                ]),
        ]);
        var report = new StartupReport();
        var runner = new BootstrapPlanRunner<string>(plan, report, NullLogger.Instance);

        // Act
        var act = () => runner.RunAsync("bootstrap", CancellationToken.None);

        // Assert
        var failure = (await act.Should().ThrowAsync<InvalidOperationException>()).Which;
        failure.Message.Should().Contain("composition.2");
        failure.InnerException.Should().BeSameAs(thrown);
        executions.Should().Equal("composition.1", "composition.2");
        rollbacks.Should().Equal("composition.1");
        report.Records.Select(record => record.Id).Should().Equal("composition.1", "composition.2");
        report.Records.Select(record => record.Outcome).Should().Equal(
            StepOutcome.Completed,
            StepOutcome.Failed);
        report.Records[1].ErrorType.Should().Be(nameof(InvalidOperationException));
    }

    [Fact(DisplayName = "7.取消后不再执行后续 Step，报告把正在执行的 Step 记为 Aborted")]
    public async Task Test007()
    {
        // Arrange
        var executions = new List<string>();
        var rollbacks = new List<string>();
        using var cancellation = new CancellationTokenSource();
        var plan = new BootstrapPlan<string>(
        [
            new BootstrapGroup<string>(
                "composition",
                [
                    new TrackingStep("composition.1", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                    new TrackingStep(
                        "composition.2",
                        "composition",
                        StepOutcome.Completed,
                        executions,
                        rollbacks,
                        canRollback: true,
                        executeException: new OperationCanceledException(cancellation.Token),
                        onExecute: cancellation.Cancel),
                    new TrackingStep("composition.3", "composition", StepOutcome.Completed, executions, rollbacks, canRollback: true),
                ]),
        ]);
        var report = new StartupReport();
        var runner = new BootstrapPlanRunner<string>(plan, report, NullLogger.Instance);

        // Act
        var act = () => runner.RunAsync("bootstrap", cancellation.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        executions.Should().Equal("composition.1", "composition.2");
        rollbacks.Should().BeEmpty();
        report.Records.Select(record => record.Id).Should().Equal("composition.1", "composition.2");
        report.Records.Select(record => record.Outcome).Should().Equal(
            StepOutcome.Completed,
            StepOutcome.Aborted);
        report.Records[1].ErrorType.Should().BeNull();
    }

    private static BootstrapPlan<string> CreatePlan(
        int[] sizes,
        List<string> executions,
        List<string> rollbacks,
        string? failingId = null)
    {
        var groups = new List<BootstrapGroup<string>>();
        for (int groupIndex = 0; groupIndex < sizes.Length; groupIndex++)
        {
            string group = s_groupNames[groupIndex];
            var steps = new List<IBootstrapStep<string>>();
            for (int stepIndex = 1; stepIndex <= sizes[groupIndex]; stepIndex++)
            {
                string id = $"{group}.{stepIndex}";
                bool failing = string.Equals(id, failingId, StringComparison.Ordinal);
                steps.Add(new TrackingStep(
                    id,
                    group,
                    failing ? StepOutcome.Failed : StepOutcome.Completed,
                    executions,
                    rollbacks,
                    canRollback: true,
                    error: failing ? new InvalidOperationException($"步骤 {id} 注入失败。") : null));
            }

            groups.Add(new BootstrapGroup<string>(group, steps));
        }

        return new BootstrapPlan<string>(groups);
    }

    private static string[] CreateExpectedIds(int[] sizes)
    {
        var ids = new List<string>();
        for (int groupIndex = 0; groupIndex < sizes.Length; groupIndex++)
        {
            for (int stepIndex = 1; stepIndex <= sizes[groupIndex]; stepIndex++)
                ids.Add($"{s_groupNames[groupIndex]}.{stepIndex}");
        }

        return [.. ids];
    }
}
