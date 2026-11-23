using System.Runtime.CompilerServices;
using AtomUI;
using AtomUI.Desktop.Controls;
using Avalonia;
using ImageLyre.App.Cli;
using ImageLyre.App.Localization;
using ImageLyre.App.Startup;
using ImageLyre.Localization;

namespace ImageLyre.App;

/// <summary>进程入口：先解析命令，再决定走桌面宿主还是 CLI。</summary>
/// <remarks>
/// <para>
/// 命令行参数由 <see cref="StartupOptions.Parse"/> 解析；桌面命令在 <see cref="RunDesktop"/> 里创建本地化服务并启动
/// AtomUI 桌面生命周期，其余命令交给 <see cref="CliHost.RunAsync"/> 并在无界面条件下返回它的退出码。
/// </para>
/// <para>
/// <see cref="Main"/> 本身不引用任何 Avalonia 类型，<see cref="RunDesktop"/> 另加 <see cref="MethodImplOptions.NoInlining"/>
/// 隔离 JIT 期的方法解析，因此 CLI 分支不会初始化 Avalonia，也不会创建窗口。
/// </para>
/// <para>
/// 退出码：CLI 命令按 <see cref="CliHost"/> 的 0/2/1 约定返回，桌面分支返回 Avalonia 经典桌面生命周期的退出码。
/// </para>
/// </remarks>
public static class Program
{
    /// <summary>进程入口。</summary>
    /// <param name="args">命令行参数，原样交给 <see cref="StartupOptions.Parse"/> 与桌面生命周期。</param>
    /// <returns>进程退出码。</returns>
    [STAThread]
    public static int Main(string[] args)
    {
        StartupOptions options = StartupOptions.Parse(args);

        if (options.Command == StartupCommand.Desktop)
            return RunDesktop(options, args);

        return CliHost.RunAsync(options, Console.Out, Console.Error, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>创建本地化服务并启动桌面宿主。</summary>
    /// <param name="options">启动参数解析结果。</param>
    /// <param name="args">原样传给 Avalonia 生命周期的命令行参数。</param>
    /// <returns>Avalonia 经典桌面生命周期的退出码。</returns>
    /// <remarks>
    /// 单独成方法并禁止内联，是为了让 <see cref="Main"/> 的 CLI 分支在 JIT 期也不必解析 Avalonia 与 AtomUI 的类型引用，
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
