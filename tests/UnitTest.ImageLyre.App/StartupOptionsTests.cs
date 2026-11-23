using FluentAssertions;
using ImageLyre.App.Startup;
using Xunit;

namespace ImageLyre.App.Tests;

/// <summary>守住 <see cref="StartupOptions.Parse"/> 的命令白名单、原始参数保留与无效参数判定。</summary>
public sealed class StartupOptionsTests
{
    #region 桌面与三条命令

    [Fact(DisplayName = "1.空参数解析为桌面命令且不进入无界面分支")]
    public void Test001()
    {
        // Act
        var options = StartupOptions.Parse([]);

        // Assert
        options.Command.Should().Be(StartupCommand.Desktop);
        options.Headless.Should().BeFalse();
        options.RawArguments.Should().BeEmpty();
    }

    [Theory(DisplayName = "2.三条命令参数各自解析为对应命令并进入无界面分支")]
    [InlineData("--version", StartupCommand.Version)]
    [InlineData("--plugins", StartupCommand.Plugins)]
    [InlineData("--help", StartupCommand.Help)]
    public void Test002(string argument, StartupCommand expected)
    {
        // Act
        var options = StartupOptions.Parse([argument]);

        // Assert
        options.Command.Should().Be(expected);
        options.Headless.Should().BeTrue();
        options.RawArguments.Should().ContainSingle().Which.Should().Be(argument);
    }

    #endregion

    #region 无效参数

    [Fact(DisplayName = "3.两个命令同时出现时判为无效并保留原始参数顺序")]
    public void Test003()
    {
        // Act
        var options = StartupOptions.Parse(["--version", "--plugins"]);

        // Assert
        options.Command.Should().Be(StartupCommand.Invalid);
        options.Headless.Should().BeTrue();
        options.RawArguments.Should().Equal("--version", "--plugins");
    }

    [Fact(DisplayName = "4.未识别的参数判为无效并保留原文")]
    public void Test004()
    {
        // Act
        var options = StartupOptions.Parse(["--bogus"]);

        // Assert
        options.Command.Should().Be(StartupCommand.Invalid);
        options.Headless.Should().BeTrue();
        options.RawArguments.Should().ContainSingle().Which.Should().Be("--bogus");
    }

    [Fact(DisplayName = "5.T05 的 --headless 开关已收窄，传入后判为无效")]
    public void Test005()
    {
        // Act
        var options = StartupOptions.Parse(["--headless"]);

        // Assert
        options.Command.Should().Be(StartupCommand.Invalid);
        options.RawArguments.Should().ContainSingle().Which.Should().Be("--headless");
    }

    [Fact(DisplayName = "6.同一条命令重复出现也按多个命令处理并判为无效")]
    public void Test006()
    {
        // Act
        var options = StartupOptions.Parse(["--help", "--help"]);

        // Assert
        options.Command.Should().Be(StartupCommand.Invalid);
        options.RawArguments.Should().Equal("--help", "--help");
    }

    #endregion

    #region 参数校验

    [Fact(DisplayName = "7.参数为 null 时抛 ArgumentNullException 并指明参数名")]
    public void Test007()
    {
        // Act
        var act = () => StartupOptions.Parse(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("args");
    }

    #endregion
}
