using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Tests.Startup;

/// <summary>把日志文本收进列表的工厂，供构建器与 HostPlan 断言使用。</summary>
internal sealed class RecordingLoggerFactory : ILoggerFactory
{
    private readonly object _gate = new();
    private readonly List<string> _messages = [];

    public IReadOnlyList<string> Messages
    {
        get
        {
            lock (_gate)
            {
                return _messages.ToArray();
            }
        }
    }

    public void AddProvider(ILoggerProvider provider) => ArgumentNullException.ThrowIfNull(provider);

    public ILogger CreateLogger(string categoryName) => new RecordingLogger(this);

    public void Dispose()
    {
    }

    private void Capture(string message)
    {
        lock (_gate)
        {
            _messages.Add(message);
        }
    }

    private sealed class RecordingLogger : ILogger
    {
        private readonly RecordingLoggerFactory _factory;

        public RecordingLogger(RecordingLoggerFactory factory) => _factory = factory;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

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
