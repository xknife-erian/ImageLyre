using Avalonia;

namespace ImageLyre.Desktop;

/// <summary>桌面应用入口。</summary>
internal static class Program
{
    /// <summary>启动 Avalonia 桌面生命周期。</summary>
    /// <param name="args">命令行参数。</param>
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    /// <summary>构造应用构建器，供设计器与无头测试复用。</summary>
    /// <returns>已配置平台探测、字体与日志的应用构建器。</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
