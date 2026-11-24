using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>Presentation 阶段共享的上下文：桌面生命周期、欢迎窗、插件管理器，以及步骤依次填入的槽位管理器与壳窗体。</summary>
/// <remarks>
/// <para>
/// 实例由 <see cref="DesktopStartup"/> 在 Host 启动完成后构造，之后原样传给 <see cref="PresentationPlan"/> 的每个步骤；
/// <see cref="SlotManager"/> 与 <see cref="ShellWindow"/> 先为空，由装配组的前两步填入，后续步骤只读取，步骤之间不交换其它引用。
/// </para>
/// <para>
/// 线程：上下文只在 Avalonia UI 线程上使用，步骤串行写入，因此不加锁；它不是线程安全的，也不得跨线程传递。
/// </para>
/// </remarks>
internal sealed class DesktopBootstrapContext
{
    /// <summary>创建一次 PresentationPlan 执行的上下文。</summary>
    /// <param name="desktop">经典桌面生命周期，显示组通过它替换主窗体。</param>
    /// <param name="pluginManager">已启动插件的管理器，取壳与附着都经过它。</param>
    /// <param name="welcome">启动期欢迎窗口，成功时由显示组关闭，失败时由宿主填失败原因。</param>
    /// <exception cref="ArgumentNullException">任一参数为 <see langword="null"/>。</exception>
    internal DesktopBootstrapContext(
        IClassicDesktopStyleApplicationLifetime desktop,
        IPluginManager pluginManager,
        Window welcome)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(welcome);

        Desktop = desktop;
        PluginManager = pluginManager;
        Welcome = welcome;
    }

    /// <summary>本次启动的经典桌面生命周期，只由显示组改写 <c>MainWindow</c>。</summary>
    internal IClassicDesktopStyleApplicationLifetime Desktop { get; }

    /// <summary>本次启动的插件管理器，与宿主容器内的实例相同。</summary>
    internal IPluginManager PluginManager { get; }

    /// <summary>启动期欢迎窗口，标题、版本与状态文本在创建时已经写入。</summary>
    internal Window Welcome { get; }

    /// <summary>本次启动唯一的槽位管理器，由装配组第一步创建，注入、壳面板与 Feature 附着共用它。</summary>
    internal SlotManager? SlotManager { get; set; }

    /// <summary>壳插件产出的主窗体，由取壳步骤填入；失败路径据此释放已经构造的窗口。</summary>
    internal Window? ShellWindow { get; set; }
}
