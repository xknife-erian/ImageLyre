using Cangming.PicoPlugins.Core;
using FluentAssertions;
using ImageLyre.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ImageLyre.Bootstrap.Tests;

/// <summary>守住 Bootstrap 在无界面模式下以空插件清单启动并记录扫描路线与能力插件计数的契约。</summary>
public sealed class BootstrapHostTests
{
    [Fact(DisplayName = "1.无界面模式启动成功，插件清单为空且日志输出能力插件 0 个")]
    public async Task Test001()
    {
        // Arrange
        var loggerFactory = new CapturingLoggerFactory();

        // Act
        await using var host = await BootstrapHost.StartAsync(
            loggerFactory,
            HostMode.Headless,
            configure: null,
            CancellationToken.None);

        // Assert
        host.PluginManager.Plugins.Should().BeEmpty();
        host.Services.GetRequiredService<IPluginManager>().Should().BeSameAs(host.PluginManager);
        loggerFactory.Messages.Should().Contain("HasUiRoute=False");
        loggerFactory.Messages.Should().Contain("能力插件 0 个");
    }

    /// <summary>把日志文本收进列表的工厂，避免为一条验收测试引入日志适配包。</summary>
    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        private readonly object _gate = new();
        private readonly List<string> _messages = new();

        public IReadOnlyList<string> Messages
        {
            get
            {
                lock (_gate)
                    return _messages.ToArray();
            }
        }

        public void AddProvider(ILoggerProvider provider) => ArgumentNullException.ThrowIfNull(provider);

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

        public void Dispose()
        {
        }

        public void Capture(string message)
        {
            lock (_gate)
                _messages.Add(message);
        }
    }

    /// <summary>把格式化后的日志文本交给工厂的日志器；本测试只关心文本本身。</summary>
    private sealed class CapturingLogger : ILogger
    {
        private readonly CapturingLoggerFactory _factory;

        public CapturingLogger(CapturingLoggerFactory factory) => _factory = factory;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                _factory.Capture(formatter(state, exception));
            }
        }
    }
}
