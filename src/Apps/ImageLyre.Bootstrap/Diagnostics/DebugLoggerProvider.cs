using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Diagnostics;

/// <summary>把日志逐行转发到 <see cref="Debug" /> 的提供程序，供没有控制台的桌面宿主在调试器输出窗格查看。</summary>
/// <remarks>
///     桌面宿主是 <c>WinExe</c>，没有控制台，控制台通道写向 <c>Console.Out</c> 的内容在调试器里无处落脚；
///     这条通道只在挂调试器时真正产出，级别过滤交给 <see cref="ILoggerFactory" /> 的最小级别。
/// </remarks>
internal sealed class DebugLoggerProvider : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new DebugLogger(categoryName);

    /// <inheritdoc />
    public void Dispose()
    {
    }

    private sealed class DebugLogger(string categoryName) : ILogger
    {
        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => true;

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        /// <inheritdoc />
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Debug.WriteLine($"{logLevel}: {categoryName}[{eventId}] {formatter(state, exception)}");
            if (exception is not null)
            {
                Debug.WriteLine(exception.ToString());
            }
        }
    }
}
