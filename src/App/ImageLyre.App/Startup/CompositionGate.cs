namespace ImageLyre.App.Startup;

/// <summary>一次启动的编排与关停门闩：Begin 与 Stop 各只成功一次，Stop 之后不得再 Begin。</summary>
/// <remarks>
/// <para>
/// 状态只有三个：Idle → Begun → Stopped。<c>TryBegin</c> 用 <c>Interlocked.CompareExchange</c> 从 Idle 迁移到 Begun，
/// <c>TryStop</c> 用 <c>Interlocked.Exchange</c> 从 Idle 或 Begun 迁移到 Stopped；已经 Stopped 时再调用返回
/// <see langword="false"/>，因此重复关停不重复释放。
/// </para>
/// <para>
/// <c>TryStop</c> 允许先于 <c>TryBegin</c>：桌面在启动完成前退出时，先关停会把门闩钉在 Stopped，
/// 随后迟到的 <c>ComposeAsync</c> 无法再开始，避免“窗口已经退出、插件却才被启动”的半启动状态。
/// </para>
/// <para>
/// 线程安全：两个方法都用 Interlocked，可在任意线程并发调用；本类型不持有资源、不创建线程，也没有需要释放的字段。
/// </para>
/// </remarks>
internal sealed class CompositionGate
{
    private const int Idle = 0;
    private const int Begun = 1;
    private const int Stopped = 2;

    private int _state;

    /// <summary>尝试开始一次编排；仅当门闩仍在 Idle 时返回 <see langword="true"/>。</summary>
    /// <returns>第一次成功调用返回 <see langword="true"/>；已经开始或已经关停时返回 <see langword="false"/>。</returns>
    /// <remarks>原子迁移 Idle → Begun，并发调用下只有一个调用方能抢到开始权。</remarks>
    public bool TryBegin() => Interlocked.CompareExchange(ref _state, Begun, Idle) == Idle;

    /// <summary>尝试关停一次；仅当门闩尚未处于 Stopped 时返回 <see langword="true"/>。</summary>
    /// <returns>第一次关停调用返回 <see langword="true"/>；已经关停时返回 <see langword="false"/>。</returns>
    /// <remarks>
    /// 原子迁移到 Stopped；先前状态是 Idle 时也允许关停，这样启动尚未完成就退出也能把门闩钉死，
    /// 后面的 <see cref="TryBegin"/> 不会再生效。
    /// </remarks>
    public bool TryStop() => Interlocked.Exchange(ref _state, Stopped) != Stopped;

    /// <summary>门闩是否已经关停。</summary>
    /// <remarks>只读快照，用于 <c>ComposeAsync</c> 在启动返回后判断期间是否已经退出；不改变状态。</remarks>
    public bool IsStopped => Volatile.Read(ref _state) == Stopped;
}
