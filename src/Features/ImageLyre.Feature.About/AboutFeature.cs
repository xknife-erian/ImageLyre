using System;
using System.Reflection;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using CommunityToolkit.Mvvm.Input;
using ImageLyre.Plugins;
using ImageLyre.Shell.Contracts;

namespace ImageLyre.Feature.About;

/// <summary>「关于」功能：官方插件，菜单在代码里声明。</summary>
/// <remarks>菜单项与命令同处一文件，命令名写错在编译期就能发现，不需要描述文件。</remarks>
[Plugin(
    id: "ImageLyre.About.Feature",
    name: "关于",
    alias: "About",
    description: "在帮助菜单提供关于项")]
public sealed class AboutFeature : ImageLyreCodeMenuFeature
{
    /// <inheritdoc />
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules) =>
        modules.Optional<IWorkbenchShell>();

    /// <inheritdoc />
    public override void RegisterAttachments(IAttachmentRegistry registry)
    {
        var command = new RelayCommand(ShowAboutWindow);

        AttachMenu(registry, "帮助", new MenuCommandAttachment("关于", "ShowAboutWindow", command));
    }

    private static void ShowAboutWindow()
    {
        var version = typeof(AboutFeature).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "0.0.0";

        var window = new AtomUIWindow
        {
            Title = "关于 ImageLyre",
            Width = 420,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Content = new StackPanel
            {
                Spacing = 8,
                Margin = new Thickness(24),
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new TextBlock { Text = "ImageLyre", FontSize = 22, FontWeight = Avalonia.Media.FontWeight.SemiBold },
                    new TextBlock { Text = "跨平台显微图像定量分析系统" },
                    new TextBlock { Text = $"版本 {version}" },
                },
            },
        };

        window.Show();
    }
}
