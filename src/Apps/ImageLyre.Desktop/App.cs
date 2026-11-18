using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Themes.Fluent;

namespace ImageLyre.Desktop;

/// <summary>ImageLyre 桌面应用宿主。</summary>
public sealed partial class App : Application
{
    /// <inheritdoc />
    public override void Initialize()
        => Styles.Add(new FluentTheme());

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new Window
            {
                Title = "ImageLyre",
                Width = 1280,
                Height = 800,
                Content = new TextBlock
                {
                    Text = "ImageLyre 新一代显微图像分析系统",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
