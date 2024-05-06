using System;
using System.Globalization;
using System.IO;
using System.Text;
using Cangming.Logs;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using NLogLevel = NLog.LogLevel;
using MelLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ImageLyre.Bootstrap.Logging;

/// <summary>日志初始化入口：配置 NLog，再把日志工厂交给 <see cref="LogHub" />。</summary>
/// <remarks>
///     <para>
///         本类是仓里唯一允许直接引用 NLog 的地方；其余代码一律经 <see cref="LogHub" /> 取日志，
///         因此换 sink 只改这一处。
///     </para>
///     <para>
///         调用时序是硬约束：<see cref="LogHub" /> 未初始化时取日志会抛异常且不可恢复，
///         所以 <see cref="Initialize" /> 必须是宿主启动的第一步，早于任何业务类型的静态日志字段。
///     </para>
///     <para>重复调用安全：已初始化时直接返回，不重建工厂。</para>
/// </remarks>
public static class LoggingBootstrap
{
    private static readonly Lock Gate = new();

    private static ILoggerFactory? s_factory;

    /// <summary>实际生效的日志目录；未初始化时为 <see langword="null" />。</summary>
    public static string? LogDirectory { get; private set; }

    /// <summary>本次进程的日志会话号，供文件列与诊断信息共用。</summary>
    public static string SessionId { get; } =
        DateTime.Now.ToString("yyyyMMdd-HHmmss-", CultureInfo.InvariantCulture)
        + Random.Shared.Next(1000, 10000).ToString("D4", CultureInfo.InvariantCulture);

    /// <summary>配置 NLog 并把日志工厂交给 <see cref="LogHub" />。</summary>
    /// <param name="logDirectory">日志根目录；为空时落到「我的文档/ImageLyre-Logs/当月」。</param>
    public static void Initialize(string? logDirectory = null)
    {
        lock (Gate)
        {
            if (LogHub.IsInitialized)
            {
                return;
            }

            var root = logDirectory ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ImageLyre-Logs",
                DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture));
            Directory.CreateDirectory(root);

            LogDirectory = root;

            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("imagelyre-file")
            {
                FileName = Path.Combine(root, "imagelyre-${shortdate}.log"),
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}${onexception:${newline}${exception:format=tostring}}",
                KeepFileOpen = false,
                Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                WriteBom = false,
            };
            config.AddTarget(fileTarget);
            config.AddRuleForAllLevels(fileTarget);

            // 控制台通道只在开发期有意义：桌面宿主没有控制台，输出会被丢弃。
            var consoleTarget = new ConsoleTarget("imagelyre-console")
            {
                Layout = "${time}|${level:uppercase=true}|${logger:shortName=true}|${message}",
            };
            config.AddTarget(consoleTarget);
            config.AddRule(NLogLevel.Info, NLogLevel.Fatal, consoleTarget);

            LogManager.Configuration = config;

            s_factory = LoggerFactory.Create(logging =>
            {
                logging.SetMinimumLevel(MelLevel.Trace);
                logging.AddNLog();
                logging.AddProvider(new Diagnostics.DebugLoggerProvider());
            });

            LogHub.Initialize(s_factory);
        }
    }

    /// <summary>取得已初始化的日志工厂，供宿主注册到容器。</summary>
    /// <returns>日志工厂。</returns>
    /// <exception cref="InvalidOperationException">尚未调用 <see cref="Initialize" />。</exception>
    public static ILoggerFactory GetLoggerFactory()
    {
        lock (Gate)
        {
            return s_factory ?? throw new InvalidOperationException("LoggingBootstrap 尚未初始化，请先调用 Initialize()。");
        }
    }
}
