using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using ImageLyre.Presentation.Desktop.Startup;
using Xunit;

namespace ImageLyre.Presentation.Desktop.Tests.Startup;

/// <summary>守住 PresentationPlan 的固定组结构与组序：欢迎组留空，扁平序列保持装配先于显示。</summary>
public sealed class PresentationPlanTests
{
    [Fact(DisplayName = "1.PresentationPlan 固定 welcome/composition/show 三组，扁平序列按创建槽位、注入、取壳、附着的顺序先装配后显示")]
    public void Test001()
    {
        // Act
        BootstrapPlan<DesktopBootstrapContext> plan = PresentationPlan.Create();

        // Assert
        plan.Groups.Select(group => group.Name).Should().Equal("welcome", "composition", "show");
        plan.Groups.Select(group => group.Steps.Count).Should().Equal(0, 4, 2);
        plan.Flatten().Select(step => step.Id).Should().Equal(
            "presentation.composition.slots",
            "presentation.composition.inject",
            "presentation.composition.shell",
            "presentation.composition.attach",
            "presentation.show.main-window",
            "presentation.show.close-welcome");
    }
}
