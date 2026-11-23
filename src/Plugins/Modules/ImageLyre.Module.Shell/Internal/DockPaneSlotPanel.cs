using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Cangming.Logs;
using Cangming.PicoPlugins.Avalonias.Slots;
using Cangming.PicoPlugins.Core.Features;
using Cangming.Toolkit.Ordering;
using ImageLyre.Contracts;
using ImageLyre.Shell.Attachments;
using ImageLyre.Shell.Contracts;
using Microsoft.Extensions.Logging;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace ImageLyre.Module.Shell.Internal;

/// <summary>辅助面板专用呈现器：只把 <see cref="DockPaneAttachment"/> 渲染成页签，其它附着物跳过并记日志。</summary>
/// <remarks>
/// <para>
/// cangming 的 <see cref="SlotPanel"/> 不认识 Lyre 自己的 <see cref="DockPaneAttachment"/>，因此这里继承它并覆写
/// <see cref="SlotPanel.UpdateContent"/>；这是 cangming <c>ADR-0007</c> 规定的应用词汇扩展方式，不改基座。
/// </para>
/// <para>
/// 排序：附着物实现 <see cref="IOrderable"/>，按显式 <see cref="IOrderable.Order"/> 排序后呈现；M1 没有容量准入策略，
/// 多项会全部呈现。空态“尚无测量”是壳的静态内容，不依赖任何 Feature。
/// </para>
/// </remarks>
internal sealed class DockPaneSlotPanel : SlotPanel
{
    private static readonly ILogger s_logger = LogHub.For(typeof(DockPaneSlotPanel));

    private readonly ILocalizationService _localization;
    private AtomTextBlock? _emptyState;

    /// <summary>创建绑定辅助面板槽位的呈现器。</summary>
    /// <param name="slotManager">与工具栏、视口共享的槽位管理器。</param>
    /// <param name="localization">壳文案服务，用于取面板空态文本。</param>
    /// <exception cref="ArgumentNullException"><paramref name="slotManager"/> 或 <paramref name="localization"/> 为 <see langword="null"/>。</exception>
    internal DockPaneSlotPanel(ISlotManager slotManager, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(slotManager);
        ArgumentNullException.ThrowIfNull(localization);

        _localization = localization;
        SlotName = ShellSlots.Panel;
        SlotManager = slotManager;
        MinWidth = 320;
    }

    /// <summary>按当前语言刷新面板空态文本；有页签内容时不改页签标题。</summary>
    /// <remarks>只改已有空态文本，不重建控件树；页签标题属于附着物，由贡献方自己负责。</remarks>
    internal void RefreshTexts()
    {
        if (_emptyState is not null)
            _emptyState.Text = _localization.Get(ShellStringKeys.PanelEmpty);
    }

    /// <summary>重新读取面板槽位，渲染页签或“尚无测量”空态。</summary>
    /// <remarks>
    /// 非 <see cref="DockPaneAttachment"/> 的附着物不参与渲染，只记一条警告；全部被跳过时与空槽位一样显示空态。
    /// </remarks>
    protected override void UpdateContent()
    {
        if (SlotManager is null || string.IsNullOrEmpty(SlotName))
        {
            Content = null;
            return;
        }

        IReadOnlyList<IFeatureAttachment> attachments = SlotManager.GetSlotContents(SlotName);
        var panes = new List<DockPaneAttachment>();

        foreach (IFeatureAttachment attachment in attachments)
        {
            if (attachment is DockPaneAttachment pane)
                panes.Add(pane);
            else
                s_logger.LogWarning("辅助面板忽略不支持的附着物类型：{AttachmentType}。", attachment.GetType().Name);
        }

        if (panes.Count == 0)
        {
            Content = CreateEmptyState();
            return;
        }

        // ponytail: M1 没有面板容量准入策略，多项按 Order 全部呈现；G3 换成准入策略后再收窄。
        panes.SortBySpecifiedOrder();

        var tabs = new TabControl();
        foreach (DockPaneAttachment pane in panes)
        {
            DetachFromParent(pane.Content);
            tabs.Items.Add(new TabItem
            {
                Header = pane.Title,
                Content = pane.Content,
            });
        }

        Content = tabs;
    }

    /// <summary>创建或刷新“尚无测量”空态文本。</summary>
    /// <returns>居中的占位文本控件。</returns>
    /// <remarks>空态是壳的静态内容，真实测量面板由 Feature 通过面板槽位附着。</remarks>
    private AtomTextBlock CreateEmptyState()
    {
        // ponytail: 面板空态先用静态文本，测量能力接入后改为按领域状态渲染。
        _emptyState ??= new AtomTextBlock
        {
            FontSize = 14,
            Opacity = 0.5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _emptyState.Text = _localization.Get(ShellStringKeys.PanelEmpty);
        return _emptyState;
    }

    /// <summary>把内容控件从旧容器摘下来，避免重复渲染时同一控件有两个父级。</summary>
    /// <param name="control">要重新挂载的内容控件。</param>
    /// <remarks>
    /// 面板重建页签时会复用 <see cref="DockPaneAttachment.Content"/>；若旧父级是页签、内容呈现器或面板，
    /// 先断掉父子关系，再挂到新的 <see cref="TabItem"/> 上。
    /// </remarks>
    private static void DetachFromParent(Control control)
    {
        switch (control.Parent)
        {
            case Panel panel:
                panel.Children.Remove(control);
                break;

            case ContentControl contentControl:
                contentControl.Content = null;
                break;

            case ContentPresenter presenter:
                presenter.Content = null;
                break;

            case Decorator decorator:
                decorator.Child = null;
                break;
        }
    }
}
