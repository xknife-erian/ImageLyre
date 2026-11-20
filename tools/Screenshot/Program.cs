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

        var panel = ImageLyre.Feature.EmptyState.EmptyStateFeature.CreateEmptyStatePanel();

        var window = new Window
        {
            Width = 720,
            Height = 480,
            Background = Brushes.White,
            Content = panel,
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
        var path = Path.Combine(outputDirectory, "empty-state.png");
        frame.Save(path);
        Console.WriteLine($"SAVED {path} {frame.PixelSize.Width}x{frame.PixelSize.Height}");
        return 0;
    }
}
