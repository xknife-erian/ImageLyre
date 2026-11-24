using Avalonia.Controls;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Shell.Contracts;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>把插件管理器交出的壳产物挂成 Avalonia 主窗体：槽位注入、取壳断言与 Feature 附着各为一段，由 Plan 保证先后。</summary>
/// <remarks>
/// <para>
/// 顺序是 cangming v1 的硬约束：先 <see cref="InjectSlotManager"/> 把同一个槽位管理器注入壳，再 <see cref="BuildShell"/>
/// 取壳让 <c>ShellWindow</c> 在构造期贡献四个槽位，最后才 <see cref="AttachFeatures"/> 附着 Feature；
/// cangming v1 的 <c>AttachToSlot</c> 在槽位不存在时抛异常，因此不能照 Mercury 的“先附着再取壳”顺序写。
/// <see cref="PresentationPlan"/> 按这三段的顺序各放一个 Step，拆开只为让失败归属到具体步骤，顺序语义与原方法一致。
/// </para>
/// <para>
/// <c>slotManager</c> 必须同时用于注入、壳窗口里的每个 <c>SlotPanel</c> 与 Feature 附着；
/// 三处各 <c>new</c> 一个会得到“附着成功但界面永远为空”的假通过，因为壳与 Feature 各自持有不同的槽位表。
/// </para>
/// </remarks>
internal static class MainWindowFactory
{
    /// <summary>把本次启动唯一的槽位管理器注入插件管理器，壳的 <c>Lazy</c> 在取壳时才求值。</summary>
    /// <param name="pluginManager">已启动插件的管理器，必须包含提供 <see cref="IWorkbenchShell"/> 的壳插件。</param>
    /// <param name="slotManager">本次启动唯一使用的槽位管理器，调用方在返回后继续持有同一实例。</param>
    /// <exception cref="ArgumentNullException"><paramref name="pluginManager"/> 或 <paramref name="slotManager"/> 为 <see langword="null"/>。</exception>
    /// <remarks>本方法必须在 UI 线程执行，属于“注入槽位”一步，本身不构造窗口。</remarks>
    internal static void InjectSlotManager(IPluginManager pluginManager, SlotManager slotManager)
    {
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(slotManager);

        // 注入只交给壳保存 Lazy，真正取 ISlotManager 发生在 Build 里构造 ShellWindow 时。
        pluginManager.InjectSlotManager(slotManager);
    }

    /// <summary>取壳并断言四个一级槽位已经贡献，返回挂载前的 Avalonia 窗体。</summary>
    /// <param name="pluginManager">已启动插件的管理器，必须包含提供 <see cref="IWorkbenchShell"/> 的壳插件。</param>
    /// <param name="slotManager">已经注入插件管理器的同一个槽位管理器。</param>
    /// <returns>壳插件产出的 Avalonia 主窗体，尚未显示。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pluginManager"/> 或 <paramref name="slotManager"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException">壳插件未装载，或其产物不是 <see cref="Window"/>。</exception>
    /// <remarks>
    /// 本方法必须在 UI 线程执行，因为取壳会构造窗口与控件；槽位断言失败时先关闭已经构造的窗体再抛出，
    /// 避免调用方拿不到引用而把半构造的壳留在内存里。关闭走 <see cref="DesktopStopPlan.CloseShell"/>，
    /// 因为 Avalonia 的 <see cref="Window"/> 不实现 <see cref="IDisposable"/>，释放窗体只有关闭一条路。
    /// </remarks>
    internal static Window BuildShell(IPluginManager pluginManager, SlotManager slotManager)
    {
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(slotManager);

        IWorkbenchShell? shell = pluginManager.FindModuleBuilder<IWorkbenchShell>()?.Build().Value;
        if (shell is null)
            throw new InvalidOperationException("壳插件未装载");

        if (shell is not Window window)
            throw new InvalidOperationException("壳插件产物不是 Avalonia Window");

        // v1 的槽位必须先贡献再附着：取壳时 ShellWindow 已贡献四个槽位，这里才轮到 Feature。
        try
        {
            EnsureSlotsContributed(slotManager);
        }
        catch
        {
            // 关窗会触发平台关闭并回收控件；壳插件自己持有的其它资源由桌面关停序列接手。
            DesktopStopPlan.CloseShell(window);
            throw;
        }

        return window;
    }

    /// <summary>向已贡献槽位的壳附着全部 Feature 内容。</summary>
    /// <param name="pluginManager">已启动插件的管理器，Feature 附着由它统一编排。</param>
    /// <param name="slotManager">与壳窗口里面板共享的同一个槽位管理器。</param>
    /// <exception cref="ArgumentNullException"><paramref name="pluginManager"/> 或 <paramref name="slotManager"/> 为 <see langword="null"/>。</exception>
    /// <remarks>本方法必须在 UI 线程执行，因为附着会创建 Feature 内容；调用前必须已完成取壳与槽位断言。</remarks>
    internal static void AttachFeatures(IPluginManager pluginManager, SlotManager slotManager)
    {
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(slotManager);

        // ponytail: M1 还没有 Feature 工程，附着编排实际跑空；T14 接入 Feature 后由它贡献工具条与面板内容。
        pluginManager.RegisterFeatureAttachments(slotManager);
    }

    /// <summary>断言壳已经贡献四个一级槽位，保证附着顺序没有被提前。</summary>
    /// <param name="slotManager">本次启动唯一的槽位管理器。</param>
    /// <exception cref="InvalidOperationException">四个槽位中任意一个尚未贡献。</exception>
    /// <remarks>
    /// 这是“先取壳、再附着”的代码级护栏：取壳会构造 <c>ShellWindow</c> 并贡献槽位，
    /// 若有人把 <c>RegisterFeatureAttachments</c> 提到 <c>Build</c> 之前，这里会在附着前直接失败。
    /// </remarks>
    private static void EnsureSlotsContributed(ISlotManager slotManager)
    {
        foreach (string slotName in new[] { ShellSlots.Toolbar, ShellSlots.Viewport, ShellSlots.Panel, ShellSlots.StatusBar })
        {
            if (!slotManager.TryGetSlot(slotName, out _))
                throw new InvalidOperationException($"壳插件未贡献槽位 {slotName}，不能附着 Feature");
        }
    }
}
