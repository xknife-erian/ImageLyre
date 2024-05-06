using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Cangming.Logs;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap;

/// <summary>插件启动策略：读用户级禁用名单，在扫描之前把命中项拒之门外。</summary>
/// <remarks>
///     <para>
///         名单必须早于 <c>PicoPluginSystem.Launch</c> 生效：管理器只在构造时读一次静态禁用集合，
///         之后再登记不会影响本轮启动。因此本类由装配层在登记插件系统时同步调用。
///     </para>
///     <para>
///         文件格式为 <c>{"disabled": ["插件 Id", ...]}</c>；文件缺失、字段缺失或 JSON 非法都按空名单处理，
///         并把原因写进日志——禁用名单写坏不该让软件起不来。
///     </para>
/// </remarks>
internal sealed partial class PluginStartupPolicy
{
    private static readonly ILogger s_logger = LogHub.For<PluginStartupPolicy>();

    private readonly IReadOnlyList<string> _disabledPluginIds;

    private PluginStartupPolicy(IReadOnlyList<string> disabledPluginIds) => _disabledPluginIds = disabledPluginIds;

    /// <summary>用户级名单文件的默认位置。</summary>
    internal static string DefaultFilePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ImageLyre",
        "plugins.json");

    /// <summary>被禁用的插件标识。</summary>
    internal IReadOnlyList<string> DisabledPluginIds => _disabledPluginIds;

    /// <summary>从指定文件读取禁用名单。</summary>
    /// <param name="filePath">名单文件路径。</param>
    /// <returns>启动策略；文件不存在或不可读时为空名单。</returns>
    internal static PluginStartupPolicy Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            return new PluginStartupPolicy([]);
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));

            if (!document.RootElement.TryGetProperty("disabled", out var disabled)
                || disabled.ValueKind != JsonValueKind.Array)
            {
                return new PluginStartupPolicy([]);
            }

            var ids = new List<string>();

            foreach (var item in disabled.EnumerateArray())
            {
                var id = item.GetString();

                if (!string.IsNullOrWhiteSpace(id))
                {
                    ids.Add(id.Trim());
                }
            }

            LogLoaded(s_logger, ids.Count, filePath);

            return new PluginStartupPolicy(ids);
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException)
        {
            LogUnreadable(s_logger, exception, filePath);

            return new PluginStartupPolicy([]);
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "插件禁用名单已加载：{Count} 项，来源 {Path}")]
    private static partial void LogLoaded(ILogger logger, int count, string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "插件禁用名单不可读，按空名单处理：{Path}")]
    private static partial void LogUnreadable(ILogger logger, Exception exception, string path);
}
