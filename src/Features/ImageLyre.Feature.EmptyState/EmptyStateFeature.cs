using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts.Workbench;
using LyreContentAttachment = ImageLyre.UI.Contracts.Attachments.ContentAttachment;

namespace ImageLyre.Feature.EmptyState;

/// <summary>「空态」功能：没有打开图像时，内容区只放一句怎么开始的话与两个入口。</summary>
/// <remarks>
///     按主界面规格 01 空态：不摆工具与档案，标题与提示居中，两个按钮一主一次；
///     字号与颜色一律走主题令牌，功能里不出现裸数值。
/// </remarks>
[Plugin(
    id: "ImageLyre.EmptyState.Feature",
    name: "空态",
    alias: "EmptyState",
    description: "打开图像前的引导页")]
public sealed class EmptyStateFeature : BasePicoFeatures
{
    /// <inheritdoc />
    public override void RegisterAttachments(IAttachmentRegistry registry)
    {
        registry.AttachToSlot(
            WorkbenchSlots.Content,
            new LyreContentAttachment { Create = BuildEmptyState, Order = 0 });
    }

    private static StackPanel BuildEmptyState()
    {
        var title = new TextBlock { Text = "把图放上去" };
        Bind(title, TextBlock.FontSizeProperty, "Lyre.Type.Size.S20");

        var hint = new TextBlock
        {
            Text = "16 位、带校准的 TIFF 会被直接识别；不支持的格式会说明原因。",
            HorizontalAlignment = HorizontalAlignment.Center,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };
        Bind(hint, TextBlock.FontSizeProperty, "Lyre.Type.Size.S13");

        var open = new Button { Content = "打开文件…", HorizontalAlignment = HorizontalAlignment.Center };
        var sample = new Button { Content = "打开示例", HorizontalAlignment = HorizontalAlignment.Center };

        return new StackPanel
        {
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { title, hint, open, sample },
        };
    }

    private static void Bind(AvaloniaObject target, AvaloniaProperty property, string key) =>
        target.Bind(property, new DynamicResourceExtension(key));
}
