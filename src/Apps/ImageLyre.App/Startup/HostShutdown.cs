using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ImageLyre.App.Startup;

/// <summary>宿主收尾：在没有 UI 同步上下文的线程上停止并释放宿主。</summary>
/// <remarks>
///     消息循环返回后，当前线程仍挂着已停摆的派发器；<c>StopAsync</c> 里任何没有
///     <c>ConfigureAwait(false)</c> 的 await 都会把续体排进那个队列，结果是窗口消失、进程留在后台。
///     因此收尾必须切到线程池执行。
/// </remarks>
internal static class HostShutdown
{
    /// <summary>停止并释放宿主。</summary>
    /// <param name="host">已构建的宿主。</param>
    internal static void Stop(IHost host) =>
        Task.Run(async () =>
            {
                await host.StopAsync().ConfigureAwait(false);
                host.Dispose();
            })
            .GetAwaiter()
            .GetResult();
}
