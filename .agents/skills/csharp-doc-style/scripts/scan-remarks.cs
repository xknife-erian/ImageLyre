#!/usr/bin/env dotnet
// scan-remarks.cs —— 注释段落超标自检（游刃 csharp-doc-style 技能自带工具）。
//
// 用途：统计 XML Doc 注释中「单段说明文字超过阈值行数」的块，供注释结构整改任务的自检与复核使用。
// 判据出自本技能目录下的 SKILL.md 第 3 节；脚本与 SKILL.md 同处一个技能目录，调用时以 skill_view
// 返回的 skill_dir 为基准拼接路径，勿写死仓库落地约定。
//
// 本文件是 .NET 10 单文件应用：首行 shebang 使其可直接执行，也可 dotnet run scan-remarks.cs -- 调用；
// 需要机器上有 .NET 10 SDK（现场编译，产物落在 ~/.local/share/dotnet/runfile/，不进被扫描的仓库）。
// 注意：dotnet 驱动层会截走 --help/-h，直接执行时取帮助请用 help 或 -?。
//
// 用法：
//     scripts/scan-remarks.cs [--max 4] [目录 ...]
//     不带目录时默认扫描 src tests testbed drivers（在仓库根运行）。
//     --self-test 运行内置回归自检（在临时目录内进行，不接触任何真实仓库）。
//
// 关键判据说明：只统计「说明性正文行」。`<summary>`、`<remarks>`、`<param>`、`<returns>`、
// `<exception>`、`<para>`、`<list>`、`<item>`、`<description>`、`<see>` 等结构标签行各自成一体，
// 不计入段落行数；把它们计进去会把正常文档误报为超标，这是本工具存在的主要理由。
//
// 退出码：0 = 无超标；1 = 存在超标；2 = 参数错误。
using System.Text;
using System.Text.RegularExpressions;

return new ScanRemarks().Run(args);

internal sealed class ScanRemarks
{
    /// <summary>结构标签行：这些行本身不是说明性正文，出现即视为段落边界。</summary>
    static readonly Regex Struct = new(
        @"^</?(summary|remarks|para|list|item|description|param|typeparam|returns|exception"
        + @"|inheritdoc|code|c|see|seealso|listheader|term|value|example|paramref|typeparamref)\b",
        RegexOptions.Compiled);

    static readonly string[] DefaultRoots = { "src", "tests", "testbed", "drivers" };

    const string UsageText = """
        scan-remarks.cs —— 统计 XML Doc 注释中超标段落。

        用法（Linux / macOS / WSL，已加执行位）：
          scripts/scan-remarks.cs [--max 4] [目录 ...]
        用法（Windows 或未加执行位时）：
          dotnet run scripts/scan-remarks.cs -- [--max 4] [目录 ...]

        不带目录时默认扫描 src tests testbed drivers（在仓库根运行）。
        退出码：0 = 无超标；1 = 存在超标；2 = 参数错误。
        注意：直接执行时 --help/-h 会被 dotnet 驱动层截走，取帮助请用 help 或 -?。

        """;

    int _max = 4;
    bool _selfTest;

    public int Run(string[] args)
    {
        var roots = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--max":
                    i++;
                    if (i >= args.Length || !int.TryParse(args[i], out _max) || _max < 1)
                    {
                        Console.Error.WriteLine("--max 需要一个正整数。");
                        Console.Error.Write(UsageText);
                        return 2;
                    }
                    break;
                case "--self-test": _selfTest = true; break;
                case "help" or "-?" or "-h" or "--help": Console.Out.Write(UsageText); return 0;
                default:
                    if (args[i].StartsWith('-'))
                    {
                        Console.Error.WriteLine($"未知参数：{args[i]}");
                        Console.Error.Write(UsageText);
                        return 2;
                    }
                    roots.Add(args[i]);
                    break;
            }
        }

        if (_selfTest) return SelfTest();

        if (roots.Count == 0) roots.AddRange(DefaultRoots);
        var total = 0;
        foreach (var root in roots)
        {
            var hits = Scan(root);
            total += hits.Count;
            Console.WriteLine($"--- {root}: {hits.Count} 处");
            foreach (var (path, start, run) in hits)
                Console.WriteLine($"    {path}:{start}  连续 {run} 行正文");
        }
        Console.WriteLine($"合计 {total} 处（判据：单段正文超过 {_max} 行）");
        return total == 0 ? 0 : 1;
    }

    /// <summary>返回 root 目录下超标块列表，每项为 (显示路径, 起始行号, 连续正文行数)。</summary>
    List<(string Path, int Start, int Run)> Scan(string root)
    {
        var hits = new List<(string, int, int)>();
        if (!Directory.Exists(root)) return hits;

        var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Select(f => (Full: f, Display: Display(root, f)))
            .OrderBy(x => x.Display, PathPartsComparer.Instance);

        foreach (var (full, display) in files)
        {
            // 与旧版同一口径：显示路径中含 /bin/ 或 /obj/ 的一律跳过
            if (display.Contains("/bin/") || display.Contains("/obj/")) continue;

            var lines = SplitLines(File.ReadAllText(full, Encoding.UTF8));
            var run = 0;
            var start = 0;
            for (var index = 0; index < lines.Count; index++)
            {
                var stripped = lines[index].Trim();
                if (!stripped.StartsWith("///", StringComparison.Ordinal))
                {
                    run = 0;
                    continue;
                }
                var body = stripped[3..].Trim();
                if (body.Length == 0 || Struct.IsMatch(body))
                {
                    run = 0;
                    continue;
                }
                run++;
                if (run == 1) start = index + 1;
                if (run > _max)
                {
                    hits.Add((display, start, run));
                    run = 0;
                }
            }
        }
        return hits;
    }

    /// <summary>与 Python 版一致：root 为 "." 时不带前缀，其余按 root/相对路径 拼接。</summary>
    static string Display(string root, string full)
    {
        var rel = Path.GetRelativePath(root, full).Replace('\\', '/');
        if (root == "." || root == "./" || root == "") return rel;
        return root.Replace('\\', '/').TrimEnd('/') + "/" + rel;
    }

    static List<string> SplitLines(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();

    /// <summary>按路径分段比较，与 Python 的 Path 排序口径一致。</summary>
    internal sealed class PathPartsComparer : IComparer<string>
    {
        public static readonly PathPartsComparer Instance = new();
        public int Compare(string? a, string? b)
        {
            var pa = (a ?? "").Split('/');
            var pb = (b ?? "").Split('/');
            for (var i = 0; i < Math.Min(pa.Length, pb.Length); i++)
            {
                var c = string.CompareOrdinal(pa[i], pb[i]);
                if (c != 0) return c;
            }
            return pa.Length.CompareTo(pb.Length);
        }
    }

    // ───────────────────────── 内置回归自检 ─────────────────────────

    int SelfTest()
    {
        var pass = 0;
        var fail = 0;
        var root = Path.Combine(Path.GetTempPath(), "youren-scanrem-selftest-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);

        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { pass++; Console.WriteLine($"  PASS  {name}"); }
            else { fail++; Console.WriteLine($"  FAIL  {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
        }
        (int Code, string Output) RunTool(params string[] argv)
        {
            var prevOut = Console.Out;
            var prevErr = Console.Error;
            var buffer = new StringWriter();
            try
            {
                Console.SetOut(buffer);
                Console.SetError(buffer);
                return (new ScanRemarks().Run(argv), buffer.ToString());
            }
            finally
            {
                Console.SetOut(prevOut);
                Console.SetError(prevErr);
            }
        }

        try
        {
            // fixture：结构标签不计入、5 行正文超标、bin 跳过、恰好 4 行不超标、空 /// 与标签复位
            Directory.CreateDirectory(Path.Combine(root, "src", "bin"));
            Directory.CreateDirectory(Path.Combine(root, "src", "obj"));
            Directory.CreateDirectory(Path.Combine(root, "tests"));
            File.WriteAllText(Path.Combine(root, "src", "Bad.cs"),
                "public class Bad\n{\n    /// <summary>短说明</summary>\n"
                + "    /// 正文第一行，说明这个方法的用途与约束，\n"
                + "    /// 继续补充第二行，讲清楚参数关系与调用顺序，\n"
                + "    /// 第三行讲边界情况，例如空集合与超时，\n"
                + "    /// 第四行讲错误处理，说明异常会如何向上抛，\n"
                + "    /// 第五行讲线程安全，说明是否可并发调用。\n"
                + "    /// <para>结构标签行不计入</para>\n"
                + "    /// 尾部一行正文\n    public void M() { }\n}\n");
            File.WriteAllText(Path.Combine(root, "src", "Ok4.cs"),
                "/// <summary>刚好四行</summary>\n/// 一\n/// 二\n/// 三\n/// 四\n/// <param name=\"x\">参数</param>\npublic class Ok4 { }\n");
            File.WriteAllText(Path.Combine(root, "src", "EmptyResets.cs"),
                "/// 一\n/// 二\n///\n/// 三\n/// 四\n/// <returns>结果</returns>\n/// 五\npublic class EmptyResets { }\n");
            File.WriteAllText(Path.Combine(root, "src", "bin", "Skipped.cs"),
                "/// 一\n/// 二\n/// 三\n/// 四\n/// 五\n/// 六\n");
            File.WriteAllText(Path.Combine(root, "src", "obj", "Skipped.cs"),
                "/// 一\n/// 二\n/// 三\n/// 四\n/// 五\n/// 六\n");
            File.WriteAllText(Path.Combine(root, "notes.txt"), "/// 一\n/// 二\n/// 三\n/// 四\n/// 五\n/// 六\n");

            var (code, output) = RunTool("--max", "4", root);
            Check("有超标时退出码 1", code == 1, $"exit={code}");
            Check("只报 Bad.cs 一处", output.Contains("--- " + root + ": 1 处"), output.Split('\n')[0]);
            Check("命中行号与连续行数正确",
                Regex.IsMatch(output, @"src/Bad\.cs:4  连续 5 行正文"), output);
            Check("结构标签行不计入（<para> 后不影响计数）", !output.Contains("连续 6 行正文"));
            Check("bin/obj 下的文件被跳过", !output.Contains("Skipped.cs"));
            Check("非 .cs 文件不扫描", !output.Contains("notes.txt"));
            Check("合计行含判据行数", output.Contains("合计 1 处（判据：单段正文超过 4 行）"));

            var (code2, output2) = RunTool("--max", "2", root);
            // 判据是「超过 max 行即触发并复位」，因此 max=2 时命中 3 行处、每个块只报一次
            Check("--max 2 时 Ok4 也超标", code2 == 1 && output2.Contains("Ok4.cs:2  连续 3 行正文"), output2);
            Check("--max 2 时合计 2 处", output2.Contains("合计 2 处（判据：单段正文超过 2 行）"), output2.Split('\n')[^2]);

            var (code3, output3) = RunTool("--max", "99", root);
            Check("--max 99 时无超标且退出码 0", code3 == 0 && output3.Contains("合计 0 处"), $"exit={code3}");

            var (code4, output4) = RunTool(Path.Combine(root, "不存在的目录"));
            Check("目录不存在按 0 处处理且退 0", code4 == 0 && output4.Contains(": 0 处"), $"exit={code4}");

            // 缺省四棵树是相对工作目录的，断言必须切到 fixture 根再跑，否则结果随调用目录漂移
            var prevCwd = Environment.CurrentDirectory;
            int code5;
            string output5;
            try
            {
                Environment.CurrentDirectory = root;
                (code5, output5) = RunTool();
            }
            finally
            {
                Environment.CurrentDirectory = prevCwd;
            }
            Check("缺省四棵树（含缺失目录不报错）", code5 == 1 && output5.Contains("--- testbed: 0 处"), $"exit={code5}");

            var (code6, _) = RunTool("--max", "abc", root);
            Check("--max 非整数退 2", code6 == 2, $"exit={code6}");
            var (code7, _) = RunTool("--bogus");
            Check("未知参数退 2", code7 == 2, $"exit={code7}");
            var (code8, output8) = RunTool("help");
            Check("help 退 0 且含用法", code8 == 0 && output8.Contains("--max 4"), $"exit={code8}");
        }
        finally
        {
            try { Directory.Delete(root, true); } catch (IOException) { /* 清理失败不影响结论 */ }
        }

        Console.WriteLine();
        Console.WriteLine($"自检：{pass} 项通过，{fail} 项失败。");
        return fail == 0 ? 0 : 1;
    }
}
