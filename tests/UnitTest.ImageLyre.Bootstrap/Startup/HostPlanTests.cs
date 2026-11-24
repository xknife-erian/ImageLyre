using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住 HostPlan 的固定组结构与组序：空组原样保留，扁平序列只含真实步骤。</summary>
public sealed class HostPlanTests
{
    [Fact(DisplayName = "1.HostPlan 固定 infrastructure/plugins/host-services 三组，空组保留且扁平结果只有插件步骤")]
    public void Test001()
    {
        // Act
        BootstrapPlan<HostBootstrapContext> plan = HostPlan.Create();

        // Assert
        plan.Groups.Select(group => group.Name).Should().Equal("infrastructure", "plugins", "host-services");
        plan.Groups.Select(group => group.Steps.Count).Should().Equal(0, 1, 0);
        plan.Flatten().Select(step => step.Id).Should().Equal("host.plugins.start");
    }
}
