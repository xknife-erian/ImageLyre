using System;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using CommunityToolkit.Mvvm.Input;
using ImageLyre.Plugins;
using ImageLyre.Shell.Contracts;

namespace ImageLyre.Feature.PluginCenter;

/// <summary>「插件中心」功能：官方插件，菜单在代码里声明，点击打开插件中心窗口。</summary>
[Plugin(
    id: "ImageLyre.PluginCenter.Feature",
    name: "插件中心入口",
    alias: "PluginCenter",
    description: "在帮助菜单提供插件中心入口")]
public sealed class PluginCenterFeature : ImageLyreCodeMenuFeature
{
    /// <inheritdoc />
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules) => modules.Optional<IPluginCenter>();

    /// <inheritdoc />
    public override void RegisterAttachments(IAttachmentRegistry registry)
    {
        var command = new RelayCommand(OpenPluginCenter);

        AttachMenu(registry, "帮助", new MenuCommandAttachment("插件中心", "OpenPluginCenter", command, order: 10));
    }

    private void OpenPluginCenter()
    {
        var center = BaseContext.UseOptional<IPluginCenter>()?.Value;

        center?.CreateWindow().Show();
    }
}
