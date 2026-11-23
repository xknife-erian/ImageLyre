using System.Reactive.Linq;
using System.Runtime.InteropServices;
using ImageLyre.Contracts.Platform;

namespace ImageLyre.Module.Platform.Internal;

/// <summary>平台能力矩阵的 M1 实现：Windows 声明八项支持，其余平台按未实测处理。</summary>
/// <remarks>
/// <para>
/// 当前系统由构造参数注入，缺省时读 <see cref="RuntimeInformation"/>；矩阵在实例生命周期内不变，
/// <see cref="CapabilityChanged"/> 返回永不触发的流，调用方不得把“没有事件”当成能力缺失。
/// </para>
/// <para>
/// 只声明实测过的结果：Windows 的八项能力全部为真，macOS、Linux 与 Unknown 全部为假，
/// 因为 M1 没有在这些平台做过能力验收，把未实测写成可用会让适配层在运行期才失败。
/// </para>
/// </remarks>
internal sealed class PlatformCapabilities : IPlatformCapabilities
{
    private static readonly HashSet<PlatformCapability> s_windowsCapabilities =
    [
        PlatformCapability.SystemTray,
        PlatformCapability.OSCredentials,
        PlatformCapability.OSNotifications,
        PlatformCapability.FileAssociations,
        PlatformCapability.UrlSchemes,
        PlatformCapability.SingleInstance,
        PlatformCapability.AutoUpdate,
        PlatformCapability.CrashDump,
    ];

    /// <summary>创建一个平台能力矩阵。</summary>
    /// <param name="operatingSystem">当前系统；为空时从 <see cref="RuntimeInformation"/> 判定，便于测试注入。</param>
    internal PlatformCapabilities(OperatingSystemKind? operatingSystem = null) =>
        CurrentOS = operatingSystem ?? DetectCurrentOS();

    /// <summary>当前运行的操作系统类别。</summary>
    public OperatingSystemKind CurrentOS { get; }

    /// <summary>查询当前平台是否提供指定能力。</summary>
    /// <param name="capability">平台能力项。</param>
    /// <returns>仅 Windows 的八项实测能力返回 <see langword="true"/>。</returns>
    public bool Supports(PlatformCapability capability) =>
        CurrentOS == OperatingSystemKind.Windows && s_windowsCapabilities.Contains(capability);

    /// <summary>永不触发的变更流：M1 不做运行期平台探测。</summary>
    public IObservable<PlatformCapability> CapabilityChanged => Observable.Never<PlatformCapability>();

    // ponytail: macOS/Linux 的矩阵等 M2 实测补；M1 只有 Windows 的八项实测结果。
    private static OperatingSystemKind DetectCurrentOS()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OperatingSystemKind.Windows;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OperatingSystemKind.MacOS;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OperatingSystemKind.Linux;

        return OperatingSystemKind.Unknown;
    }
}
