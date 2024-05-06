using System;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using CommunityToolkit.Mvvm.Input;
using ImageLyre.Feature.PluginCenter.Internal;
using ImageLyre.Plugins;
using ImageLyre.UI.Contracts;
using ImageLyre.Contracts;

namespace ImageLyre.Feature.PluginCenter;

/// <summary>「插件中心」功能：官方插件，菜单 code-first，窗口由界面侧自己创建。</summary>
/// <remarks>
///     插件与区域数据来自 <see cref="IPluginCenter" /> 契约，提供方是不含界面的模块；
///     本功能只负责窗口，因此可以自由使用 AtomUI。
/// </remarks>
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

        if (center is null)
        {
            return;
        }

        new PluginCenterWindow(center).Show();
    }
}
