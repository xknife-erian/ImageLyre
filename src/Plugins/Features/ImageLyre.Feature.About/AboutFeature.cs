using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Support;
using ImageLyre.Contracts;
using ImageLyre.Shell.Contracts.Menus;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Feature.About;

/// <summary>关于 Feature：向菜单注册表贡献「帮助 → 关于」，点击时打开只读的关于窗口。</summary>
/// <remarks>
/// <para>
/// 菜单由宿主级 <see cref="IMenuRegistry"/> 登记，壳按注册表渲染；Feature 不引用壳插件，也不创建菜单控件。
/// 注册动作只发生在 <see cref="OnStartAsync"/>，此时壳窗口尚未构造，因此注册项一定会出现在首帧菜单里。
/// </para>
/// <para>
/// 生命周期：停止或释放时撤销菜单与菜单项注册；窗口按需创建，关闭即丢弃，不缓存控件。
/// </para>
/// </remarks>
[Plugin(id: "lyre.feature.about", name: "关于")]
public sealed class AboutFeature : BasePicoFeatures, ILocalizationSource, ISupportUsingIoC
{
    private static readonly ILogger s_logger = LogHub.For(typeof(AboutFeature));

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> s_texts =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.Ordinal)
        {
            ["zh-CN"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [AboutStringKeys.MenuHelp] = "帮助",
                [AboutStringKeys.MenuAbout] = "关于",
                [AboutStringKeys.WindowTitle] = "关于 ImageLyre",
                [AboutStringKeys.LabelProduct] = "产品",
                [AboutStringKeys.LabelVersion] = "版本",
                [AboutStringKeys.LabelRuntime] = "运行时",
                [AboutStringKeys.LabelOperatingSystem] = "操作系统",
                [AboutStringKeys.LabelLicense] = "许可证",
                [AboutStringKeys.LicenseName] = "木兰宽松许可证 v2.0",
                [AboutStringKeys.Copyright] = "© 2022-2026 稀布老咖。保留所有权利。",
                [AboutStringKeys.Close] = "关闭",
            },
            ["en-US"] = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [AboutStringKeys.MenuHelp] = "Help",
                [AboutStringKeys.MenuAbout] = "About",
                [AboutStringKeys.WindowTitle] = "About ImageLyre",
                [AboutStringKeys.LabelProduct] = "Product",
                [AboutStringKeys.LabelVersion] = "Version",
                [AboutStringKeys.LabelRuntime] = "Runtime",
                [AboutStringKeys.LabelOperatingSystem] = "Operating system",
                [AboutStringKeys.LabelLicense] = "License",
                [AboutStringKeys.LicenseName] = "Mulan Permissive Software License v2.0",
                [AboutStringKeys.Copyright] = "© 2022-2026 稀布老咖. All rights reserved.",
                [AboutStringKeys.Close] = "Close",
            },
        };

    private readonly AboutFeatureContext _context;
    private ILocalizationService? _localization;
    private IMenuRegistry? _menus;
    private IDisposable? _menuHandle;
    private IDisposable? _itemHandle;
    private AboutWindow? _window;
    private bool _disposed;

    /// <summary>创建一个只完成上下文装配的关于 Feature。</summary>
    /// <remarks>构造函数不解析服务、不注册菜单，也不创建窗口。</remarks>
    public AboutFeature()
    {
        _context = new AboutFeatureContext();
        UseContext(_context);
    }

    /// <summary>保存宿主 IoC 的延迟引用。</summary>
    /// <param name="iocLazy">宿主容器访问器的延迟引用，不可为 <see langword="null"/>。</param>
    /// <returns>当前 Feature，供注入链继续。</returns>
    public Task<IPicoPlugin> InjectAsync(Lazy<IIoCManager> iocLazy)
    {
        _context.SetIoC(iocLazy);
        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <summary>注册关于文案源，并向菜单注册表贡献「帮助」与「关于」。</summary>
    /// <returns>恒为 <see langword="true"/>。</returns>
    /// <remarks>
    /// 注册顺序是“先菜单、后菜单项”，因为注册表拒绝向不存在的菜单挂项；注册项在壳窗口构造前完成，
    /// 因此首帧菜单即可见。
    /// </remarks>
    protected override Task<bool> OnStartAsync()
    {
        _localization = _context.Localization;
        _menus = _context.Menus;

        _localization.AddSource(this);
        _menuHandle = _menus.RegisterMenu(new MenuDefinition("help", AboutStringKeys.MenuHelp, 100));
        _itemHandle = _menus.RegisterItem(
            new MenuItemDefinition("help", "about", AboutStringKeys.MenuAbout, 0, ShowAboutAsync));

        s_logger.LogInformation("关于 Feature 已启动，帮助菜单已注册。");
        return Task.FromResult(true);
    }

    /// <summary>停止时撤销菜单与菜单项注册。</summary>
    /// <returns>基类默认成功值。</returns>
    protected override Task<bool> OnStopAsync()
    {
        UnregisterMenus();
        return base.OnStopAsync();
    }

    /// <summary>释放时再次撤销注册并关闭已打开的关于窗口。</summary>
    protected override void OnDispose()
    {
        _disposed = true;
        UnregisterMenus();

        if (_window is { } window)
        {
            _window = null;
            Dispatcher.UIThread.Post(window.Close);
        }

        base.OnDispose();
    }

    /// <summary>按指定语言取关于文案。</summary>
    /// <param name="key">资源键。</param>
    /// <param name="culture">目标语言。</param>
    /// <param name="value">命中文案；未命中时为空串。</param>
    /// <returns>命中时为 <see langword="true"/>。</returns>
    public bool TryGet(string key, CultureInfo culture, out string value)
    {
        if (key is not null &&
            culture is not null &&
            s_texts.TryGetValue(culture.Name, out IReadOnlyDictionary<string, string>? texts) &&
            texts.TryGetValue(key, out string? text))
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    /// <summary>打开关于窗口；已打开时激活既有窗口而不重复创建。</summary>
    /// <returns>窗口关闭后完成的任务。</returns>
    private async Task ShowAboutAsync()
    {
        if (_disposed)
            return;

        if (_window is { } existing)
        {
            existing.Activate();
            return;
        }

        var window = new AboutWindow(
            _localization ?? throw new InvalidOperationException("关于 Feature 尚未启动，不能打开窗口。"));
        _window = window;
        window.Closed += (_, _) => _window = null;

        Window? owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

        if (owner is null)
        {
            window.Show();
            return;
        }

        await window.ShowDialog(owner);
    }

    /// <summary>撤销菜单与菜单项注册；重复调用幂等。</summary>
    private void UnregisterMenus()
    {
        _itemHandle?.Dispose();
        _itemHandle = null;
        _menuHandle?.Dispose();
        _menuHandle = null;
    }
}
