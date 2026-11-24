using Avalonia.Controls;
using ImageLyre.Bootstrap.Startup;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>桌面关停的收口：用 <see cref="ExitArbiter"/> 保证开始与关停各一次，关停先释放壳窗体、再停止宿主并可等待。</summary>
/// <remarks>
/// <para>
/// 状态与“只关停一次、停止可先于开始、返回同一个任务”的语义由 <see cref="ExitArbiter"/> 提供；本类只持有本次启动的宿主与
/// 壳窗体，并在关闭动作里按“释放壳窗体 → 停止宿主 → 释放宿主”的固定顺序执行，避免插件停止后控件仍被访问。
/// </para>
/// <para>
/// 登记与关停的互斥用一把锁：<see cref="AttachHost"/> 与 <see cref="AttachShell"/> 在锁内检查关停状态，关闭动作在锁内取走引用，
/// 因此关停先到时登记返回 false 由调用方自行释放，登记先到时关闭动作一定能取到引用，不会漏停。
/// 宿主停止失败只写调试输出，不反向阻断退出；超时与超时后失败由 <see cref="ExitArbiter"/> 记录日志。
/// </para>
/// </remarks>
internal sealed class DesktopShutdown
{
    private static readonly TimeSpan s_timeout = TimeSpan.FromSeconds(30);

    private readonly ExitArbiter _exit;
    private readonly Lock _gate = new();

    private IHost? _host;
    private Window? _shell;

    /// <summary>创建桌面关停收口。</summary>
    /// <param name="loggerFactory">日志工厂，为 <see cref="ExitArbiter"/> 提供超时与超时后失败的日志出口。</param>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory"/> 为 <see langword="null"/>。</exception>
    internal DesktopShutdown(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _exit = new ExitArbiter(StopCoreAsync, s_timeout, loggerFactory.CreateLogger<ExitArbiter>());
    }

    /// <summary>尝试开始一次桌面启动；只有在尚未开始、尚未关停时返回 <see langword="true"/>。</summary>
    /// <returns>首次调用返回 <see langword="true"/>；已经关停或已经开始时返回 <see langword="false"/>。</returns>
    /// <remarks>启动失败与关停都已发生后再开始被拒绝，从而不会出现“窗口已退出、插件却又被启动”的半启动状态。</remarks>
    internal bool TryBegin() => _exit.TryBegin();

    /// <summary>本次启动是否已经进入关停。</summary>
    /// <remarks>只读快照；调用方在宿主启动等异步等待之后用它判断期间桌面是否已经退出。</remarks>
    internal bool IsStopped => _exit.IsStopped;

    /// <summary>执行或等待唯一一次关停；重复调用与并发调用返回同一个任务。</summary>
    /// <returns>关停完成的任务；超时为 <see cref="TimeoutException"/>，其余宿主停止异常按本类策略收敛为调试输出。</returns>
    internal Task StopAsync() => _exit.StopAsync();

    /// <summary>登记本次启动的宿主，让关闭动作负责停止它。</summary>
    /// <param name="host">已构建但可能尚未启动的宿主。</param>
    /// <returns>登记成功返回 <see langword="true"/>；已经关停时返回 <see langword="false"/>，调用方必须自行释放宿主。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="host"/> 为 <see langword="null"/>。</exception>
    /// <remarks>登记必须在 <c>StartAsync</c> 之前完成，否则启动期间到达的退出信号会找不到宿主可停。</remarks>
    internal bool AttachHost(IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        lock (_gate)
        {
            if (_exit.IsStopped)
                return false;

            _host = host;
            return true;
        }
    }

    /// <summary>登记主窗体，让关闭动作在停止宿主之前先释放它。</summary>
    /// <param name="shell">壳插件产出的主窗体，关停时若实现 <see cref="IDisposable"/> 则被释放。</param>
    /// <returns>登记成功返回 <see langword="true"/>；已经关停时返回 <see langword="false"/>，调用方必须自行释放窗体。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="shell"/> 为 <see langword="null"/>。</exception>
    internal bool AttachShell(Window shell)
    {
        ArgumentNullException.ThrowIfNull(shell);

        lock (_gate)
        {
            if (_exit.IsStopped)
                return false;

            _shell = shell;
            return true;
        }
    }

    /// <summary>关闭动作：先释放壳窗体，再停止并释放宿主；只由 <see cref="ExitArbiter"/> 执行一次。</summary>
    /// <param name="cancellationToken">超时后取消的令牌，传给宿主停止方法。</param>
    /// <returns>关停完成的任务。</returns>
    private async Task StopCoreAsync(CancellationToken cancellationToken)
    {
        Window? shell;
        IHost? host;

        lock (_gate)
        {
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
            await host.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // ponytail: 关停失败没有日志出口，WinExe 下 stderr 不可见；关停异常不反向阻断退出，日志管线接入后改记日志。
            System.Diagnostics.Debug.WriteLine($"宿主关停失败：{ex}");
        }
        finally
        {
            host.Dispose();
        }
    }
}
