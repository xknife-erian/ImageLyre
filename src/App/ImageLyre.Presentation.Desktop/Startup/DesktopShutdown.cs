using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>桌面启动的关停收口：门闩保证开始与关停各一次，关停先释放壳窗体、再停止宿主。</summary>
/// <remarks>
/// <para>
/// 状态只有三个：Idle → Begun → Stopped；<see cref="TryBegin"/> 只在 Idle 时成功，<see cref="StopAsync"/> 从任意状态迁移到 Stopped，
/// 因此 <c>desktop.Exit</c>、主窗体 <c>Closed</c> 与启动失败三条路径调用同一个关停时只有第一条真正执行。
/// 所有状态与两个引用都在 <c>_gate</c> 内读写，锁外只做释放与等待。
/// </para>
/// <para>
/// 关停顺序固定为“释放壳窗体 → 停止宿主”，与迁移前的 <c>StopAsync</c> 一致：先退订槽位与本地化事件，再停插件，
/// 避免插件停止后控件仍被访问。宿主停止失败只写调试输出，不反向阻断进程退出，因为关停不是用户可以修复的失败。
/// </para>
/// <para>
/// 本类是 T09 <c>ExitArbiter</c> 与桌面 StopPlan 的临时前身，只覆盖“不重复关停”与“先壳后宿主”两条语义；
/// 完整 await、超时与停止先于开始的并发用例在 T09 补齐，届时本类整体替换而不是叠加第二套关停。
/// </para>
/// </remarks>
internal sealed class DesktopShutdown
{
    private const int Idle = 0;
    private const int Begun = 1;
    private const int Stopped = 2;

    private readonly Lock _gate = new();

    private int _state;
    private IHost? _host;
    private Window? _shell;

    /// <summary>尝试开始一次桌面启动；只有在尚未开始、尚未关停时返回 <see langword="true"/>。</summary>
    /// <returns>首次调用返回 <see langword="true"/>；已经关停或已经开始时返回 <see langword="false"/>。</returns>
    /// <remarks>启动失败与关停都已发生后再开始被拒绝，从而不会出现“窗口已退出、插件却又被启动”的半启动状态。</remarks>
    internal bool TryBegin()
    {
        lock (_gate)
        {
            if (_state != Idle)
                return false;

            _state = Begun;
            return true;
        }
    }

    /// <summary>本次启动是否已经进入关停。</summary>
    /// <remarks>只读快照；调用方在宿主启动等异步等待之后用它判断期间桌面是否已经退出。</remarks>
    internal bool IsStopped
    {
        get
        {
            lock (_gate)
                return _state == Stopped;
        }
    }

    /// <summary>登记本次启动的宿主，让关停路径负责停止它。</summary>
    /// <param name="host">已构建但可能尚未启动的宿主。</param>
    /// <returns>登记成功返回 <see langword="true"/>；已经关停时返回 <see langword="false"/>，调用方必须自行释放宿主。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="host"/> 为 <see langword="null"/>。</exception>
    /// <remarks>登记必须在 <c>StartAsync</c> 之前完成，否则启动期间到达的退出信号会找不到宿主可停。</remarks>
    internal bool AttachHost(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        lock (_gate)
        {
            if (_state == Stopped)
                return false;

            _host = host;
            return true;
        }
    }

    /// <summary>登记主窗体，让关停路径在停止宿主之前先释放它。</summary>
    /// <param name="shell">壳插件产出的主窗体，关停时若实现 <see cref="IDisposable"/> 则被释放。</param>
    /// <returns>登记成功返回 <see langword="true"/>；已经关停时返回 <see langword="false"/>，调用方必须自行释放窗体。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="shell"/> 为 <see langword="null"/>。</exception>
    internal bool AttachShell(Window shell)
    {
        ArgumentNullException.ThrowIfNull(shell);

        lock (_gate)
        {
            if (_state == Stopped)
                return false;

            _shell = shell;
            return true;
        }
    }

    /// <summary>执行一次关停：先释放壳窗体，再停止并释放宿主；重复调用直接返回。</summary>
    /// <returns>关停完成的任务；宿主停止异常在内部收敛，不向调用方传播。</returns>
    internal async Task StopAsync()
    {
        Window? shell;
        IHost? host;

        lock (_gate)
        {
            if (_state == Stopped)
                return;

            _state = Stopped;
            shell = _shell;
            host = _host;
            _shell = null;
            _host = null;
        }

        if (shell is IDisposable disposable)
            disposable.Dispose();

        if (host is null)
            return;

        try
        {
            await host.StopAsync();
        }
        catch (Exception ex)
        {
            // ponytail: 关停失败没有日志出口，WinExe 下 stderr 不可见；日志管线接入后改记日志。
            System.Diagnostics.Debug.WriteLine($"宿主关停失败：{ex}");
        }
        finally
        {
            host.Dispose();
        }
    }
}
