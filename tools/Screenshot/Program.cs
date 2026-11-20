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

        var window = new Window
        {
            Width = 480,
            Height = 240,
            Background = Brushes.White,
            Content = new TextBlock { Text = "ImageLyre 空态", FontSize = 20 },
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var frame = window.CaptureRenderedFrame();
        if (frame is null)
        {
            Console.WriteLine("FRAME NULL");
            return 1;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "smoke.png");
        frame.Save(path);
        Console.WriteLine($"SAVED {path} {frame.PixelSize.Width}x{frame.PixelSize.Height}");
        return 0;
    }
}
