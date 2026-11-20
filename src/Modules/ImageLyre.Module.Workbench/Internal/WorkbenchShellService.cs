using Avalonia.Controls;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts.Workbench;

namespace ImageLyre.Module.Workbench.Internal;

/// <summary><see cref="IWorkbenchShell" /> 的实现：先贡献区域，再创建窗口；单例语义由字段缓存保证。</summary>
internal sealed class WorkbenchShellService : IWorkbenchShell
{
    private readonly WorkbenchModuleContext _context;
    private Window? _shell;
    private string? _pendingTitle;

    /// <summary>创建壳服务。</summary>
    /// <param name="context">工作台上下文，不可为 <see langword="null" />。</param>
    internal WorkbenchShellService(WorkbenchModuleContext context) => _context = context;

    /// <inheritdoc />
    public Window CreateShell()
    {
        if (_shell is not null)
        {
            return _shell;
        }

        var slots = _context.Slots;
        var registry = (IAttachmentRegistry)slots;

        // 先声明区域，Feature 附着时才有地方挂；同名区域重复贡献会抛异常，故这两步只走一次。
        registry.ContributeSlot(WorkbenchSlots.Menu, "主菜单");
        registry.ContributeSlot(WorkbenchSlots.Content, "主内容");
        registry.ContributeSlot(WorkbenchSlots.StatusBar, "状态栏");

        _shell = new WorkbenchShell(slots);

        if (_pendingTitle is not null)
        {
            _shell.Title = _pendingTitle;
        }

        return _shell;
    }

    /// <inheritdoc />
    public void SetTitle(string title)
    {
        _pendingTitle = title;

        if (_shell is not null)
        {
            _shell.Title = title;
        }
    }
}
