using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住嵌套 Plan 的两层结构与组容量校验：顺序可断言，重复 Id 与越界组容量在 Plan 层拒绝。</summary>
public sealed class BootstrapPlanTests
{
    [Fact(DisplayName = "1.扁平化按组间顺序与组内声明顺序返回唯一序列")]
    public void Test001()
    {
        // Arrange
        var plan = new BootstrapPlan<string>(
        [
            CreateGroup("infrastructure", 5),
            CreateGroup("plugins", 5),
        ]);
        var expected = new[]
        {
            "infrastructure.1",
            "infrastructure.2",
            "infrastructure.3",
            "infrastructure.4",
            "infrastructure.5",
            "plugins.1",
            "plugins.2",
            "plugins.3",
            "plugins.4",
            "plugins.5",
        };

        // Act
        var ids = plan.Flatten().Select(step => step.Id).ToArray();

        // Assert
        ids.Should().Equal(expected);
    }

    [Theory(DisplayName = "2.组容量只接受 5 至 9 项，4 项与 10 项在 Plan 校验时被拒")]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(9, true)]
    [InlineData(10, false)]
    public void Test002(int stepCount, bool accepted)
    {
        // Arrange
        var plan = new BootstrapPlan<string>([CreateGroup("infrastructure", stepCount)]);

        // Act
        var act = () => plan.ValidateGroupCapacity();

        // Assert
        if (accepted)
        {
            act.Should().NotThrow();
        }
        else
        {
            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"*infrastructure*{stepCount}*");
        }
    }

    [Fact(DisplayName = "3.两个组使用同一 Step Id 时 Plan 构造被拒")]
    public void Test003()
    {
        // Arrange
        var first = new BootstrapGroup<string>("infrastructure", [new RecordingStep("shared.log", "infrastructure")]);
        var second = new BootstrapGroup<string>("plugins", [new RecordingStep("shared.log", "plugins")]);

        // Act
        var act = () => new BootstrapPlan<string>([first, second]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*shared.log*")
            .WithParameterName("groups");
    }

    [Fact(DisplayName = "4.Step 声明的 Group 与所属组不一致时 Plan 构造被拒")]
    public void Test004()
    {
        // Arrange
        var group = new BootstrapGroup<string>("plugins", [new RecordingStep("plugins.start", "infrastructure")]);

        // Act
        var act = () => new BootstrapPlan<string>([group]);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*plugins.start*")
            .WithParameterName("groups");
    }

    private static BootstrapGroup<string> CreateGroup(string name, int stepCount)
    {
        var steps = Enumerable.Range(1, stepCount)
            .Select(index => new RecordingStep($"{name}.{index}", name))
            .ToArray();

        return new BootstrapGroup<string>(name, steps);
    }
}
