namespace ImageLyre.App.Startup;

/// <summary>一次进程启动可执行的命令。</summary>
/// <remarks>
/// <para>
/// M1 的 CLI 只有 <see cref="Version"/>、<see cref="Plugins"/> 与 <see cref="Help"/> 三条命令，
/// <see cref="Desktop"/> 表示不带命令参数、走桌面宿主，<see cref="Invalid"/> 表示参数未知、重复或出现多个命令；
/// 业务命令按 <c>ADR-0001</c> §2.5 归 G2，届时在 <see cref="StartupOptions.Parse"/> 的白名单里成组扩展。
/// </para>
/// </remarks>
public enum StartupCommand
{
    /// <summary>不带任何命令参数：启动桌面宿主。</summary>
    Desktop,

    /// <summary>带 <c>--version</c>：输出版本行并退出。</summary>
    Version,

    /// <summary>带 <c>--plugins</c>：启动 Bootstrap 并输出插件清单后退出。</summary>
    Plugins,

    /// <summary>带 <c>--help</c>：输出用法与退出码约定后退出。</summary>
    Help,

    /// <summary>参数未知、重复或出现多个命令：由 CLI 以退出码 2 拒绝。</summary>
    Invalid,
}

/// <summary>一次进程启动的命令行解析结果，只包含命令与原始参数两项。</summary>
/// <remarks>
/// <para>
/// 解析是严格白名单：空参数为 <see cref="StartupCommand.Desktop"/>，唯一一个已知命令参数映射到对应命令，
/// 出现多个命令参数或任意未识别参数一律判 <see cref="StartupCommand.Invalid"/>；不解析 <c>--key=value</c>，
/// 也不接受短选项，因为 M1 的三条命令都是无值开关。
/// </para>
/// <para>
/// T05 曾把 <c>--version</c>、<c>--plugins</c>、<c>--help</c> 与调试用的 <c>--headless</c> 四条开关都粗判为无界面模式；
/// T06 按 <c>DESIGN-0001</c> §3.3 收窄为命令白名单后，<c>--headless</c> 已删除，再传入会落
/// <see cref="StartupCommand.Invalid"/> 并由 CLI 以退出码 2 拒绝。
/// </para>
/// <para>
/// 原始参数保存在 <see cref="RawArguments"/> 里，因为 <see cref="StartupCommand.Invalid"/> 需要在错误信息中回显用户实际输入；
/// 该副本在解析时生成，调用方事后修改传入数组不会影响本实例。
/// </para>
/// </remarks>
public sealed class StartupOptions
{
    private static readonly IReadOnlyDictionary<string, StartupCommand> s_commands =
        new Dictionary<string, StartupCommand>(StringComparer.Ordinal)
        {
            ["--version"] = StartupCommand.Version,
            ["--plugins"] = StartupCommand.Plugins,
            ["--help"] = StartupCommand.Help,
        };

    private readonly string[] _rawArguments;

    private StartupOptions(StartupCommand command, string[] rawArguments)
    {
        Command = command;
        _rawArguments = rawArguments;
    }

    /// <summary>本次启动要执行的命令。</summary>
    /// <remarks>
    /// 为 <see cref="StartupCommand.Desktop"/> 时入口走桌面宿主；其余取值都表示无界面，其中
    /// <see cref="StartupCommand.Invalid"/> 只用于让 CLI 输出参数错误，不会进入任何宿主启动逻辑。
    /// </remarks>
    public StartupCommand Command { get; }

    /// <summary>是否进入无界面分支，等价于 <see cref="Command"/> 不等于 <see cref="StartupCommand.Desktop"/>。</summary>
    /// <remarks>
    /// 为 <see langword="true"/> 时调用方不得构造 <c>AppBuilder</c>、不得创建窗口，也不得触碰任何 Avalonia 类型；
    /// 这条例由 <c>Program.Main</c> 在构造 <c>App</c> 之前分支执行，本类只给出判定结果。
    /// </remarks>
    public bool Headless => Command != StartupCommand.Desktop;

    /// <summary>解析时得到的原始参数副本，顺序与进程启动时一致。</summary>
    /// <remarks>
    /// 该集合只读且与传入数组不共享存储；CLI 在 <see cref="StartupCommand.Invalid"/> 时用它回显原文。
    /// </remarks>
    public IReadOnlyList<string> RawArguments => _rawArguments;

    /// <summary>按命令白名单解析进程参数。</summary>
    /// <param name="args">进程启动参数，允许为空数组。</param>
    /// <returns>解析结果；无法唯一确定一条命令时为 <see cref="StartupCommand.Invalid"/>。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> 为 <see langword="null"/>。</exception>
    /// <remarks>
    /// <para>
    /// 判定顺序只有三步：参数为空直接返回桌面命令；逐个查白名单，任何一个参数未命中即判无效；
    /// 命中后若已经出现过一条命令也判无效，因此同一条命令重复两次与两条不同命令同时出现等价。
    /// </para>
    /// <para>
    /// 无效结果仍保留全部原始参数，不在这里输出任何文本；输出与退出码归 <c>CliHost</c>，
    /// 因为本类型同时服务于桌面入口，写控制台会把桌面启动变成一次隐式的 CLI 调用。
    /// </para>
    /// </remarks>
    public static StartupOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Length == 0)
            return new StartupOptions(StartupCommand.Desktop, []);

        StartupCommand command = StartupCommand.Invalid;
        foreach (string arg in args)
        {
            if (!s_commands.TryGetValue(arg, out StartupCommand candidate) || command != StartupCommand.Invalid)
            {
                command = StartupCommand.Invalid;
                break;
            }

            command = candidate;
        }

        // ponytail: M1 的命令只有三条，业务命令按 ADR-0001 §2.5 归 G2；届时把这里改成命令表并补参数校验，不再追加分支。
        return new StartupOptions(command, [.. args]);
    }
}
