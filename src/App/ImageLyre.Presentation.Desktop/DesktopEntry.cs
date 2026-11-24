using AtomUI;
using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Themes.AntDesign;

namespace ImageLyre.Presentation.Desktop;

/// <summary>桌面 Presentation 对 App 暴露的入口：只负责 AtomUI 主题与设计语言的装配。</summary>
/// <remarks>
/// <para>
/// <see cref="ConfigureAtomUI"/> 由 <c>Program.RunDesktop</c> 的 <c>UseAtomUI</c> 回调调用。AtomUI 6.3 起设计语言必须显式启用，
/// 只启控件包会在启动期抛 <c>ThemeLoadException</c>，因此控件包与 Ant Design 语言包在这里成对登记。
/// </para>
/// <para>
/// 本类不创建窗口、不启动宿主；桌面生命周期的转交由 <see cref="Startup.DesktopStartup.RunAsync"/> 完成，
/// 两者分开是为了让 App 的 AppBuilder 配置保持同步、无异步等待。
/// </para>
/// </remarks>
public static class DesktopEntry
{
    /// <summary>启用 AtomUI 桌面控件包与 Ant Design 设计语言。</summary>
    /// <param name="builder">AtomUI 的构建器，由 <c>UseAtomUI</c> 回调传入。</param>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> 为 <see langword="null"/>。</exception>
    /// <remarks>本方法在 Avalonia AppBuilder 配置阶段执行，必须保持同步且不创建窗口。</remarks>
    public static void ConfigureAtomUI(IAtomUIBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseDesktopControls();
        // AtomUI 6.3 起设计语言必须显式启用；只启控件包会在启动期抛 ThemeLoadException。
        builder.UseAntDesign(language => language.AddDesktopControls());
    }
}
