using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Features;
using ImageLyre.UI.Contracts.Busy;
using ImageLyre.UI.Contracts.Workbench;
using System.Threading.Tasks;
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

    private StackPanel BuildEmptyState()
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

        // D2 的进度源是模拟的：分段上报，演示壳层遮罩；真实解码属业务插件，不在基座里做。
        sample.Click += async (_, _) =>
        {
            IWorkbenchShell? shell = BaseContext.UseOptional<IWorkbenchShell>()?.Value;
            shell?.AddDocumentTab("sample.tif", "sample.tif");
            await SimulateOpenAsync(BaseContext);
        };

        // D3 缺校准：一根内联黄条，贴在内容上方；默认隐藏，用按钮演示开关。
        var warning = BuildWarningBar();
        var demoWarning = new Button
        {
            Content = "模拟缺校准",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        demoWarning.Click += (_, _) => warning.IsVisible = !warning.IsVisible;

        var stack = new StackPanel
        {
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { warning, title, hint, open, sample, demoWarning },
        };

        return stack;
    }

    private const string SAMPLE_MESSAGE = "正在打开 sample.tif";

    private static void Bind(AvaloniaObject target, AvaloniaProperty property, string key) =>
        target.Bind(property, new DynamicResourceExtension(key));

    private static async Task SimulateOpenAsync(BaseFeatureContext context)
    {
        Lazy<IBusyManager>? lazy = context.UseOptional<IBusyManager>();
        if (lazy is null)
        {
            return;
        }

        IBusyManager busy = lazy.Value;

        var indicator = new BusyIndicator { Type = BusyIndicatorType.Progress, Message = SAMPLE_MESSAGE };
        var id = busy.StartBusy(indicator);
        try
        {
            for (uint percent = 10; percent <= 100; percent += 10)
            {
                await Task.Delay(120).ConfigureAwait(true);
                busy.ReportProgress(id, new BusyRecord(id, percent, SAMPLE_MESSAGE));
            }

            busy.ReportCompleted(id);
        }
        finally
        {
            // 任何异常都要收起遮罩，否则界面会一直停在“正在打开”。
            busy.ReportFailed(id);
        }
    }

    private static Border BuildWarningBar()
    {
        var text = new TextBlock
        {
            Text = "这张图没有校准信息，测量结果不能标定到微米。请先设置比例尺。",
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
        };
        Bind(text, TextBlock.FontSizeProperty, "Lyre.Type.Size.S13");

        var bar = new Border
        {
            IsVisible = false,
            Padding = new Avalonia.Thickness(12, 8),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = text,
        };
        Bind(bar, Border.BackgroundProperty, "Lyre.Signal.Warning.Background");
        Bind(bar, Border.BorderBrushProperty, "Lyre.Signal.Warning.Line");
        bar.BorderThickness = new Avalonia.Thickness(2, 0, 0, 0);

        return bar;
    }
}
