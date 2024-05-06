using System;
using Cangming.Logs;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap.Services;

/// <summary>进程级未处理异常的统一出口：只记录，不吞掉，也不弹窗打断用户。</summary>
/// <remarks>
///     <para>
///         挂两条通道：<see cref="AppDomain.UnhandledException" /> 接住后台线程与终结器线程上的异常，
///         <see cref="TaskScheduler.UnobservedTaskException" /> 接住没人 await 的任务异常——后者若不标记已观察，
///         会在 GC 时把进程带走。
///     </para>
///     <para>
///         界面线程的未处理异常由宿主在 UI 层自行转调 <see cref="Process" />，因为本层不引用任何 UI 框架；
///         这是"装配层与界面解耦"的代价，也是它成立的前提。
///     </para>
/// </remarks>
public sealed partial class UnhandledExceptionProcessor
{
    private static readonly ILogger s_logger = LogHub.For<UnhandledExceptionProcessor>();

    /// <summary>挂接进程级未处理异常通道。</summary>
    public static void Setup()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) => Process(args.ExceptionObject as Exception, "AppDomain");

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Process(args.Exception, "TaskScheduler");
            args.SetObserved();
        };
    }

    /// <summary>记录一条未处理异常；界面线程的异常也调用这里。</summary>
    /// <param name="exception">异常实例；可能为空（例如仅携带消息的终结器异常）。</param>
    /// <param name="source">来源标记，用于在日志里区分通道。</param>
    public static void Process(Exception? exception, string source) =>
        LogUnhandled(s_logger, source, exception?.Message ?? "(无消息)", exception);

    [LoggerMessage(EventId = 1, Level = LogLevel.Critical, Message = "未处理异常（来源 {Source}）：{Message}")]
    private static partial void LogUnhandled(ILogger logger, string source, string message, Exception? exception);
}
