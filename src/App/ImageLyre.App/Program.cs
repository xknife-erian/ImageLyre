using System.Runtime.CompilerServices;
using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using ImageLyre.App.Localization;
using ImageLyre.App.Startup;
using ImageLyre.Localization;

namespace ImageLyre.App;

/// <summary>进程入口：先判定宿主分支，再决定是否初始化 Avalonia。</summary>
/// <remarks>
/// <para>
/// 无界面分支只做参数解析与一行控制台输出，随后返回 0，期间不构造 <c>AppBuilder</c>、不创建窗口、不触碰任何 Avalonia 类型；
/// 桌面分支才创建本地化服务与 <see cref="App"/>，再由 <c>StartWithClassicDesktopLifetime</c> 进入 UI 循环。
/// </para>
/// <para>
/// 返回值边界：M1 的无界面分支固定返回 0；参数错误返回 2、运行失败返回 1 由 T06 的 CLI 负责，本入口不提前实现。
/// </para>
/// </remarks>
public static class Program
{
    /// <summary>进程入口。</summary>
    /// <param name="args">命令行参数。</param>
    /// <returns>进程退出码；M1 的桌面分支返回 Avalonia 生命周期的退出码，无界面分支固定 0。</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        StartupOptions options = StartupOptions.Parse(args);

        if (options.Headless)
        {
            // ponytail: M1 只做宿主分派，--version、--plugins、--help 的真实输出与退出码归 T06；这里先给一行可判读的模式提示。
            Console.WriteLine("ImageLyre 已进入无界面模式（M1 仅完成宿主分派）。");
            return 0;
        }

        return RunDesktop(options, args);
    }

    /// <summary>创建本地化服务并启动桌面宿主。</summary>
    /// <param name="options">启动参数解析结果。</param>
    /// <param name="args">原样传给 Avalonia 生命周期的命令行参数。</param>
    /// <returns>Avalonia 经典桌面生命周期的退出码。</returns>
    /// <remarks>
    /// 单独成方法并禁止内联，是为了让 <see cref="Main"/> 的无界面分支在 JIT 期也不必解析 Avalonia 与 AtomUI 的类型引用，
    /// 从而把“无界面分支不初始化 Avalonia”固定在代码形状上，而不只依赖运行期恰好没有走到这一支。
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int RunDesktop(StartupOptions options, string[] args)
    {
        var localization = new LocalizationService();
        localization.AddSource(new AppStringSource());

        return AppBuilder.Configure<App>(() => new App(options, localization))
            .UseAtomUIPlatformDetect(AtomUIWindowingPlatform.Auto)
            .UseAtomUI(builder => builder.UseDesktopControls())
            .StartWithClassicDesktopLifetime(args);
    }
}
