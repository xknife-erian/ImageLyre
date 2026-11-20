using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using Cangming.PicoPlugins.Core.Support;
using ImageLyre.Module.Workbench.Internal;
using ImageLyre.UI.Contracts.Workbench;

namespace ImageLyre.Module.Workbench;

/// <summary>工作台容器模块：声明菜单、内容与状态栏区域，并在界面就绪后创建主窗口。</summary>
/// <remarks>
///     本模块是全应用唯一创建控件的 Module。<see cref="Build" /> 只产出壳服务的延迟实例，
///     真正的区域贡献与窗口构造发生在宿主调用 <see cref="IWorkbenchShell.CreateShell" /> 时，
///     也就是 Avalonia 就绪之后——这条时序不能提前。
/// </remarks>
[Plugin(
    id: "ImageLyre.Workbench.Module",
    name: "工作台容器",
    alias: "Workbench",
    description: "贡献文件菜单、帮助菜单、内容与状态栏区域，并创建主窗口")]
public sealed class WorkbenchModule : BasePicoModule<IWorkbenchShell>, ISupportSlotManager
{
    private Lazy<IWorkbenchShell>? _shellLazy;

    /// <summary>创建工作台模块，并绑定工作台上下文。</summary>
    public WorkbenchModule() => UseContext(new WorkbenchModuleContext());

    private WorkbenchModuleContext Context => ContextOf<WorkbenchModuleContext>();

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Naming",
        "CA1725:Parameter names should match base declaration",
        Justification = "沧溟的 Slot 注入与模块注入两个接口对同一方法声明了不同参数名，实现只能满足其一。")]
    public Task<IPicoPlugin> InjectAsync(Lazy<ISlotManager> slotManagerLazy)
    {
        Context.SetSlotManager(slotManagerLazy);

        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <inheritdoc />
    public override Lazy<IWorkbenchShell> Build(params object[] args) =>
        _shellLazy ??= new Lazy<IWorkbenchShell>(() => new WorkbenchShellService(Context));

    /// <inheritdoc />
    protected override Task<bool> OnStartAsync()
    {
        OnServiceStarted(new PluginStatusReportEventArgs(nameof(WorkbenchModule), "就绪"));

        return Task.FromResult(true);
    }
}
