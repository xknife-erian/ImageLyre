using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using FluentAssertions;
using ImageLyre.Bootstrap.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>守住 Generic Host 构建器：IHost 可构建、日志工厂与插件管理器同源、重复构建不因静态状态失败。</summary>
public sealed class BootstrapHostBuilderTests
{
    [Fact(DisplayName = "1.无界面模式构建 IHost，日志工厂与插件管理器都来自同一容器")]
    public void Test001()
    {
        // Arrange
        var loggerFactory = new RecordingLoggerFactory();

        // Act
        using IHost host = BootstrapHostBuilder.Build(loggerFactory, HostMode.Headless, configure: null, args: []);

        // Assert
        host.Services.GetRequiredService<ILoggerFactory>().Should().BeSameAs(loggerFactory);
        host.Services.GetRequiredService<IPluginManager>().Should().NotBeNull();
        PicoPluginSettings.HasUiRoute.Should().BeFalse();
        loggerFactory.Messages.Should().Contain("HasUiRoute=False");
    }

    [Fact(DisplayName = "2.构建并启动 IHost 时 HostPlan 只执行 plugins 组，报告记录该步且宿主服务唯一")]
    public async Task Test002()
    {
        // Arrange
        var loggerFactory = new RecordingLoggerFactory();
        using IHost host = BootstrapHostBuilder.Build(loggerFactory, HostMode.Headless, configure: null, args: []);

        // Act
        await host.StartAsync(TestContext.Current.CancellationToken);
        try
        {
            // Assert
            StartupReport report = host.Services.GetRequiredService<StartupReport>();
            report.Records.Select(record => record.Group).Should().Equal("plugins");
            report.Records.Select(record => record.Id).Should().Equal("host.plugins.start");
            report.Records.Select(record => record.Outcome).Should().Equal(StepOutcome.Completed);
            host.Services.GetServices<IHostedService>().Should().ContainSingle()
                .Which.Should().BeOfType<AppBootstrapService>();
            loggerFactory.Messages.Should().Contain("能力插件 0 个");
        }
        finally
        {
            await host.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact(DisplayName = "3.同一进程重复构建并启停两个宿主，插件管理器各自独立且都能启动")]
    public async Task Test003()
    {
        // Arrange
        var loggerFactory = new RecordingLoggerFactory();

        // Act & Assert
        await using (BootstrapHost first = await BootstrapHost.StartAsync(
            loggerFactory,
            HostMode.Headless,
            configure: null,
            CancellationToken.None))
        {
            first.Services.GetRequiredService<IPluginManager>().Should().BeSameAs(first.PluginManager);
        }

        await using (BootstrapHost second = await BootstrapHost.StartAsync(
            loggerFactory,
            HostMode.Headless,
            configure: null,
            CancellationToken.None))
        {
            second.Services.GetRequiredService<IPluginManager>().Should().BeSameAs(second.PluginManager);
            second.PluginManager.Plugins.Should().BeEmpty();
        }

        loggerFactory.Messages.Should().Contain("HasUiRoute=False");
    }

    [Fact(DisplayName = "4.桌面模式构建 IHost 时 HasUiRoute=True 并写入传入日志工厂")]
    public void Test004()
    {
        // Arrange
        var loggerFactory = new RecordingLoggerFactory();

        // Act
        using IHost host = BootstrapHostBuilder.Build(loggerFactory, HostMode.Desktop, configure: null, args: []);

        // Assert
        PicoPluginSettings.HasUiRoute.Should().BeTrue();
        loggerFactory.Messages.Should().Contain("HasUiRoute=True");
    }
}
