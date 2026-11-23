using Avalonia.Controls;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Shell.Contracts;

namespace ImageLyre.App.Startup;

/// <summary>把插件管理器交出的壳产物挂成 Avalonia 主窗体，并完成槽位注入与 Feature 附着。</summary>
/// <remarks>
/// <para>
/// 顺序是 cangming v1 的硬约束：先 <c>InjectSlotManager</c> 把同一个槽位管理器注入壳，再取壳让
/// <c>ShellWindow</c> 在构造期贡献三个槽位，最后才 <c>RegisterFeatureAttachments</c> 附着 Feature。
/// cangming v1 的 <c>AttachToSlot</c> 在槽位不存在时抛异常，因此不能照 Mercury 的“先附着再取壳”顺序写。
/// </para>
/// <para>
/// <paramref name="slotManager"/> 必须同时用于注入、壳窗口里的每个 <c>SlotPanel</c> 与 Feature 附着；
/// 三处各 <c>new</c> 一个会得到“附着成功但界面永远为空”的假通过，因为壳与 Feature 各自持有不同的槽位表。
/// </para>
/// </remarks>
internal static class MainWindowFactory
{
    /// <summary>按固定顺序组装主窗体。</summary>
    /// <param name="pluginManager">已启动插件的管理器，必须包含提供 <see cref="IWorkbenchShell"/> 的壳插件。</param>
    /// <param name="slotManager">本次启动唯一使用的槽位管理器，调用方在返回后继续持有同一实例。</param>
    /// <returns>壳插件产出的 Avalonia 主窗体。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="pluginManager"/> 或 <paramref name="slotManager"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="InvalidOperationException">壳插件未装载，或其产物不是 <see cref="Window"/>。</exception>
    /// <remarks>
    /// 本方法必须在 UI 线程执行，因为取壳会构造窗口与控件、附着会创建 Feature 内容；
    /// 方法本身不创建窗口生命周期，也不注册 Feature 之外的服务。
    /// </remarks>
    internal static Window Compose(IPluginManager pluginManager, SlotManager slotManager)
    {
        ArgumentNullException.ThrowIfNull(pluginManager);
        ArgumentNullException.ThrowIfNull(slotManager);

        // 注入只交给壳保存 Lazy，真正取 ISlotManager 发生在 Build 里构造 ShellWindow 时。
        pluginManager.InjectSlotManager(slotManager);

        IWorkbenchShell? shell = pluginManager.FindModuleBuilder<IWorkbenchShell>()?.Build().Value;
        if (shell is null)
            throw new InvalidOperationException("壳插件未装载");

        // v1 的槽位必须先贡献再附着：取壳时 ShellWindow 已贡献三个槽位，这里才轮到 Feature。
        // ponytail: M1 还没有 Feature 工程，附着编排实际跑空；T14 接入 Feature 后由它贡献工具条与面板内容。
        pluginManager.RegisterFeatureAttachments(slotManager);

        return shell is Window window
                   ? window
                   : throw new InvalidOperationException("壳插件产物不是 Avalonia Window");
    }
}
