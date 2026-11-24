using Cangming.PicoPlugins.Core;
using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住插件失败的纯分类：只有 Failed 状态映射为 Degraded 结果，其余状态映射为 Completed。</summary>
public sealed class StartPluginsStepTests
{
    [Fact(DisplayName = "1.只有 Failed 插件产生降级诊断与 Degraded 结果，Running 与 Stopped 插件不产生")]
    public void Test001()
    {
        // Act
        StepResult result = StartPluginsStep.CreateResult(
        [
            ("lyre.capability.platform", PluginState.Running),
            ("lyre.test.broken", PluginState.Failed),
            ("lyre.capability.performance", PluginState.Stopped),
        ]);

        // Assert
        result.Outcome.Should().Be(StepOutcome.Degraded);
        result.Diagnostics.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new StepDiagnostic(
                "lyre.test.broken",
                "插件启动状态为 Failed，已降级继续。"));
    }

    [Fact(DisplayName = "2.没有 Failed 插件时结果为 Completed 且不带诊断")]
    public void Test002()
    {
        // Act
        StepResult result = StartPluginsStep.CreateResult(
        [
            ("lyre.capability.platform", PluginState.Running),
            ("lyre.capability.performance", PluginState.Stopped),
        ]);

        // Assert
        result.Outcome.Should().Be(StepOutcome.Completed);
        result.Error.Should().BeNull();
        result.Diagnostics.Should().BeNull();
    }
}
