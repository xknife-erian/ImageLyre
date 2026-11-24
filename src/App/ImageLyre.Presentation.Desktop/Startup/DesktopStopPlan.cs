using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace ImageLyre.Presentation.Desktop.Startup;

/// <summary>桌面关停的显式命名序列：关闭壳窗体、停止宿主、释放宿主三个动作，按固定顺序只执行一次。</summary>
/// <remarks>
/// <para>
/// 这是一段命名的关闭序列，不是通用 Plan/Step 容器：三个动作都是关停的一部分，没有取用同一个步骤模型的第二个消费者，
/// 因此用静态方法直接表达顺序，不再包一层组与 Step。日志刷新当前没有独立管线，不在这里造一个空动作，
/// 接入日志管线后再决定是否补第四步；该例外由 T12 记入迁移文档。
/// </para>
/// <para>
/// 线程：<see cref="CloseShell"/> 与 <see cref="ExecuteAsync"/> 必须在 Avalonia UI 线程调用，因为关闭窗口会同步触发
/// <c>Closed</c>、回收平台窗口并退订控件事件；宿主停止的等待不要求 UI 线程，但调用方已在 UI 线程且需要先关窗再停插件，故保持串行。
/// </para>
/// </remarks>
internal static class DesktopStopPlan
{
    /// <summary>关闭壳窗体；窗体为 <see langword="null"/>、已关闭或尚未显示时安全。</summary>
    /// <param name="shell">壳插件产出的主窗体，可为 <see langword="null"/>。</param>
    /// <remarks>
    /// Avalonia 的 <see cref="Window"/> 不实现 <see cref="IDisposable"/>，释放窗体只有关闭一条路；已关闭窗口的
    /// <see cref="Window.PlatformImpl"/> 已被置空，据此跳过重复关闭，避免再次触发 <c>Closed</c> 与平台窗口回收。
    /// 调用方保证此时欢迎窗已经显示并进入主循环，因此关闭壳窗体不会被经典桌面生命周期误判为“最后一个窗口关闭”。
    /// </remarks>
    internal static void CloseShell(Window? shell)
    {
        if (shell is null || shell.PlatformImpl is null)
            return;

        shell.Close();
    }

    /// <summary>按“关壳 → 停宿主 → 释放宿主”执行完整关停。</summary>
    /// <param name="shell">壳窗体，可为 <see langword="null"/>；有值时先关闭。</param>
    /// <param name="host">本次启动的宿主，可为 <see langword="null"/>；有值时停止后释放。</param>
    /// <param name="cancellationToken">超时后取消的令牌，传给宿主停止方法。</param>
    /// <returns>关停完成的任务。</returns>
    /// <remarks>
    /// 先关闭壳再停止插件，避免插件停止后控件仍被访问；宿主停止异常不在本方法内吞掉，由 <c>ExitArbiter</c> 原样交给等待方，
    /// 但 <c>host.Dispose()</c> 放在 <c>finally</c> 里，异常路径也不会漏掉宿主释放。
    /// </remarks>
    internal static async Task ExecuteAsync(Window? shell, IHost? host, CancellationToken cancellationToken)
    {
        CloseShell(shell);

        if (host is null)
            return;

        try
        {
            await host.StopAsync(cancellationToken);
        }
        finally
        {
            host.Dispose();
        }
    }
}
