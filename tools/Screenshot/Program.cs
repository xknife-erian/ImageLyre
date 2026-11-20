using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Threading;

internal sealed class App : Application
{
}

internal static class Program
{
    private static int Main()
    {
        AppBuilder.Configure<App>()
                  .UseSkia()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                  .SetupWithoutStarting();

        var app = (App)Application.Current!;
        app.Resources.MergedDictionaries.Add(new Avalonia.Markup.Xaml.Styling.ResourceInclude(new Uri("avares://ImageLyre.Themes/Tokens.axaml")) { Source = new Uri("avares://ImageLyre.Themes/Tokens.axaml") });
        app.Styles.Add(new Avalonia.Markup.Xaml.Styling.StyleInclude(new Uri("avares://ImageLyre.Themes/Controls.axaml")) { Source = new Uri("avares://ImageLyre.Themes/Controls.axaml") });

        var stack = new StackPanel { Spacing = 8, Margin = new Thickness(24) };
        foreach (var sizeKey in new[] { "S20", "S16", "S13", "S11" })
        {
            var text = new TextBlock { Text = $"ImageLyre {sizeKey}" };
            text.Bind(TextBlock.FontSizeProperty, new Avalonia.Markup.Xaml.MarkupExtensions.DynamicResourceExtension($"Lyre.Type.Size.{sizeKey}"));
            stack.Children.Add(text);
        }

        stack.Children.Add(new Button { Content = "打开文件…" });
        stack.Children.Add(new Button { Content = "打开示例" });

        var window = new Window
        {
            Width = 480,
            Height = 320,
            Background = Brushes.White,
            Content = stack,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var frame = window.CaptureRenderedFrame();
        if (frame is null)
        {
            Console.WriteLine("FRAME NULL");
            return 1;
        }

        var outputDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "docs", "assets", "headless");
        Directory.CreateDirectory(outputDirectory);
        var path = Path.Combine(outputDirectory, "tokens-and-controls.png");
        frame.Save(path);
        Console.WriteLine($"SAVED {path} {frame.PixelSize.Width}x{frame.PixelSize.Height}");
        return 0;
    }
}
