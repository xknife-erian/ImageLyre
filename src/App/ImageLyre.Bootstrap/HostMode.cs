namespace ImageLyre.Bootstrap;

/// <summary>宿主模式，只用于决定插件扫描期是否保留 Feature。</summary>
/// <remarks>
/// <para>
/// cangming 在 <c>PicoPluginSystem.Launch</c> 的扫描阶段读取 <c>PicoPluginSettings.HasUiRoute</c>，
/// 因此本枚举只影响 Feature 是否进入插件清单，不改变宿主是否创建窗口、是否初始化 Avalonia 或是否执行界面编排；
/// 那些决定属于 <c>ImageLyre.App</c>，本工程不感知，也不据此分支。
/// </para>
/// <para>
/// 两个取值与扫描开关一一对应：<see cref="Desktop"/> 保留 Feature，<see cref="Headless"/> 在扫描期过滤全部 Feature，
/// 后者用于 CLI 与后台宿主，避免 Feature 被启动却永远没有界面可附着。
/// </para>
/// </remarks>
public enum HostMode
{
    /// <summary>桌面宿主，扫描期保留 Feature。</summary>
    Desktop,

    /// <summary>无界面宿主，扫描期过滤全部 Feature。</summary>
    Headless,
}
