namespace ImageLyre.Bootstrap.Startup;

/// <summary>启动步骤的执行阶段，决定它可以在哪个线程上构造对象与访问服务。</summary>
/// <remarks>
/// <para>
/// 两个阶段与 <c>DESIGN-0002</c> §2 的术语一一对应：Host 阶段在任意线程执行、不构造 UI；Presentation 阶段在 Avalonia 消息循环启动后的 UI 线程执行。
/// 阶段不是调度器：Plan 不负责切换线程，调用方在执行前保证自己处在正确阶段。
/// </para>
/// </remarks>
public enum StepPhase
{
    /// <summary>宿主阶段：日志、DI、插件与宿主服务，可在任意线程执行，不创建依赖插件产物的界面控件。</summary>
    Host,

    /// <summary>表示阶段：欢迎窗关闭、槽位、取壳、附着与显示，必须在 Avalonia UI 线程执行。</summary>
    Presentation,
}
