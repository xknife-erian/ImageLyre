using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住启动报告的内存聚合：按组取记录、保持加入顺序，并完整保留每条 StepRecord 的字段。</summary>
public sealed class StartupReportTests
{
    [Fact(DisplayName = "1.报告按组聚合，保持加入顺序并完整保留 StepRecord 字段")]
    public void Test001()
    {
        // Arrange
        var infrastructureLog = CreateRecord(
            "infrastructure.log",
            "infrastructure",
            StepOutcome.Completed,
            TimeSpan.FromMilliseconds(3),
            errorType: null,
            rolledBack: false);
        var pluginsStart = CreateRecord(
            "plugins.start",
            "plugins",
            StepOutcome.Degraded,
            TimeSpan.FromMilliseconds(21),
            errorType: nameof(InvalidOperationException),
            rolledBack: false);
        var infrastructureHost = CreateRecord(
            "infrastructure.host",
            "infrastructure",
            StepOutcome.Completed,
            TimeSpan.FromMilliseconds(7),
            errorType: null,
            rolledBack: true);
        var report = new StartupReport();
        report.Add(infrastructureLog);
        report.Add(pluginsStart);
        report.Add(infrastructureHost);

        // Act
        var infrastructure = report.ForGroup("infrastructure");

        // Assert
        infrastructure.Should().BeEquivalentTo(
            new[] { infrastructureLog, infrastructureHost },
            options => options.WithStrictOrdering());
        report.ForGroup("plugins").Should().BeEquivalentTo(new[] { pluginsStart });
        report.ForGroup("host-services").Should().BeEmpty();
        report.Records.Should().BeEquivalentTo(
            new[] { infrastructureLog, pluginsStart, infrastructureHost },
            options => options.WithStrictOrdering());
    }

    private static StepRecord CreateRecord(
        string id,
        string group,
        StepOutcome outcome,
        TimeSpan elapsed,
        string? errorType,
        bool rolledBack) =>
        new(id, group, StepPhase.Host, outcome, elapsed, errorType, rolledBack);
}
