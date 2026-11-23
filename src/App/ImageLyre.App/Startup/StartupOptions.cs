namespace ImageLyre.App.Startup;

/// <summary>一次进程启动的解析结果，M1 只暴露宿主分支这一项。</summary>
/// <remarks>
/// <para>
/// 解析刻意保持粗粒度：M1 只需要回答“这次启动要不要界面”，具体命令、参数错误与退出码由 T06 的 CLI 承担，
/// 因此这里不做命令白名单、不校验参数组合，也不解析 <c>--key=value</c> 形式。
/// </para>
/// </remarks>
public sealed class StartupOptions
{
    private static readonly string[] s_headlessSwitches = ["--version", "--plugins", "--help", "--headless"];

    private StartupOptions(bool headless) => Headless = headless;

    /// <summary>是否进入无界面分支。</summary>
    /// <remarks>
    /// 为 <see langword="true"/> 时宿主不得构造 <c>AppBuilder</c>、不得创建窗口，也不得触碰任何 Avalonia 类型；
    /// 这条例由 <c>Program.Main</c> 在构造 <c>App</c> 之前分支执行，本类只负责给出判定结果。
    /// </remarks>
    public bool Headless { get; }

    /// <summary>按进程参数判定宿主分支。</summary>
    /// <param name="args">进程启动参数，允许为空数组。</param>
    /// <returns>解析结果。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> 为 <see langword="null"/>。</exception>
    /// <remarks>
    /// 判据是“参数里出现任一已知无界面开关即无界面”，未识别的参数一律留给桌面分支；<c>--headless</c> 是 M1 的显式调试入口，
    /// T06 之后仍保留，其余三个开关届时改为命令白名单并返回真实输出。
    /// </remarks>
    public static StartupOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        // ponytail: T05 的判定只到“参数里出现四条开关之一”，T06 收窄为三条命令的白名单并让未知参数返回 2。
        bool headless = args.Any(static arg => s_headlessSwitches.Contains(arg, StringComparer.Ordinal));
        return new StartupOptions(headless);
    }
}
