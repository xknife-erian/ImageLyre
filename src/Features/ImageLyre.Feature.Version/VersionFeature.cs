using System.Reflection;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Avalonias.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.UI.Contracts;

namespace ImageLyre.Feature.Version;

/// <summary>「版本号」功能：向状态栏附着当前产品版本。</summary>
/// <remarks>
///     版本号来自程序集的信息版本；构建时由 <c>Directory.Build.props</c> 统一注入，
///     因此本功能读到的是产品版本，而不是某个具体工程的版本。
/// </remarks>
[Plugin(
    id: "ImageLyre.Version.Feature",
    name: "版本号",
    alias: "Version",
    description: "在状态栏显示当前版本")]
public sealed class VersionFeature : BasePicoFeatures
{
    /// <inheritdoc />
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules) =>
        modules.Optional<IWorkbenchShell>();

    /// <inheritdoc />
    public override void RegisterAttachments(IAttachmentRegistry registry)
    {
        var version = typeof(VersionFeature).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? "0.0.0";

        // 信息版本可能带构建元数据后缀（+commit），状态栏只显示版本号本体。
        var display = version.Split('+')[0];

        registry.AttachToSlot(WorkbenchSlots.StatusBar, new StatusBarAttachment($"v{display}", "ImageLyre 版本"));
    }
}
