namespace ImageLyre.Contracts.Platform;

/// <summary>当前运行的操作系统类别，用于平台能力判定与适配层选择。</summary>
/// <remarks>
/// 只用于选择适配实现，不得作为业务分支依据：契约层与插件层按平台分叉就违反了「三端同构」，
/// 需要区分平台时一律先查 <see cref="IPlatformCapabilities.Supports"/>。
/// </remarks>
public enum OperatingSystemKind
{
    /// <summary>Microsoft Windows。</summary>
    Windows,

    /// <summary>Apple macOS。</summary>
    MacOS,

    /// <summary>Linux 发行版。</summary>
    Linux,

    /// <summary>未知或无法识别，例如无图形界面的测试环境。</summary>
    Unknown,
}

/// <summary>平台相关能力的判定项。</summary>
/// <remarks>
/// 枚举只增不改：新增一项表示多一个可判定的能力，既有取值的名称与语义保持不变；
/// 删除或改名会让已经写进配置与日志的判定结果失去含义。
/// </remarks>
public enum PlatformCapability
{
    /// <summary>系统托盘图标，例如 Windows 托盘区与 macOS 菜单栏。</summary>
    SystemTray,

    /// <summary>操作系统凭据库，例如 Windows Credential Manager 与 macOS Keychain。</summary>
    OSCredentials,

    /// <summary>操作系统原生通知，区别于应用内通知。</summary>
    OSNotifications,

    /// <summary>文件关联注册，即「打开方式」。</summary>
    FileAssociations,

    /// <summary>URL scheme 注册，用于深链接。</summary>
    UrlSchemes,

    /// <summary>单实例互斥与二次启动的参数转发。</summary>
    SingleInstance,

    /// <summary>应用自更新：检查、下载、应用并重启。</summary>
    AutoUpdate,

    /// <summary>崩溃转储的捕获与上报。</summary>
    CrashDump,
}

/// <summary>平台能力矩阵：声明当前操作系统与各平台能力的支持状态。</summary>
/// <remarks>
/// <para>
/// 判定的一致性契约是硬约束：<see cref="Supports"/> 返回 <see langword="false"/> 时，宿主的适配层必须取不到该能力的实现，
/// 而不是取到之后在运行期失败；返回 <see langword="true"/> 也不承诺一定可用，实现可以另有运行期限制并在自己的文档里写明。
/// </para>
/// <para>
/// 线程与生命周期：<see cref="Supports"/> 与 <see cref="CurrentOS"/> 必须允许任意线程调用；
/// <see cref="CapabilityChanged"/> 只在宿主 UI 线程上触发，订阅方负责在销毁时退订。实例由平台插件在启动期注册，
/// 贯穿整个进程生命周期，调用方不得释放它。
/// </para>
/// <para>
/// 本接口抄录自 cangming 的 `Cangming.Platform.Contracts`（0.1.9.1122）并按本产品口径裁剪；
/// 本仓可独立演进，向上游提升不由本仓负责，因此改动不受上游兼容性约束，但仍须遵守「只增不改」。
/// </para>
/// </remarks>
public interface IPlatformCapabilities
{
    /// <summary>当前运行的操作系统类别。</summary>
    /// <remarks>读取本身不触发任何探测；返回值在进程生命周期内不变。</remarks>
    OperatingSystemKind CurrentOS { get; }

    /// <summary>查询当前平台是否提供指定能力。</summary>
    /// <param name="capability">平台能力项。</param>
    /// <returns>平台提供该能力时为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
    bool Supports(PlatformCapability capability);

    /// <summary>某项平台能力的支持状态变化时推送。</summary>
    /// <remarks>
    /// 订阅后不重放历史，只推后续变更；实现若不支持运行期变化，可以返回一条永不触发的流，
    /// 调用方必须把「没有事件」视为状态未变，而不是能力缺失。
    /// </remarks>
    IObservable<PlatformCapability> CapabilityChanged { get; }
}
