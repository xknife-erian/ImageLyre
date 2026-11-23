using Microsoft.Extensions.Logging;

namespace ImageLyre.App.Cli;

/// <summary>CLI 的最小 stderr 日志工厂，只把 Information 及以上写到调用方给的错误流。</summary>
/// <remarks>
/// <para>
/// stdout 留给命令结果，因此日志一律写 stderr；本工厂不持有文件、不缓冲、不异步排队，每条日志都在调用线程上同步写出，
/// 多个插件线程同时记录时用一把锁串行化整条记录，避免半行交错。生产入口把 <see cref="Console.Error"/> 传进来，
/// 测试则传 <see cref="StringWriter"/>，两者走同一条路径。
/// </para>
/// <para>
/// 这是 M1 的最小实现，等宿主日志管线接入 NLog 或 Serilog 后整体替换；替换时 <see cref="CliHost"/> 仍然只依赖
/// <see cref="ILoggerFactory"/>，调用形状不变。
/// </para>
/// </remarks>
internal sealed class StderrLoggerFactory : ILoggerFactory
{
    private readonly object _gate = new();
    private readonly TextWriter _error;

    // ponytail: 这是 M1 的最小日志实现，只做同步写 stderr；宿主日志管线接入 NLog/Serilog 后整体替换，不在本类上继续加功能。

    /// <summary>创建一个写到指定错误流的日志工厂。</summary>
    /// <param name="error">日志目标流，通常是 <see cref="Console.Error"/>；工厂不拥有它，也不负责释放。</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> 为 <see langword="null"/>。</exception>
    internal StderrLoggerFactory(TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _error = error;
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        ArgumentNullException.ThrowIfNull(categoryName);
        return new StderrLogger(this, categoryName);
    }

    /// <summary>忽略外部日志提供器：M1 只有 stderr 一条通道。</summary>
    /// <param name="provider">被忽略的提供器，不得为 <see langword="null"/>。</param>
    /// <remarks>
    /// 接入真实日志管线前，调用方没有理由注册提供器；这里只做参数校验，不保存也不释放提供器，
    /// 因为本工厂不拥有它的生命周期。
    /// </remarks>
    public void AddProvider(ILoggerProvider provider) => ArgumentNullException.ThrowIfNull(provider);

    /// <summary>释放工厂；本实现不持有可释放资源，因此为空操作。</summary>
    public void Dispose()
    {
    }

    /// <summary>把一条已格式化的日志连同异常写到错误流。</summary>
    /// <param name="level">日志级别。</param>
    /// <param name="category">日志类别，取自创建日志器时的类型或名称。</param>
    /// <param name="message">已由调用方格式化好的消息。</param>
    /// <param name="exception">随日志携带的异常；为 <see langword="null"/> 时不追加异常文本。</param>
    /// <remarks>写整行时持锁，保证并发记录不会把级别前缀、类别与消息拆散。</remarks>
    private void Write(LogLevel level, string category, string message, Exception? exception)
    {
        lock (_gate)
        {
            _error.WriteLine($"[{level}] {category}: {message}");
            if (exception is not null)
                _error.WriteLine(exception);
        }
    }

    /// <summary>把日志委托回工厂写出的最小日志器。</summary>
    private sealed class StderrLogger : ILogger
    {
        private readonly StderrLoggerFactory _factory;
        private readonly string _category;

        internal StderrLogger(StderrLoggerFactory factory, string category)
        {
            _factory = factory;
            _category = category;
        }

        /// <inheritdoc />
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        /// <inheritdoc />
        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None && logLevel >= LogLevel.Information;

        /// <inheritdoc />
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            ArgumentNullException.ThrowIfNull(formatter);
            _factory.Write(logLevel, _category, formatter(state, exception), exception);
        }
    }
}
