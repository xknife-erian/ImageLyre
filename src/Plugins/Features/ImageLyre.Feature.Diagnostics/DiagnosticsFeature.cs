using System.Globalization;
using Avalonia.Threading;
using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Avalonias.Core;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using Cangming.PicoPlugins.Core.Support;
using Cangming.Toolkit.Ordering;
using ImageLyre.Contracts;
using ImageLyre.Contracts.Diagnostics;
using ImageLyre.Contracts.Platform;
using ImageLyre.Feature.Diagnostics.Internal;
using ImageLyre.Shell.Attachments;
using ImageLyre.Shell.Contracts;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Feature.Diagnostics;

/// <summary>M1 的诊断 Feature：消费平台与性能契约，填充工具条、辅助面板与状态栏。</summary>
/// <remarks>
/// <para>
/// 依赖 <see cref="IPlatformCapabilities"/> 与 <see cref="IPerformanceMonitor"/>，并在平台能力之后启动；
/// 上下文只读写两个契约与宿主本地化服务，Feature 不引用任何 Module 工程。
/// </para>
/// <para>
/// 附着：工具条一个刷新按钮、辅助面板一页五行读数、状态栏 OS 与内存两段；每个附着物都写显式
/// <see cref="OrderRule"/>，因为 cangming v1 对无偏好项不做稳定排序。
/// </para>
/// <para>
/// 文案：实现 <see cref="ILocalizationSource"/> 并在 <c>OnStartAsync</c> 注册；语言切换只刷新面板内已有控件的
/// <c>Text</c>，工具条标题、面板页签与状态栏文本在附着时定稿，这是 v1 没有替换/撤销能力的限制。
/// </para>
/// </remarks>
[Plugin(id: "lyre.feature.diagnostics", name: "诊断")]
public sealed class DiagnosticsFeature : BasePicoFeatures, ILocalizationSource, ISupportUsingIoC
{
    private static readonly ILogger s_logger = LogHub.For(typeof(DiagnosticsFeature));

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> s_texts =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["zh-CN"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DiagnosticsStringKeys.PanelTitle] = "诊断",
                [DiagnosticsStringKeys.ToolbarRefresh] = "刷新",
                [DiagnosticsStringKeys.LabelOs] = "操作系统",
                [DiagnosticsStringKeys.LabelMemory] = "内存",
                [DiagnosticsStringKeys.LabelThreads] = "线程",
                [DiagnosticsStringKeys.LabelCpu] = "CPU",
                [DiagnosticsStringKeys.LabelUptime] = "运行时长",
            },
            ["en-US"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [DiagnosticsStringKeys.PanelTitle] = "Diagnostics",
                [DiagnosticsStringKeys.ToolbarRefresh] = "Refresh",
                [DiagnosticsStringKeys.LabelOs] = "OS",
                [DiagnosticsStringKeys.LabelMemory] = "Memory",
                [DiagnosticsStringKeys.LabelThreads] = "Threads",
                [DiagnosticsStringKeys.LabelCpu] = "CPU",
                [DiagnosticsStringKeys.LabelUptime] = "Uptime",
            },
        };

    private readonly DiagnosticsFeatureContext _context;
    private DiagnosticsPanel? _panel;
    private ILocalizationService? _localization;
    private bool _subscribed;
    private bool _disposed;

    /// <summary>创建一个只完成上下文装配的诊断 Feature。</summary>
    /// <remarks>构造函数不读取依赖，也不创建控件；控件在 <see cref="RegisterAttachments"/> 里一次性创建。</remarks>
    public DiagnosticsFeature() : base()
    {
        _context = new DiagnosticsFeatureContext();
        UseContext(_context);
    }

    /// <summary>声明两个能力契约与平台能力的启动先后。</summary>
    /// <param name="modules">依赖声明构建器。</param>
    /// <remarks>本方法会被读依赖与读顺序各调用一次，只写声明，不求值任何 Lazy。</remarks>
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules)
    {
        modules.Require<IPlatformCapabilities>();
        modules.Require<IPerformanceMonitor>();
        modules.After<IPlatformCapabilities>();
    }

    /// <summary>保存宿主 IoC 的延迟引用，供启动时解析本地化服务。</summary>
    /// <param name="iocLazy">宿主容器访问器的延迟引用，不可为 <see langword="null"/>。</param>
    /// <returns>当前 Feature，供注入链继续。</returns>
    public Task<IPicoPlugin> InjectAsync(Lazy<IIoCManager> iocLazy)
    {
        _context.SetIoC(iocLazy);
        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <summary>注册诊断文案源并订阅语言切换。</summary>
    /// <returns>恒为 <see langword="true"/>。</returns>
    protected override Task<bool> OnStartAsync()
    {
        _localization = _context.Localization;
        _localization.AddSource(this);

        if (!_subscribed)
        {
            _localization.LanguageChanged += OnLanguageChanged;
            _subscribed = true;
        }

        s_logger.LogInformation("诊断 Feature 已启动，文案源已注册。");
        return Task.FromResult(true);
    }

    /// <summary>退订语言切换事件。</summary>
    /// <returns>基类默认成功值。</returns>
    protected override Task<bool> OnStopAsync()
    {
        Unsubscribe();
        return base.OnStopAsync();
    }

    /// <summary>释放时再次退订，防止停止路径未走到时留下事件引用。</summary>
    protected override void OnDispose()
    {
        _disposed = true;
        Unsubscribe();
        base.OnDispose();
    }

    /// <summary>把刷新按钮、诊断面板与两段状态栏读数附着到壳的槽位。</summary>
    /// <param name="registry">槽位注册面，不可为 <see langword="null"/>。</param>
    /// <exception cref="InvalidOperationException">Feature 尚未启动，本地化服务还没取得。</exception>
    /// <remarks>
    /// 面板控件只在这里创建一次；工具条标题、面板页签与状态栏文本在附着时按当前语言定稿，
    /// 语言切换只刷新面板内五行的 <c>Text</c>，不重建任何控件。
    /// </remarks>
    public override void RegisterAttachments(IAttachmentRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        ILocalizationService localization = _localization
            ?? throw new InvalidOperationException("诊断 Feature 尚未启动，不能附着控件。");

        _panel = new DiagnosticsPanel(
            localization,
            () => _context.Platform,
            () => _context.Performance);
        _panel.Refresh();

        var toolbarButton = new ToolbarButtonAttachment(
            localization.Get(DiagnosticsStringKeys.ToolbarRefresh),
            new RefreshCommand(_panel.Refresh))
        {
            Order = new NumberOrderRule(OrderType.AscendingByNumber, 10),
        };

        var dockPane = new DockPaneAttachment(
            localization.Get(DiagnosticsStringKeys.PanelTitle),
            _panel.Root)
        {
            Order = new NumberOrderRule(OrderType.AscendingByNumber, 10),
        };

        PerformanceSnapshot snapshot = _context.Performance.GetPerformanceSnapshot();
        var statusOs = new StatusBarAttachment(
            $"{localization.Get(DiagnosticsStringKeys.LabelOs)}: {_context.Platform.CurrentOS}")
        {
            Order = new NumberOrderRule(OrderType.AscendingByNumber, 10),
        };

        var statusMemory = new StatusBarAttachment(
            $"{localization.Get(DiagnosticsStringKeys.LabelMemory)}: {snapshot.WorkingSetMb.ToString("F1", CultureInfo.InvariantCulture)} MB")
        {
            Order = new NumberOrderRule(OrderType.AscendingByNumber, 20),
        };

        // ponytail: 工具条标题、状态栏文本与面板页签都在附着时定稿；状态栏两段也暂由 Feature 静态附带。
        // v1 没有替换/撤销能力，运行期切语言只刷新面板五行的 Text，等 G3 的槽位撤销再补完整刷新。
        registry.AttachToSlot(ShellSlots.Toolbar, toolbarButton);
        registry.AttachToSlot(ShellSlots.Panel, dockPane);
        registry.AttachToSlot(ShellSlots.StatusBar, statusOs);
        registry.AttachToSlot(ShellSlots.StatusBar, statusMemory);
    }

    /// <summary>按指定语言取诊断文案。</summary>
    /// <param name="key">资源键。</param>
    /// <param name="culture">目标语言。</param>
    /// <param name="value">命中文案；未命中时为空串。</param>
    /// <returns>命中时为 <see langword="true"/>。</returns>
    /// <remarks>源内不做跨语言回退，未知键返回 <see langword="false"/> 并只给出空串，回退由聚合服务负责。</remarks>
    public bool TryGet(string key, CultureInfo culture, out string value)
    {
        if (key is not null &&
            culture is not null &&
            s_texts.TryGetValue(culture.Name, out IReadOnlyDictionary<string, string>? texts) &&
            texts.TryGetValue(key, out string? text))
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    /// <summary>语言切换后把面板刷新投递到 UI 线程。</summary>
    private void OnLanguageChanged(object? sender, EventArgs e) =>
        Dispatcher.UIThread.Post(RefreshTexts);

    /// <summary>只刷新面板内已有控件的文本，不重建窗口或控件树。</summary>
    private void RefreshTexts()
    {
        if (_disposed)
            return;

        _panel?.Refresh();
    }

    /// <summary>退订本地化事件；重复调用幂等。</summary>
    private void Unsubscribe()
    {
        if (!_subscribed || _localization is null)
            return;

        _localization.LanguageChanged -= OnLanguageChanged;
        _subscribed = false;
    }
}
