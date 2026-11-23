using FluentAssertions;
using ImageLyre.App.Cli;
using ImageLyre.App.Startup;
using Xunit;

namespace ImageLyre.App.Tests;

/// <summary>守住 CLI 三条命令的输出、退出码、stderr 分流与参数错误回显。</summary>
public sealed class CliHostTests
{
    private static readonly string s_expectedVersion =
        typeof(StartupOptions).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";

    #region 成功命令

    [Fact(DisplayName = "1.--version 输出产品名与程序集版本并返回 0，stderr 为空")]
    public async Task Test001()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        int exitCode = await CliHost.RunAsync(
            StartupOptions.Parse(["--version"]),
            output,
            error,
            CancellationToken.None);

        // Assert
        exitCode.Should().Be(0);
        output.ToString().Should().Be($"ImageLyre {s_expectedVersion}{Environment.NewLine}");
        error.ToString().Should().BeEmpty();
    }

    [Fact(DisplayName = "2.--help 列出三条命令与 0/1/2 退出码并返回 0")]
    public async Task Test002()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        int exitCode = await CliHost.RunAsync(
            StartupOptions.Parse(["--help"]),
            output,
            error,
            CancellationToken.None);

        // Assert
        exitCode.Should().Be(0);
        string text = output.ToString();
        text.Should().Contain("--version");
        text.Should().Contain("--plugins");
        text.Should().Contain("--help");
        text.Should().Contain("  0  命令执行成功");
        text.Should().Contain("  1  运行期失败（例如插件启动失败）");
        text.Should().Contain("  2  未知或冲突的命令行参数");
        error.ToString().Should().BeEmpty();
    }

    [Fact(DisplayName = "3.--plugins 列出壳、两个能力插件与故障样本并返回 0，诊断日志只走 stderr")]
    public async Task Test003()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        int exitCode = await CliHost.RunAsync(
            StartupOptions.Parse(["--plugins"]),
            output,
            error,
            CancellationToken.None);

        // Assert
        exitCode.Should().Be(0);
        string text = output.ToString();
        text.Should().Contain("Id: lyre.shell.workbench");
        text.Should().Contain("名称: 工作台壳");
        text.Should().Contain("Id: lyre.capability.platform");
        text.Should().Contain("名称: 平台能力");
        text.Should().Contain("Id: lyre.capability.performance");
        text.Should().Contain("名称: 性能监控");
        text.Should().Contain("Id: lyre.test.broken");
        text.Should().Contain("名称: 故障样本");
        text.Should().Contain("状态: Failed");
        text.Should().Contain("状态: Running");
        text.Should().Contain("Feature: 否");
        text.Should().Contain("共 4 个插件，其中 Feature 0 个");
        text.Should().NotContain("HasUiRoute");
        error.ToString().Should().Contain("HasUiRoute=False");
        error.ToString().Should().Contain("能力插件 2 个");
    }

    #endregion

    #region 参数错误与运行失败

    [Fact(DisplayName = "4.未知参数返回 2，stderr 回显参数原文，stdout 为空")]
    public async Task Test004()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        int exitCode = await CliHost.RunAsync(
            StartupOptions.Parse(["--bogus"]),
            output,
            error,
            CancellationToken.None);

        // Assert
        exitCode.Should().Be(2);
        output.ToString().Should().BeEmpty();
        error.ToString().Should().Contain("未知或冲突的参数：--bogus");
        error.ToString().Should().Contain("用法：ImageLyre");
    }

    [Fact(DisplayName = "5.两个命令冲突时返回 2，stderr 回显完整原始参数")]
    public async Task Test005()
    {
        // Arrange
        var output = new StringWriter();
        var error = new StringWriter();

        // Act
        int exitCode = await CliHost.RunAsync(
            StartupOptions.Parse(["--version", "--plugins"]),
            output,
            error,
            CancellationToken.None);

        // Assert
        exitCode.Should().Be(2);
        output.ToString().Should().BeEmpty();
        error.ToString().Should().Contain("未知或冲突的参数：--version --plugins");
    }

    [Fact(DisplayName = "6.运行期写入失败时向 stderr 报告原因并返回 1")]
    public async Task Test006()
    {
        // Arrange
        var output = new StringWriter();
        output.Dispose();
        var error = new StringWriter();

        // Act
        int exitCode = await CliHost.RunAsync(
            StartupOptions.Parse(["--version"]),
            output,
            error,
            CancellationToken.None);

        // Assert
        exitCode.Should().Be(1);
        error.ToString().Should().Contain("运行失败");
        error.ToString().Should().Contain("ObjectDisposedException");
    }

    [Fact(DisplayName = "7.桌面命令误入 CLI 时抛 ArgumentException 并指明参数名")]
    public async Task Test007()
    {
        // Act
        var act = async () => await CliHost.RunAsync(
            StartupOptions.Parse([]),
            new StringWriter(),
            new StringWriter(),
            CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.Message.Should().Contain("桌面命令");
        exception.Which.ParamName.Should().Be("options");
    }

    #endregion
}
