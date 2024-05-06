using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ImageLyre.App;

/// <summary>Avalonia 应用壳：只把主窗口编排结果挂到桌面生命周期上。</summary>
/// <remarks>本类必须声明为 <c>partial</c>：AtomUI 的源生成器会为它补启动挂接，漏掉编译期即报错。</remarks>
public partial class App : Application
{
    private readonly Func<Window> _composeMainWindow;

    /// <summary>供 Avalonia 资源加载器识别的无参构造；运行时入口使用工厂注入。</summary>
    /// <exception cref="InvalidOperationException">直接由资源加载器创建时抛出。</exception>
    public App() =>
        throw new InvalidOperationException("App 必须由 Program 的主窗口编排工厂创建。");

    /// <summary>创建应用壳。</summary>
    /// <param name="composeMainWindow">主窗口编排工厂，在 Avalonia 就绪后执行。</param>
    internal App(Func<Window> composeMainWindow) => _composeMainWindow = composeMainWindow;

    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _composeMainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
