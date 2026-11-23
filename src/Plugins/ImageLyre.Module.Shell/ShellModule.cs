using Cangming.Logs;
using Cangming.PicoPlugins;
using Cangming.PicoPlugins.Core;
using Cangming.PicoPlugins.Core.Features;
using Cangming.PicoPlugins.Core.Modules;
using ImageLyre.Shell.Contracts;
using Microsoft.Extensions.Logging;

namespace ImageLyre.Module.Shell;

/// <summary>M1 的壳插件：以 <see cref="Build"/> 产出主窗体，并在窗口构造期贡献三个一级槽位。</summary>
/// <remarks>
/// <para>
/// 壳是 M1 唯一的 UI 型 Module：它接收槽位管理器注入、贡献槽位、构造 <see cref="ShellWindow"/>，
/// 并实现 <see cref="IWorkbenchShell"/> 让宿主把窗口挂到桌面。它不消费任何模块契约，
/// 因此 <c>DeclareDependencies</c> 保持空声明；槽位管理器走 <see cref="ISupportSlotManager"/> 的独立注入通道，
/// 不写进依赖声明，也不经 <c>Use&lt;T&gt;()</c> 取用。
/// </para>
/// <para>
/// 时序铁律：构造函数只装配上下文，不读任何依赖；<see cref="InjectAsync(Lazy{ISlotManager})"/> 只保存延迟引用；
/// <see cref="Build"/> 的 <see cref="Lazy{T}"/> 工厂在宿主第一次取窗口时执行，槽位贡献与控件构造都发生在那里，
/// 因此宿主必须在 Avalonia 就绪后、在 UI 线程上取壳。
/// </para>
/// <para>
/// cangming 0.1.9.1122 的 <c>BasePicoModule(BaseModuleContext)</c> 是 <c>private protected</c>，跨程序集派生类不可调用，
/// 所以这里先用无参基类构造，再在构造函数里用 <c>UseContext</c> 替换为 <see cref="ShellModuleContext"/>；
/// Mercury 指南中的 <c>base(new XxxModuleContext())</c> 基于 0.1.6，不能直接照抄。
/// </para>
/// </remarks>
[Plugin(id: "lyre.shell.workbench", name: "工作台壳")]
public sealed class ShellModule : BasePicoModule<IWorkbenchShell>, ISupportSlotManager
{
    private static readonly ILogger s_logger = LogHub.For(typeof(ShellModule));

    private readonly ShellModuleContext _context;
    private Lazy<IWorkbenchShell>? _shell;

    /// <summary>创建一个只完成上下文装配的壳插件。</summary>
    /// <remarks>构造函数不读取槽位管理器，也不构造任何控件；这些动作分别属于注入阶段与 <see cref="Build"/>。</remarks>
    public ShellModule() : base()
    {
        _context = new ShellModuleContext();
        UseContext(_context);
    }

    /// <summary>接收宿主注入的槽位管理器延迟引用并记录一行日志。</summary>
    /// <param name="slotManagerLazy">槽位管理器延迟引用，不得为 <see langword="null"/>。</param>
    /// <returns>被注入的插件本身。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slotManagerLazy"/> 为 <see langword="null"/>。</exception>
    /// <remarks>
    /// 实现的是 <see cref="ISupportSlotManager"/> 的注入点，不是模块管理器重载；本类不碰
    /// <c>InjectAsync(Lazy&lt;IModulesManager&gt;)</c>，因此基类写入依赖声明的那条路径不受影响。
    /// 这里只保存引用，不求值，也不构造控件。
    /// </remarks>
    public Task<IPicoPlugin> InjectAsync(Lazy<ISlotManager> slotManagerLazy)
    {
        ArgumentNullException.ThrowIfNull(slotManagerLazy);
        _context.SetSlotManager(slotManagerLazy);
        s_logger.LogInformation("槽位管理器已注入壳。");

        return Task.FromResult<IPicoPlugin>(this);
    }

    /// <summary>在首次取壳时构造并缓存主窗体。</summary>
    /// <param name="args">构建参数，M1 不使用；壳没有按参数产出不同窗体的场景。</param>
    /// <returns>缓存同一个窗口实例的 <see cref="Lazy{T}"/>。</returns>
    /// <remarks>
    /// 窗口构造会贡献三个槽位并创建 <c>SlotPanel</c>，因此调用方必须在 UI 线程取 <c>.Value</c>；
    /// <see cref="Lazy{T}"/> 与字段双重缓存保证重复取壳只构造一次，不会触发同名槽位重复贡献。
    /// </remarks>
    public override Lazy<IWorkbenchShell> Build(params object[] args) =>
        _shell ??= new Lazy<IWorkbenchShell>(() => new ShellWindow(_context));

    /// <summary>壳没有模块依赖：窗口只使用注入的槽位管理器。</summary>
    /// <param name="modules">依赖声明构建器，本方法不使用。</param>
    /// <remarks>方法体保持空声明，不产生副作用；依赖展示与启动顺序都不需要壳参与。</remarks>
    protected override void DeclareDependencies(IModuleDeclarationBuilder modules)
    {
        // 壳不消费任何模块契约：槽位管理器走 ISupportSlotManager 注入，不走依赖声明。
        // 本方法会被读依赖与读顺序各调用一次，因此这里不能写任何副作用。
    }

    /// <summary>启动时只记录一行日志，不构造任何控件。</summary>
    /// <returns>恒为 <see langword="true"/>。</returns>
    /// <remarks>
    /// <c>OnStartAsync</c> 早于 Avalonia 就绪，构建窗口必须留到 <see cref="Build"/>；
    /// 这里只验证插件状态机能走到 Running，供 <c>--plugins</c> 清单判读。
    /// </remarks>
    protected override Task<bool> OnStartAsync()
    {
        s_logger.LogInformation("壳插件已启动，主窗体将在 Build 时构造。");
        return Task.FromResult(true);
    }
}
