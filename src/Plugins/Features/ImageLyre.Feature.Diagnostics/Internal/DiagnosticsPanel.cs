using System.Globalization;
using Avalonia.Controls;
using Avalonia.Layout;
using ImageLyre.Contracts;
using ImageLyre.Contracts.Diagnostics;
using ImageLyre.Contracts.Platform;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace ImageLyre.Feature.Diagnostics.Internal;

/// <summary>诊断面板的五行动态读数：操作系统、内存、线程、CPU 与运行时长。</summary>
/// <remarks>
/// <para>
/// 控件只在 <c>RegisterAttachments</c> 里创建一次，之后语言切换与刷新命令都只改已有 <c>Text</c>，
/// 不重建控件树；面板页签标题由 <see cref="DockPaneAttachment.Title"/> 在附着时定稿，不在本类刷新。
/// </para>
/// <para>
/// 平台与性能契约通过 <see cref="Func{TResult}"/> 延迟访问，控件构造时不求值；<see cref="Refresh"/> 必须
/// 在 UI 线程调用，因为它直接写控件文本。
/// </para>
/// </remarks>
internal sealed class DiagnosticsPanel
{
    private readonly ILocalizationService _localization;
    private readonly Func<IPlatformCapabilities> _platform;
    private readonly Func<IPerformanceMonitor> _performance;
    private readonly AtomTextBlock _osText = new();
    private readonly AtomTextBlock _memoryText = new();
    private readonly AtomTextBlock _threadsText = new();
    private readonly AtomTextBlock _cpuText = new();
    private readonly AtomTextBlock _uptimeText = new();
    /// <summary>创建一个五行诊断面板。</summary>
    /// <param name="localization">文案服务，不可为 <see langword="null"/>。</param>
    /// <param name="platform">平台能力的延迟访问器，不可为 <see langword="null"/>。</param>
    /// <param name="performance">性能监控的延迟访问器，不可为 <see langword="null"/>。</param>
    /// <remarks>构造函数只搭控件树，不读取平台或性能契约；调用方在附着前显式调用 <see cref="Refresh"/>。</remarks>
    internal DiagnosticsPanel(
        ILocalizationService localization,
        Func<IPlatformCapabilities> platform,
        Func<IPerformanceMonitor> performance)
    {
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(platform);
        ArgumentNullException.ThrowIfNull(performance);

        _localization = localization;
        _platform = platform;
        _performance = performance;

        Root = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
        };

        foreach (AtomTextBlock line in new[] { _osText, _memoryText, _threadsText, _cpuText, _uptimeText })
            Root.Children.Add(line);
    }

    /// <summary>面板根控件，由 Feature 放进 <c>DockPaneAttachment.Content</c>。</summary>
    internal StackPanel Root { get; }

    /// <summary>按当前语言与最新快照刷新五行文本。</summary>
    /// <remarks>只改已存在控件的 <c>Text</c>；性能契约失败时按契约得到 0 值，不向调用方抛异常。</remarks>
    internal void Refresh()
    {
        IPlatformCapabilities platform = _platform();
        PerformanceSnapshot snapshot = _performance().GetPerformanceSnapshot();

        _osText.Text = $"{Label(DiagnosticsStringKeys.LabelOs)}: {platform.CurrentOS}";
        _memoryText.Text = $"{Label(DiagnosticsStringKeys.LabelMemory)}: {snapshot.WorkingSetMb.ToString("F1", CultureInfo.InvariantCulture)} MB";
        _threadsText.Text = $"{Label(DiagnosticsStringKeys.LabelThreads)}: {snapshot.ThreadCount.ToString(CultureInfo.InvariantCulture)}";
        _cpuText.Text = $"{Label(DiagnosticsStringKeys.LabelCpu)}: {snapshot.CpuPercent.ToString("F1", CultureInfo.InvariantCulture)}%";
        _uptimeText.Text = $"{Label(DiagnosticsStringKeys.LabelUptime)}: {FormatUptime(snapshot.Uptime)}";
    }

    /// <summary>取当前语言的标签。</summary>
    private string Label(string key) => _localization.Get(key);

    /// <summary>把运行时长格式化为天、时、分、秒。</summary>
    private static string FormatUptime(TimeSpan uptime) =>
        uptime.ToString(@"d\.hh\:mm\:ss", CultureInfo.InvariantCulture);
}
