using System;
using System.Collections.Generic;

namespace ImageLyre.Architecture.Tests;

/// <summary>声明工程分层矩阵：目标工程、允许引用的工程，以及不得进入非 UI 工程的包前缀。</summary>
/// <remarks>
/// <para>矩阵按目标架构登记九个工程，包含尚未创建的两个（<c>ImageLyre.Headless</c>、<c>ImageLyre.Cli</c>），因为引用方向必须在工程出现之前定好，否则第一行代码就会把方向写死。</para>
/// <para><c>src/</c> 下现有工程是占位实现，编码开始前整体删除；因此测试对"工程尚未创建"保持容忍，只对"已存在的工程违反矩阵"与"未登记的工程"报错。</para>
/// </remarks>
internal static class Layering
{
    private static readonly Dictionary<string, string[]> s_allowedProjectReferences = new(StringComparer.Ordinal)
    {
        ["ImageLyre.Kernel"] = [],
        ["ImageLyre.Imaging"] = ["ImageLyre.Kernel"],
        ["ImageLyre.Codecs"] = ["ImageLyre.Kernel"],
        ["ImageLyre.Presentation"] = ["ImageLyre.Kernel"],
        ["ImageLyre.Sandbox"] = ["ImageLyre.Kernel", "ImageLyre.Presentation"],
        ["ImageLyre.UI"] = ["ImageLyre.Kernel", "ImageLyre.Presentation", "ImageLyre.Sandbox"],
        ["ImageLyre.Headless"] =
        [
            "ImageLyre.Kernel",
            "ImageLyre.Imaging",
            "ImageLyre.Codecs",
            "ImageLyre.Presentation",
            "ImageLyre.Sandbox",
        ],
        ["ImageLyre.Cli"] =
        [
            "ImageLyre.Headless",
            "ImageLyre.Kernel",
            "ImageLyre.Imaging",
            "ImageLyre.Codecs",
            "ImageLyre.Presentation",
            "ImageLyre.Sandbox",
        ],
        ["ImageLyre.Desktop"] =
        [
            "ImageLyre.Headless",
            "ImageLyre.Kernel",
            "ImageLyre.Imaging",
            "ImageLyre.Codecs",
            "ImageLyre.Presentation",
            "ImageLyre.Sandbox",
            "ImageLyre.UI",
        ],
    };

    private static readonly string[] s_uiPackagePrefixes = ["Avalonia", "AtomUI", "SkiaSharp"];

    private static readonly string[] s_uiLayerProjects = ["ImageLyre.UI", "ImageLyre.Desktop"];

    /// <summary>获取目标工程到允许引用工程的映射。</summary>
    internal static IReadOnlyDictionary<string, string[]> AllowedProjectReferences => s_allowedProjectReferences;

    /// <summary>获取允许引用 UI 栈包的工程，其余工程引用即违规。</summary>
    internal static IReadOnlyList<string> UiLayerProjects => s_uiLayerProjects;

    /// <summary>判断工程是否属于 UI 层。</summary>
    /// <param name="projectName">工程名。</param>
    /// <returns>属于 UI 层时为 <see langword="true"/>。</returns>
    internal static bool IsUiLayer(string projectName)
        => Array.Exists(s_uiLayerProjects, name => name.Equals(projectName, StringComparison.Ordinal));

    /// <summary>判断包标识是否属于 UI 栈。</summary>
    /// <param name="packageId">包标识。</param>
    /// <returns>属于 UI 栈时为 <see langword="true"/>。</returns>
    internal static bool IsUiPackage(string packageId)
        => Array.Exists(s_uiPackagePrefixes, prefix => packageId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
}
