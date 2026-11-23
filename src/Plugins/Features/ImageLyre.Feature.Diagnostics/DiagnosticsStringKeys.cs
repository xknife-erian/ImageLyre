namespace ImageLyre.Feature.Diagnostics;

/// <summary>诊断 Feature 的文案键；统一以插件 Id 前缀开头，资源源与控件共用这一份定义。</summary>
/// <remarks>新增键时同步扩 <see cref="DiagnosticsFeature"/> 的中英字典与本工程的 README；键名是持久契约，不随布局改名。</remarks>
internal static class DiagnosticsStringKeys
{
    internal static string PanelTitle => "lyre.feature.diagnostics.panel.title";
    internal static string ToolbarRefresh => "lyre.feature.diagnostics.toolbar.refresh";
    internal static string LabelOs => "lyre.feature.diagnostics.label.os";
    internal static string LabelMemory => "lyre.feature.diagnostics.label.memory";
    internal static string LabelThreads => "lyre.feature.diagnostics.label.threads";
    internal static string LabelCpu => "lyre.feature.diagnostics.label.cpu";
    internal static string LabelUptime => "lyre.feature.diagnostics.label.uptime";

    internal static IReadOnlyList<string> All =>
    [
        PanelTitle,
        ToolbarRefresh,
        LabelOs,
        LabelMemory,
        LabelThreads,
        LabelCpu,
        LabelUptime,
    ];
}
