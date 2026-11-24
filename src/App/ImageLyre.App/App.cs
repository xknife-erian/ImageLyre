using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ImageLyre.Contracts;
using ImageLyre.Presentation.Desktop.Startup;
using Microsoft.Extensions.Logging;

namespace ImageLyre.App;

/// <summary>ImageLyre 的 Avalonia 应用：只把桌面生命周期、本地化与日志出口转交给 Presentation 的桌面入口。</summary>
/// <remarks>
/// <para>
/// 宿主分派在 <see cref="Program.Main"/> 完成，本类因此只在桌面分支被构造；桌面编排全部由
/// <see cref="DesktopStartup.RunAsync"/> 承担，本类不持有编排状态，也不引用 Bootstrap 或任何插件类型。
/// </para>
/// <para>
/// 转交是 fire-and-forget：<see cref="DesktopStartup.RunAsync"/> 在第一个 <c>await</c> 之前同步挂上启动界面，
/// 因此经典桌面生命周期仍能显示它；其内部捕获全部启动期异常，不会把未观察的任务异常留给进程。
/// </para>
/// </remarks>
public sealed partial class App : Application
{
    private readonly ILocalizationService _localization;
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>创建应用实例。</summary>
    /// <param name="localization">本地化服务，欢迎窗口与主界面共用同一实例。</param>
    /// <param name="loggerFactory">宿主日志工厂，转交给 Presentation 用于构建 Generic Host。</param>
    /// <exception cref="ArgumentNullException"><paramref name="localization"/> 或 <paramref name="loggerFactory"/> 为 <see langword="null"/>。</exception>
    public App(ILocalizationService localization, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(localization);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _localization = localization;
        _loggerFactory = loggerFactory;
    }

    /// <inheritdoc />
    /// <remarks>
    /// 本方法只判断生命周期类型并转交桌面入口，不创建窗口、不启动宿主；<c>base</c> 调用保持在最后，
    /// 因为 AtomUI 的源生成代码依赖基类在生命周期完成后补本地化与启动图宿主。
    /// </remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            _ = DesktopStartup.RunAsync(desktop, _localization, _loggerFactory);

        base.OnFrameworkInitializationCompleted();
    }
}
