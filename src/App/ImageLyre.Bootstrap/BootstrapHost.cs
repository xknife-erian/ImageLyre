using Cangming.PicoPlugins.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Bootstrap;

/// <summary>一次插件启动的结果：已启动的 Generic Host、服务容器与解析出的插件管理器。</summary>
/// <remarks>
/// <para>
/// 实例由 <see cref="StartAsync"/> 产出，调用方在进程或窗口结束时调用 <see cref="DisposeAsync"/> 停止宿主；
/// 本类不创建窗口、不持有线程，启动失败时异常直接交给宿主显示或记录。
/// </para>
/// <para>
/// <see cref="Services"/> 与 <see cref="PluginManager"/> 在整个宿主生命周期内不变；管理器是容器内的单例，
/// 因此外部按接口再次解析得到的也是同一个实例。HostPlan 的失败归属与报告写入由宿主的启动流程完成。
/// </para>
/// </remarks>
public sealed class BootstrapHost : IAsyncDisposable
{
    private readonly IHost _host;
    private readonly IPluginManager _pluginManager;

    private BootstrapHost(IHost host, IPluginManager pluginManager)
    {
        _host = host;
        _pluginManager = pluginManager;
    }

    /// <summary>宿主启动并已通知 cangming 的服务容器。</summary>
    /// <remarks>容器由宿主持有，调用方不应自行释放；<see cref="DisposeAsync"/> 负责停止并释放宿主。</remarks>
    public IServiceProvider Services => _host.Services;

    /// <summary>本次启动解析出的插件管理器，与 <see cref="Services"/> 中的实例相同。</summary>
    public IPluginManager PluginManager => _pluginManager;

    /// <summary>构建并启动 Generic Host，返回可关停的兼容宿主。</summary>
    /// <param name="loggerFactory">日志工厂，用于初始化 <c>LogHub</c> 并注册进容器供插件管理器构造使用。</param>
    /// <param name="mode">宿主模式，只决定扫描期是否保留 Feature。</param>
    /// <param name="configure">宿主追加的服务注册；为空时不注册额外服务。</param>
    /// <param name="cancellationToken">取消令牌，只取消宿主启动的等待。</param>
    /// <returns>已完成启动的宿主。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="loggerFactory"/> 为 <see langword="null"/>。</exception>
    /// <exception cref="OperationCanceledException">宿主启动等待被 <paramref name="cancellationToken"/> 取消。</exception>
    /// <remarks>
    /// 兼容入口：App 与 CLI 在 T08 之前仍调用本方法，内部改走 <see cref="BootstrapHostBuilder"/> 与 HostPlan，
    /// 但返回值、<see cref="Services"/> 与 <see cref="PluginManager"/> 的语义保持不变。启动失败时宿主被释放，异常原样抛出。
    /// </remarks>
    public static async Task<BootstrapHost> StartAsync(
        ILoggerFactory loggerFactory,
        HostMode mode,
        Action<IServiceCollection>? configure,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        IHost host = BootstrapHostBuilder.Build(loggerFactory, mode, configure, args: []);
        try
        {
            await host.StartAsync(cancellationToken);
        }
        catch
        {
            // ponytail: 启动失败时只释放宿主；插件回滚与完整关停由 T09 的 StopPlan 接管。
            host.Dispose();
            throw;
        }

        return new BootstrapHost(host, host.Services.GetRequiredService<IPluginManager>());
    }

    /// <summary>停止宿主并释放容器；宿主关停会停止全部插件。</summary>
    /// <returns>停止并释放完成的任务。</returns>
    /// <remarks>调用方应在主窗体关闭或进程退出前调用一次；停止抛出的异常仍会先执行释放再向上传播。</remarks>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await _host.StopAsync();
        }
        finally
        {
            _host.Dispose();
        }
    }
}
