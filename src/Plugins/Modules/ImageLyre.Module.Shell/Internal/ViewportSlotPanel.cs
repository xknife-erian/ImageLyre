using Avalonia.Layout;
using Cangming.PicoPlugins.Avalonias.Slots;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.Contracts;
using ImageLyre.Shell.Contracts;
using AtomTextBlock = AtomUI.Desktop.Controls.TextBlock;

namespace ImageLyre.Module.Shell.Internal;

/// <summary>图像视口槽位面板：没有附着物时显示“无图像”空态。</summary>
/// <remarks>
/// 视口空态是壳的静态内容，不由 Feature 贡献；有 <c>ContentAttachment</c> 时交回基类按类型渲染。
/// 空态与真实内容共用同一个 <see cref="SlotPanel.Content"/>，因此同一时刻只会显示一份。
/// </remarks>
internal sealed class ViewportSlotPanel : SlotPanel
{
    private readonly ILocalizationService _localization;
    private AtomTextBlock? _emptyState;

    /// <summary>创建绑定视口槽位的面板。</summary>
    /// <param name="slotManager">与工具栏、辅助面板共享的槽位管理器。</param>
    /// <param name="localization">壳文案服务，用于取视口空态文本。</param>
    /// <exception cref="ArgumentNullException"><paramref name="slotManager"/> 或 <paramref name="localization"/> 为 <see langword="null"/>。</exception>
    internal ViewportSlotPanel(ISlotManager slotManager, ILocalizationService localization)
    {
        ArgumentNullException.ThrowIfNull(slotManager);
        ArgumentNullException.ThrowIfNull(localization);

        _localization = localization;
        SlotName = ShellSlots.Viewport;
        SlotManager = slotManager;
    }

    /// <summary>按当前语言刷新视口空态文本；有真实内容时不改内容控件。</summary>
    /// <remarks>只改已有空态文本，不重建控件树；语言切换由 <see cref="ShellWindow"/> 统一调度。</remarks>
    internal void RefreshTexts()
    {
        if (_emptyState is not null)
            _emptyState.Text = _localization.Get(ShellStringKeys.ViewportEmpty);
    }

    /// <summary>先让基类按附着物渲染，再在结果为空时补上“无图像”空态。</summary>
    /// <remarks>M1 不提供运行期摘除；将来若允许摘除，基类把内容置空后这里会重新显示空态。</remarks>
    protected override void UpdateContent()
    {
        base.UpdateContent();
        if (Content is not null)
            return;

        // ponytail: 视口空态是壳的静态占位，图像视口能力接入后由真实内容替换。
        _emptyState ??= new AtomTextBlock
        {
            FontSize = 16,
            Opacity = 0.5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        _emptyState.Text = _localization.Get(ShellStringKeys.ViewportEmpty);
        Content = _emptyState;
    }
}
