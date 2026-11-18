#!/usr/bin/env dotnet
// init-docs.cs —— 在项目根生成、补全或体检 docs 目录体系（游刃 docs-scaffold 技能自带工具）。
//
// 本文件是 .NET 10 单文件应用：首行 shebang 使其可直接执行（./init-docs.cs --target ...），
// 也可用 dotnet run init-docs.cs -- --target ... 调用。需要机器上有 .NET 10 SDK，因为它要现场
// 编译；编译产物落在 ~/.local/share/dotnet/runfile/ 之下，不会写进被操作的项目。
//
// 两种用法：不带 --audit 时生成／补全（幂等，已存在的文件默认跳过、不覆盖）；带 --audit 时只读
// 体检，列出缺失目录、位置与命名不合规的文档、与本模板的规则差异，并给出 git mv 建议（仅打印）。
// 模板位于本脚本所在技能目录的 templates/ 之下，模板中的 {{用户名}}、{{项目名}}、{{日期}}
// 三处占位符在文件名与正文中一并替换。
//
// 用法与参数见 --help；退出码：0 = 完成／体检完成；1 = 参数或环境错误；2 = 落盘后校验失败。
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

return new InitDocs().Run(args);

internal sealed class InitDocs
{
    static readonly string[] ProcessTypes = { "adr", "analyses", "daily", "design", "plans" };
    // 分析类目录的旧名 analysis/ 仍受支持：体检与命名校验按同一体裁放行，迁移建议沿用项目已有写法，
    // 只在两种写法同时存在时提示保留 analyses/ 并合并另一处；新建一律铺 analyses/。
    static readonly string[] AliasTypes = { "analysis" };
    static readonly string[] DeliveryTypes = { "spec", "guide", "reference" };
    static readonly string[] AllTypes = ProcessTypes.Concat(DeliveryTypes).ToArray();
    static readonly string[] RequiredSections =
        { "目录一览", "个人目录规则", "文档流转", "必备章节", "失效与存档", "书写规范", "示例索引" };
    static readonly string[] SuggestedSections = { "项目自有目录登记表", "附件与二进制", "子系统文档目录登记" };
    static readonly string[] BinaryDocExts = { ".docx", ".doc", ".pdf", ".xlsx", ".xls", ".pptx", ".ppt" };

    const string UsageText = """
        init-docs.cs —— 在项目根生成、补全或体检 docs 目录体系。

        用法（Linux / macOS / WSL，已加执行位）：
          scripts/init-docs.cs --target <项目根> [--audit] [选项]
        用法（Windows 或未加执行位时）：
          dotnet run scripts/init-docs.cs -- --target <项目根> [--audit] [选项]

        注意：dotnet 驱动层会截走它自己认识的选项，直接执行时 --help/-h 会打印 .NET 的帮助而
        不是本工具的说明，--project 会报 “Cannot use the --project and --file options together”。
        因此取帮助请用 help 或 -?，传项目名请用 --proj；或统一在参数前加 “--” 分隔符。

          --target DIR        目标项目根目录（必填，不是 docs/ 自身）
          --audit             只读体检：列出缺失目录、位置与命名不合规的文档、与本模板的规则差异，
                              并给出 git mv 建议（仅打印，不落盘），用于已有项目的补全与升级
          --user NAME         用户名，须与 Git 账号一致；缺省取目标仓库的 git config user.name
          --proj NAME         项目名，用于规则标题与示例正文；缺省取目标目录名（别名 --project）
          --date YYYY-MM-DD   示例文件名与正文中的日期；缺省当天
          --with-examples     铺七份示例（默认行为）
          --no-examples       只铺空目录与规则 README，空目录放 .gitkeep
          --force             覆盖已存在的文件（默认跳过）
          --dry-run           只报告将要做什么，不落盘
          --self-test         运行内置回归自检（在临时目录内进行，不接触任何真实项目）
          help, -?, -h, --help
                              显示本说明（直接执行时请用 help 或 -?，见上方注意）

        类型目录共 8 个：过程类 adr/analyses/daily/design/plans（按 <用户名>/ 分目录）、交付类
        spec/guide/reference（平铺，不按人分）。分析类目录的旧名 analysis/ 仍受支持，体检一并放行；
        同一项目两种写法不要并存。项目自有的额外目录在 docs/README.md 的「项目自有
        目录登记表」中登记后即视为合规，体检不再判为位置不合规。

        """;

    readonly string _skillDir;
    readonly string _tplDir;

    string _target = "", _userName = "", _project = "", _dateStr = "";
    bool _withExamples = true, _force, _dry, _audit, _selfTest;
    string _docs = "";

    // 体检计数
    int _gaps, _misplaced, _namingBad, _nonstd, _users, _readmeDiff, _subdoc;
    // 生成计数
    int _created, _skipped, _planned;

    public InitDocs()
    {
        var scriptDir = AppContext.GetData("EntryPointFileDirectoryPath") as string
                        ?? Path.GetDirectoryName(Environment.ProcessPath) ?? ".";
        _skillDir = Path.GetFullPath(Path.Combine(scriptDir, ".."));
        _tplDir = Path.Combine(_skillDir, "templates");
    }

    public int Run(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--target": _target = Value(args, ref i); break;
                case "--user": _userName = Value(args, ref i); break;
                case "--proj":
                case "--project": _project = Value(args, ref i); break;   // --project 与 dotnet 自带选项同名，直接执行时用 --proj
                case "--date": _dateStr = Value(args, ref i); break;
                case "--with-examples": _withExamples = true; break;
                case "--no-examples": _withExamples = false; break;
                case "--force": _force = true; break;
                case "--dry-run": _dry = true; break;
                case "--audit": _audit = true; break;
                case "--self-test": _selfTest = true; break;
                case "-h" or "--help" or "-?" or "help": Console.Out.Write(UsageText); return 0;
                default:
                    Console.Error.WriteLine($"未知参数：{args[i]}");
                    Console.Error.WriteLine();
                    Console.Error.Write(UsageText);
                    return 1;
            }
        }

        if (_selfTest) return SelfTest();

        if (_target.Length == 0)
        {
            Console.Error.WriteLine("缺少必填参数 --target <项目根>。");
            Console.Error.WriteLine();
            Console.Error.Write(UsageText);
            return 1;
        }
        if (!Directory.Exists(_target))
        {
            Console.Error.WriteLine($"目标目录不存在：{_target}");
            return 1;
        }
        if (!Directory.Exists(_tplDir))
        {
            Console.Error.WriteLine($"模板目录不存在：{_tplDir}（脚本须位于技能目录的 scripts/ 之下）");
            return 1;
        }

        _target = Path.GetFullPath(_target);
        _docs = Path.Combine(_target, "docs");
        if (_project.Length == 0) _project = Path.GetFileName(_target.TrimEnd('/'));
        if (_userName.Length == 0) _userName = GitUserName(_target);
        if (_dateStr.Length == 0) _dateStr = DateTime.Now.ToString("yyyy-MM-dd");

        return _audit ? Audit() : Generate();
    }

    static string Value(string[] args, ref int i)
    {
        i++;
        return i < args.Length ? args[i] : "";
    }

    // ───────────────────────── 替换与判据 ─────────────────────────

    string Subst(string text) => text
        .Replace("{{用户名}}", _userName)
        .Replace("{{项目名}}", _project)
        .Replace("{{日期}}", _dateStr);

    static bool IsType(string name) => AllTypes.Contains(name) || AliasTypes.Contains(name);
    static bool IsProcessType(string name) => ProcessTypes.Contains(name) || AliasTypes.Contains(name);

    /// <summary>目录自述 README.md 不受命名约定约束；reference/ 承接外部原件，一律豁免。</summary>
    static bool NamingOk(string type, string file)
    {
        if (file == "README.md") return true;
        return type switch
        {
            "adr" => Regex.IsMatch(file, @"^ADR-[0-9]{4}-.+\.md$"),
            "design" => Regex.IsMatch(file, @"^DESIGN-[0-9]{4}-.+\.md$"),
            "plans" => Regex.IsMatch(file, @"^PLAN-[0-9]{4}-.+\.md$"),
            "daily" => Regex.IsMatch(file, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}(-summary)?(_[^/]+)?\.md$"),
            "analysis" or "analyses" => Regex.IsMatch(file, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}_.+\.md$"),
            "guide" => Regex.IsMatch(file, @"^[0-9]{2}-.+\.md$"),
            "spec" => Regex.IsMatch(file, @"^.+\.(md|ya?ml|json|toml|xml)$"),
            _ => true
        };
    }

    /// <summary>依据文件名推断应放位置（相对 docs），推断不出返回空串。</summary>
    string SuggestTarget(string file)
    {
        var u = _userName.Length > 0 ? _userName : "<用户名>";
        if (Regex.IsMatch(file, @"^ADR-.*\.md$")) return $"adr/{u}/{file}";
        if (Regex.IsMatch(file, @"^DESIGN-.*\.md$")) return $"design/{u}/{file}";
        if (Regex.IsMatch(file, @"^PLAN-.*\.md$")) return $"plans/{u}/{file}";
        if (Regex.IsMatch(file, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}-summary\.md$")) return $"daily/{u}/{file}";
        if (Regex.IsMatch(file, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}_.*\.md$"))
        {
            // 迁移建议跟随项目既有写法：已有旧名 analysis/ 就沿用，否则用新名 analyses/
            var analysisDir = Directory.Exists(Path.Combine(_docs, "analysis")) && !Directory.Exists(Path.Combine(_docs, "analyses"))
                ? "analysis" : "analyses";
            return $"{analysisDir}/{u}/{file}";
        }
        if (BinaryDocExts.Contains(Path.GetExtension(file).ToLowerInvariant())) return $"reference/{file}";
        return "";
    }

    static string GitUserName(string repo)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"-C \"{repo}\" config user.name")
            {
                RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false
            };
            using var p = Process.Start(psi);
            if (p is null) return "";
            var output = p.StandardOutput.ReadToEnd().Trim();
            p.WaitForExit();
            return p.ExitCode == 0 ? output : "";
        }
        catch (Exception)
        {
            return "";
        }
    }

    static string Rel(string root, string full) =>
        Path.GetRelativePath(root, full).Replace('\\', '/');

    static int Depth(string rel) => rel.Count(c => c == '/') + 1;

    static List<string> SortedDirs(string root, bool topOnly)
    {
        if (!Directory.Exists(root)) return new List<string>();
        var options = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
        return Directory.EnumerateDirectories(root, "*", options).OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    static List<string> SortedFiles(string root, string pattern, SearchOption options)
    {
        if (!Directory.Exists(root)) return new List<string>();
        return Directory.EnumerateFiles(root, pattern, options).OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    // ───────────────────────── 体检模式（只读） ─────────────────────────

    int Audit()
    {
        _gaps = _misplaced = _namingBad = _nonstd = _users = _readmeDiff = _subdoc = 0;
        Console.WriteLine($"体检模式（只读，不落盘）：{_docs}");
        Console.WriteLine(_userName.Length > 0
            ? $"用户名：{_userName}（用于生成迁移建议）"
            : "用户名：未确定——加 --user 可让迁移建议落到具体路径");
        Console.WriteLine();

        if (!Directory.Exists(_docs))
        {
            Console.WriteLine("[缺口] 没有 docs/ 目录。按新建项目路径处理即可（去掉 --audit 重新运行），无历史包袱。");
            Console.WriteLine();
            Console.WriteLine("体检完成：缺口 1 项、待迁移 0 项、命名不合规 0 项、非标准目录 0 项。");
            return 0;
        }

        Console.WriteLine("— 1. 类型目录（过程类 5 + 交付类 3）—");
        foreach (var t in AllTypes)
        {
            if (Directory.Exists(Path.Combine(_docs, t))) continue;
            // 分析类的旧名 analysis/ 已存在时视为该类型已具备，不逼存量项目再建一份 analyses/
            if (t == "analyses" && Directory.Exists(Path.Combine(_docs, "analysis"))) continue;
            Console.WriteLine(IsProcessType(t)
                ? $"  [缺口] 缺少过程类目录 docs/{t}/"
                : $"  [缺口] 缺少交付类目录 docs/{t}/");
            _gaps++;
        }

        if (Directory.Exists(Path.Combine(_docs, "analysis")) && Directory.Exists(Path.Combine(_docs, "analyses")))
        {
            Console.WriteLine("  [建议] docs/analysis/ 与 docs/analyses/ 并存——分析类目录只留一种写法，按通行惯例保留 analyses/，把 analysis/ 下的文档 git mv 合并进去");
        }

        var registered = ReadRegistry();
        foreach (var dir in SortedDirs(_docs, topOnly: true))
        {
            var b = Path.GetFileName(dir);
            if (IsType(b)) continue;
            if (registered.Contains(b))
                Console.WriteLine($"  [已登记] docs/{b}/ 已在项目自有目录登记表中——核对其中职责描述是否仍与现实一致");
            else
            {
                Console.WriteLine($"  [非标准] docs/{b}/ 不在类型目录内，也未登记——保留则登记职责与命名约定，或并入类型目录");
                _nonstd++;
            }
        }

        Console.WriteLine();
        Console.WriteLine("— 2. 用户目录与退役子目录 —");
        foreach (var t in ProcessTypes)
        {
            var dir = Path.Combine(_docs, t);
            if (!Directory.Exists(dir)) continue;
            var subDirs = Directory.EnumerateDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            var hasMarkdown = Directory.EnumerateFiles(dir, "*.md", SearchOption.TopDirectoryOnly).Any();
            if (subDirs.Count == 0 && hasMarkdown)
            {
                Console.WriteLine($"  [不合规] docs/{t}/ 下直接放了文件，缺少 <用户名>/ 子目录");
                _misplaced++;
            }
        }
        foreach (var t in DeliveryTypes)
        {
            if (_userName.Length == 0) continue;
            if (Directory.Exists(Path.Combine(_docs, t, _userName)))
            {
                Console.WriteLine($"  [不合规] docs/{t}/{_userName}/ —— 交付类平铺存放，不按用户名分目录");
                _misplaced++;
            }
        }
        var adrDir = Path.Combine(_docs, "adr");
        foreach (var userDir in SortedDirs(adrDir, topOnly: true))
        {
            var b = Path.GetFileName(userDir);
            foreach (var sub in new[] { "archive", "deprecated" })
            {
                if (Directory.Exists(Path.Combine(userDir, sub))) continue;
                Console.WriteLine($"  [缺口] docs/adr/{b}/{sub}/ 缺失（退役文档无处可去）");
                _gaps++;
            }
            _users++;
        }

        Console.WriteLine();
        Console.WriteLine("— 3. 位置与命名（过程类期望 docs/<类型>/<用户名>/<文件>，交付类期望 docs/<类型>/<文件>）—");
        var scanned = SortedFiles(_docs, "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                        || BinaryDocExts.Contains(Path.GetExtension(f).ToLowerInvariant()));
        foreach (var file in scanned)
        {
            var rel = Rel(_docs, file);
            var baseName = Path.GetFileName(file);
            if (rel == "README.md") continue;
            var depth = Depth(rel);
            var t1 = rel.Split('/')[0];
            var expected = IsType(t1) ? (IsProcessType(t1) ? 3 : 2) : 3;

            // 已登记的项目自有目录：目录级已在第 1 节放行，文件级不再套类型规则——
            // 否则 docs/other/、docs/reviews/ 这类登记过的目录，其文件会被重复报成待迁移或非标准。
            if (!IsType(t1) && registered.Contains(t1)) continue;

            if (depth < expected)
            {
                var sug = SuggestTarget(baseName);
                Console.WriteLine(sug.Length > 0
                    ? $"  [待迁移] {rel} → 建议 git mv docs/{rel} docs/{sug}"
                    : $"  [待迁移] {rel} → 目标类型需人工判断（文件名无法推断体裁）");
                _misplaced++;
                continue;
            }
            if (!IsType(t1))
            {
                Console.WriteLine($"  [非标准] docs/{rel} 位于非类型目录 docs/{t1}/ 之下");
                _nonstd++;
                continue;
            }
            if (IsProcessType(t1))
            {
                // 过程类允许三层（<类型>/<用户名>/<文件>）或四层（再加一层主题子目录，
                // 如 Workspace/、picoplugins-v2/，或 archive/、deprecated/ 两个退役子目录）；
                // 第五层起仍判层级过深，避免目录无限下钻。
                if (depth > 4)
                {
                    Console.WriteLine($"  [不合规] docs/{rel} 层级过深——过程类最多到 <类型>/<用户名>/<主题>/<文件>");
                    _misplaced++;
                    continue;
                }
            }
            else if (depth > 2)
            {
                // 交付类路径是 docs/<类型>/<子目录>/<文件>，子目录在第 2 段（下标 1）；
                // 旧版取下标 2 拿到的是文件名，导致 assets/ 与退役子目录从未真正放行。
                var l3 = rel.Split('/')[1];
                if (l3 != "assets" && l3 != "deprecated" && l3 != "archive")
                {
                    Console.WriteLine($"  [不合规] docs/{rel} 位于交付类目录的子目录中——交付类平铺存放，不按人分目录");
                    _misplaced++;
                    continue;
                }
            }
            // 主题子目录内的文件按专题习惯命名（open-issues.md、phase0-*.md 这类），
            // 命名约定只对用户目录直属与 archive/deprecated 中的文档作硬性判定。
            var topicLevel = IsProcessType(t1) && depth == 4 ? rel.Split('/')[2] : "";
            var inTopicDir = topicLevel.Length > 0 && topicLevel != "archive" && topicLevel != "deprecated";
            if (!NamingOk(t1, baseName))
            {
                if (inTopicDir)
                {
                    Console.WriteLine($"  [提示] docs/{rel} 未按 {t1}/ 的命名约定——主题子目录内不作硬性判定");
                }
                else
                {
                    Console.WriteLine($"  [命名] docs/{rel} 不符合 {t1}/ 的命名约定");
                    _namingBad++;
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("— 4. 子系统文档目录 —");
        var readmePath = Path.Combine(_docs, "README.md");
        var readmeText = File.Exists(readmePath) ? File.ReadAllText(readmePath) : null;
        var srcRoot = Path.Combine(_target, "src");
        foreach (var dir in SortedDirs(srcRoot, topOnly: false))
        {
            var name = Path.GetFileName(dir);
            if (name != "docs" && name != "~doc") continue;
            if (Depth(Rel(srcRoot, dir)) > 3) continue;   // 与旧版 find -maxdepth 3 同口径
            var srel = Rel(_target, dir);
            if (readmeText is not null && readmeText.Contains(srel))
            {
                Console.WriteLine($"  [已登记] {srel}/ 已在根 README 中登记");
            }
            else
            {
                Console.WriteLine($"  [待登记] {srel}/ 自带文档目录——在根 README 的「子系统文档目录登记」中写明范围与分工");
                _subdoc++;
            }
        }
        if (_subdoc == 0) Console.WriteLine("  未发现未登记的子系统文档目录");

        Console.WriteLine();
        Console.WriteLine("— 5. 规则文本 —");
        if (readmeText is null)
        {
            Console.WriteLine("  [缺口] 没有 docs/README.md——规则无处可循，可直接铺模板");
            _gaps++;
        }
        else
        {
            foreach (var sec in RequiredSections)
            {
                if (readmeText.Contains(sec)) continue;
                Console.WriteLine($"  [缺口] docs/README.md 缺少「{sec}」节");
                _gaps++;
            }
            foreach (var sec in SuggestedSections)
            {
                if (readmeText.Contains(sec)) continue;
                Console.WriteLine($"  [建议] docs/README.md 没有「{sec}」节——项目有额外目录、附件或子系统文档时需要它");
            }
            if (readmeText.Contains("{{"))
            {
                Console.WriteLine("  [异常] docs/README.md 中残留未替换的占位符");
                _gaps++;
            }

            var rendered = Subst(File.ReadAllText(Path.Combine(_tplDir, "README.md"), Encoding.UTF8));
            if (SameLines(readmeText, rendered))
            {
                Console.WriteLine("  [一致] docs/README.md 与本模板渲染后的内容逐行一致");
            }
            else
            {
                _readmeDiff = 1;
                Console.WriteLine("  [差异] docs/README.md 与本模板不同——逐节对照后决定：保留项目自有条款只补缺失节，");
                Console.WriteLine("         或用 --force 整体替换（会丢掉项目已有条款，替换前先确认并提交一份存档）；");
                Console.WriteLine("         若本项目 README 就是本模板生成的，请带上同样的 --user/--project/--date 再测一次");
            }

            var idx = readmeText.Split('\n').Count(l => l.StartsWith("| `docs/", StringComparison.Ordinal));
            var files = SortedFiles(_docs, "*.md", SearchOption.AllDirectories)
                .Count(f => Path.GetFileName(f) != "README.md" && Depth(Rel(_docs, f)) >= 2);
            Console.WriteLine($"  [信息] 示例索引登记 {idx} 条，docs/ 下实际文档 {files} 份——逐条核对，索引与实物必须一致");
        }

        Console.WriteLine();
        Console.WriteLine($"体检完成：缺口 {_gaps} 项、待迁移 {_misplaced} 项、命名不合规 {_namingBad} 项、非标准目录 {_nonstd} 项、子系统文档未登记 {_subdoc} 项。");
        if (_users > 0) Console.WriteLine($"涉及用户目录 {_users} 个。");
        if (_gaps == 0 && _misplaced == 0 && _namingBad == 0 && _nonstd == 0 && _subdoc == 0 && _readmeDiff == 0)
        {
            Console.WriteLine("结论：该项目的 docs 体系与规则一致，无需补全。");
        }
        else
        {
            Console.WriteLine("后续动作：先决定非标准目录与规则冲突的取舍，再跑一次不带 --audit 的生成（幂等补齐缺口），");
            Console.WriteLine("最后逐条执行上面列出的 git mv（保留文件名与编号，不重命名、不删除未获批的他人文档），");
            Console.WriteLine("并同步 docs/README.md 的目录表与示例索引。");
        }
        Console.WriteLine("本模式不改动任何文件；迁移涉及他人文档或需要改写正文时，先取得确认。");
        return 0;
    }

    /// <summary>读「项目自有目录登记表」一节里登记过的目录名（等价于旧版 sed 范围 + grep 的取法）。</summary>
    List<string> ReadRegistry()
    {
        var names = new List<string>();
        var readmePath = Path.Combine(_docs, "README.md");
        if (!File.Exists(readmePath)) return names;
        var lines = File.ReadAllLines(readmePath, Encoding.UTF8);
        var inside = false;
        foreach (var line in lines)
        {
            if (!inside)
            {
                if (line.StartsWith("## 项目自有目录登记表", StringComparison.Ordinal)) inside = true;
                continue;
            }
            if (line.StartsWith("## ", StringComparison.Ordinal)) break;
            var m = Regex.Match(line, @"^\| `([A-Za-z0-9._-]+)/`");
            if (m.Success) names.Add(m.Groups[1].Value);
        }
        return names;
    }

    static bool SameLines(string a, string b)
    {
        var la = a.Split('\n').Select(l => l.TrimEnd()).ToArray();
        var lb = b.Split('\n').Select(l => l.TrimEnd()).ToArray();
        return la.Length == lb.Length && la.Zip(lb).All(p => p.First == p.Second);
    }

    // ───────────────────────── 生成／补全模式 ─────────────────────────

    int Generate()
    {
        if (_userName.Length == 0)
        {
            Console.Error.WriteLine("取不到用户名：目标仓库未配置 git config user.name，请用 --user 显式给出（须与 Git 账号一致）。");
            return 1;
        }
        if (_userName.Contains('/') || _userName.Contains('\\'))
        {
            Console.Error.WriteLine($"用户名不能含路径分隔符：{_userName}");
            return 1;
        }
        if (!Regex.IsMatch(_dateStr, @"^[0-9]{4}-[0-9]{2}-[0-9]{2}$"))
        {
            Console.Error.WriteLine($"--date 需为 YYYY-MM-DD 形式，收到：{_dateStr}");
            return 1;
        }

        Console.WriteLine($"目标：{_docs}");
        var flags = (_force ? "  --force" : "") + (_dry ? "  --dry-run" : "");
        Console.WriteLine($"参数：用户名={_userName}  项目名={_project}  日期={_dateStr}  示例={(_withExamples ? "是" : "否")}{flags}");
        if (Directory.Exists(_docs))
            Console.WriteLine("提示：docs/ 已存在，本次为补全与维护——已存在的文件默认跳过，不覆盖既有内容；先跑 --audit 看差距清单。");
        Console.WriteLine();

        if (_withExamples)
        {
            foreach (var dir in SortedDirs(_tplDir, topOnly: false))
                MkdirDir(Rel(_tplDir, dir));
            foreach (var file in SortedFiles(_tplDir, "*", SearchOption.AllDirectories))
                Place(file, Rel(_tplDir, file));
        }
        else
        {
            Place(Path.Combine(_tplDir, "README.md"), "README.md");
            foreach (var t in ProcessTypes)
            {
                MkdirDir($"{t}/{{{{用户名}}}}");
                Gitkeep(Path.Combine(_docs, Subst(Path.Combine(t, "{{用户名}}"))));
            }
            foreach (var t in DeliveryTypes)
            {
                MkdirDir(t);
                Gitkeep(Path.Combine(_docs, t));
            }
            MkdirDir("adr/{{用户名}}/archive");
            MkdirDir("adr/{{用户名}}/deprecated");
            foreach (var sub in new[] { "archive", "deprecated" })
                Gitkeep(Path.Combine(_docs, Subst(Path.Combine("adr", "{{用户名}}", sub))));
        }

        Console.WriteLine();
        if (_dry)
        {
            Console.WriteLine($"预演结束：计划 {_planned} 个文件、{_skipped} 个已存在被跳过（--dry-run 未落盘）。");
            return 0;
        }

        var missing = false;
        var readme = Path.Combine(_docs, "README.md");
        if (!File.Exists(readme))
        {
            Console.Error.WriteLine("校验失败：未生成 docs/README.md");
            missing = true;
        }
        else if (File.ReadAllText(readme, Encoding.UTF8).Contains("{{"))
        {
            Console.Error.WriteLine("校验失败：docs/README.md 中仍有未替换的占位符");
            missing = true;
        }
        if (missing) return 2;

        Console.WriteLine($"完成：写入 {_created} 个文件、跳过 {_skipped} 个已存在文件，校验通过。");
        Console.WriteLine("提示：本次只补骨架与示例，未改动任何既有文档；位置不合规的历史文档需按 --audit 的建议另行迁移。");
        return 0;
    }

    void Place(string src, string rel)
    {
        var target = Subst(rel);
        var dst = Path.Combine(_docs, target);
        if (File.Exists(dst) || Directory.Exists(dst))
        {
            if (!_force)
            {
                _skipped++;
                Console.WriteLine($"  跳过（已存在）：{target}");
                return;
            }
        }
        if (_dry)
        {
            _planned++;
            Console.WriteLine($"  将写入：{target}");
            return;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(dst)!);
        File.WriteAllText(dst, Subst(File.ReadAllText(src, Encoding.UTF8)), new UTF8Encoding(false));
        _created++;
        Console.WriteLine($"  写入：{target}");
    }

    void MkdirDir(string rel)
    {
        var target = Subst(rel);
        if (_dry)
        {
            Console.WriteLine($"  将创建目录：{target}/");
            return;
        }
        Directory.CreateDirectory(Path.Combine(_docs, target));
    }

    void Gitkeep(string dir)
    {
        if (_dry) return;
        Directory.CreateDirectory(dir);
        var kp = Path.Combine(dir, ".gitkeep");
        if (!File.Exists(kp)) File.WriteAllText(kp, "", new UTF8Encoding(false));
    }

    // ───────────────────────── 内置回归自检 ─────────────────────────

    /// <summary>把行为回归做成子命令：在临时目录里重建各类现状并断言，不接触任何真实项目。</summary>
    int SelfTest()
    {
        var pass = 0;
        var fail = 0;
        var root = Path.Combine(Path.GetTempPath(), "youren-initdocs-selftest-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);

        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { pass++; Console.WriteLine($"  PASS  {name}"); }
            else { fail++; Console.WriteLine($"  FAIL  {name}{(detail.Length > 0 ? " — " + detail : "")}"); }
        }
        int RunTool(out string output, params string[] argv)
        {
            var prevOut = Console.Out;
            var prevErr = Console.Error;
            var buffer = new StringWriter();
            try
            {
                Console.SetOut(buffer);
                Console.SetError(buffer);
                var code = new InitDocs().Run(argv);
                output = buffer.ToString();
                return code;
            }
            finally
            {
                Console.SetOut(prevOut);
                Console.SetError(prevErr);
            }
        }

        try
        {
            // 1. 新项目 + 示例
            var fresh = Path.Combine(root, "fresh");
            Directory.CreateDirectory(fresh);
            Git(fresh, "init -q .");
            Git(fresh, "config user.name lukan");
            var code = RunTool(out _, "--target", fresh, "--user", "lukan", "--project", "测试项目", "--date", "2026-09-28");
            Check("示例模式退出码 0", code == 0, $"exit={code}");
            Check("示例模式文件数 11", CountFiles(Path.Combine(fresh, "docs")) == 11,
                $"实得 {CountFiles(Path.Combine(fresh, "docs"))}");
            Check("八类目录齐", AllTypes.All(t => Directory.Exists(Path.Combine(fresh, "docs", t))));
            Check("交付类不含用户名目录",
                DeliveryTypes.All(t => !Directory.EnumerateDirectories(Path.Combine(fresh, "docs", t)).Any()));
            Check("交付类各有自述", DeliveryTypes.All(t => File.Exists(Path.Combine(fresh, "docs", t, "README.md"))));
            Check("占位符零残留", !AnyFileContains(Path.Combine(fresh, "docs"), "{{"));
            var hashBefore = TreeHash(Path.Combine(fresh, "docs"));

            // 2. 幂等
            RunTool(out var second, "--target", fresh, "--user", "lukan", "--project", "测试项目", "--date", "2026-09-28");
            Check("幂等：11 个全部跳过", CountOf(second, "跳过（已存在）") == 11, $"实得 {CountOf(second, "跳过（已存在）")}");
            Check("幂等：写入 0", CountOf(second, "写入：") == 0);
            Check("幂等：内容不变", TreeHash(Path.Combine(fresh, "docs")) == hashBefore);

            // 3. 新生成项目的体检
            code = RunTool(out var auditFresh, "--target", fresh, "--audit", "--user", "lukan", "--project", "测试项目", "--date", "2026-09-28");
            Check("体检退出码 0", code == 0, $"exit={code}");
            Check("体检结论为无需补全", auditFresh.Contains("结论：该项目的 docs 体系与规则一致，无需补全。"));
            Check("体检报告无任何问题项",
                !Regex.IsMatch(auditFresh, @"\[缺口\]|\[待迁移\]|\[非标准\]|\[不合规\]|\[命名\]"));
            Check("规则与本模板逐行一致", auditFresh.Contains("[一致] docs/README.md 与本模板渲染后的内容逐行一致"));

            // 4. 空骨架模式
            var bare = Path.Combine(root, "bare");
            Directory.CreateDirectory(bare);
            Git(bare, "init -q .");
            Git(bare, "config user.name wangwu");
            RunTool(out _, "--target", bare, "--no-examples");
            Check("空骨架：取到 git 身份作为用户名", Directory.Exists(Path.Combine(bare, "docs", "adr", "wangwu")));
            Check("空骨架：交付类三目录平铺",
                DeliveryTypes.All(t => Directory.Exists(Path.Combine(bare, "docs", t))));
            Check("空骨架：.gitkeep 10 个", CountGitkeep(Path.Combine(bare, "docs")) == 10,
                $"实得 {CountGitkeep(Path.Combine(bare, "docs"))}");
            Check("空骨架：无示例文档",
                !Directory.EnumerateFiles(Path.Combine(bare, "docs"), "*.md", SearchOption.AllDirectories)
                    .Any(f => Path.GetFileName(f) != "README.md"));

            // 5. 乱的既有项目
            var messy = Path.Combine(root, "messy");
            foreach (var d in new[]
                     {
                         "docs/plan", "docs/adr/lukan", "docs/daily/lukan", "docs/plans",
                         "docs/backlog", "docs/spec/lukan", "src/Foo/docs"
                     })
                Directory.CreateDirectory(Path.Combine(messy, d));
            File.WriteAllText(Path.Combine(messy, "docs", "手册.docx"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "plan", "note.md"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "adr", "lukan", "ADR-1-bad.md"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "daily", "lukan", "2026-09-01-summary.md"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "plans", "PLAN-0001-no-user-dir.md"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "backlog", "2026-08-14-x.md"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "spec", "lukan", "接口规范.md"), "x");
            File.WriteAllText(Path.Combine(messy, "docs", "README.md"),
                "# 某项目文档体系\n## 目录一览\nadr/ analysis/ daily/ design/ plans/\n## 个人目录规则\n## 文档流转\n"
                + "## 各类型文档的必备章节\n## 失效与存档\n## 书写规范\n## 示例索引\n"
                + "## 项目自有目录登记表\n| 目录 | 类别 | 承载内容 | 文件命名约定 |\n| :--- | :--- | :--- | :--- |\n"
                + "| `backlog/` | 过程类 | 已发现但未承诺的问题池 | `<YYYY-MM-DD>-<主题-slug>.md` |\n");
            var filesBefore = CountFiles(messy);
            RunTool(out var auditMessy, "--target", messy, "--audit", "--user", "lukan");
            Check("缺两个过程类目录", CountOf(auditMessy, "缺少过程类目录") == 2, $"实得 {CountOf(auditMessy, "缺少过程类目录")}");
            Check("缺两个交付类目录", CountOf(auditMessy, "缺少交付类目录") == 2, $"实得 {CountOf(auditMessy, "缺少交付类目录")}");
            Check("已登记目录放行", auditMessy.Contains("[已登记] docs/backlog/"));
            Check("已登记目录下的文件不再重复报错",
                !auditMessy.Contains("[非标准] docs/backlog/") && !auditMessy.Contains("[待迁移] backlog/"));
            Check("未登记目录报非标准", auditMessy.Contains("[非标准] docs/plan/"));
            Check("交付类下建用户目录报不合规（目录级+文件级）", CountOf(auditMessy, "交付类平铺存放") == 2,
                $"实得 {CountOf(auditMessy, "交付类平铺存放")}");
            Check("外部原件建议迁入 reference/",
                auditMessy.Contains("[待迁移] 手册.docx → 建议 git mv docs/手册.docx docs/reference/手册.docx"));
            Check("无用户目录的文档给 git mv 建议",
                auditMessy.Contains("[待迁移] plans/PLAN-0001-no-user-dir.md → 建议 git mv docs/plans/"));
            Check("命名不合规被抓出", auditMessy.Contains("[命名] docs/adr/lukan/ADR-1-bad.md 不符合"));
            Check("子系统文档待登记", auditMessy.Contains("[待登记] src/Foo/docs/"));
            Check("三节建议提示都在", CountOf(auditMessy, "[建议] docs/README.md 没有") == 2,
                $"实得 {CountOf(auditMessy, "[建议] docs/README.md 没有")}");
            Check("既有 README 报差异", auditMessy.Contains("[差异] docs/README.md 与本模板不同"));
            Check("体检只读（文件数不变）", CountFiles(messy) == filesBefore);

            // 6. 登记子系统文档后复查
            File.AppendAllText(Path.Combine(messy, "docs", "README.md"),
                "\n## 子系统文档目录登记\n| 子系统文档目录 | 范围 | 与根 docs 的分工 |\n| :--- | :--- | :--- |\n| src/Foo/docs/ | Foo 模块内部实现 | 只讲 Foo 内部细节 |\n");
            RunTool(out var auditRegistered, "--target", messy, "--audit", "--user", "lukan");
            Check("子系统登记后放行", auditRegistered.Contains("[已登记] src/Foo/docs/"));
            Check("子系统未登记计数归零", auditRegistered.Contains("子系统文档未登记 0 项"));

            // 7. 补缺口后复查
            RunTool(out _, "--target", messy, "--user", "lukan", "--date", "2026-09-28");
            Check("补缺口：缺的目录已建",
                new[] { "analyses", "design", "guide", "reference" }
                    .All(t => Directory.Exists(Path.Combine(messy, "docs", t))));
            Check("补缺口：既有旧文档未动",
                File.Exists(Path.Combine(messy, "docs", "plan", "note.md"))
                && File.Exists(Path.Combine(messy, "docs", "adr", "lukan", "ADR-1-bad.md")));
            RunTool(out var auditAfter, "--target", messy, "--audit", "--user", "lukan");
            Check("补缺口后缺口归零", auditAfter.Contains("缺口 0 项"), "报告里未见「缺口 0 项」");

            // 7b. 同义异名与交付类退役子目录
            var alias = Path.Combine(root, "alias");
            foreach (var d in new[] { "docs/analysis/lukan", "docs/analyses/lukan", "docs/spec/deprecated" })
                Directory.CreateDirectory(Path.Combine(alias, d));
            File.WriteAllText(Path.Combine(alias, "docs", "analysis", "lukan", "2026-09-02_y.md"), "x");
            File.WriteAllText(Path.Combine(alias, "docs", "analyses", "lukan", "2026-09-01_x.md"), "x");
            File.WriteAllText(Path.Combine(alias, "docs", "spec", "deprecated", "old-spec.md"), "x");
            RunTool(out var auditAlias, "--target", alias, "--audit", "--user", "lukan");
            Check("analyses 不再报非标准", !auditAlias.Contains("[非标准] docs/analyses/"));
            Check("交付类 deprecated 不报平铺违规", !auditAlias.Contains("交付类平铺存放"));
            Check("两种分析类目录并存给提示", auditAlias.Contains("docs/analysis/ 与 docs/analyses/ 并存")
                                              && auditAlias.Contains("保留 analyses/"));

            // 7c. 过程类的主题子目录：四层放行，五层起仍判层级过深
            var deep = Path.Combine(root, "deep");
            Directory.CreateDirectory(Path.Combine(deep, "docs", "plans", "lukan", "picoplugins-v2"));
            Directory.CreateDirectory(Path.Combine(deep, "docs", "plans", "lukan", "archive"));
            Directory.CreateDirectory(Path.Combine(deep, "docs", "plans", "lukan", "a", "b"));
            File.WriteAllText(Path.Combine(deep, "docs", "plans", "lukan", "picoplugins-v2", "PLAN-0002-topic.md"), "x");
            File.WriteAllText(Path.Combine(deep, "docs", "plans", "lukan", "picoplugins-v2", "open-issues.md"), "x");
            File.WriteAllText(Path.Combine(deep, "docs", "plans", "lukan", "archive", "2026-11-08-note.md"), "x");
            File.WriteAllText(Path.Combine(deep, "docs", "plans", "lukan", "a", "b", "PLAN-0003-deep.md"), "x");
            RunTool(out var auditDeep, "--target", deep, "--audit", "--user", "lukan");
            Check("主题子目录放行", !auditDeep.Contains("picoplugins-v2/PLAN-0002-topic.md 层级过深"));
            Check("主题子目录内命名只提示",
                auditDeep.Contains("[提示] docs/plans/lukan/picoplugins-v2/open-issues.md")
                && !auditDeep.Contains("[命名] docs/plans/lukan/picoplugins-v2/open-issues.md"));
            Check("退役子目录仍按命名判定", auditDeep.Contains("[命名] docs/plans/lukan/archive/2026-11-08-note.md"));
            Check("第五层仍判层级过深", auditDeep.Contains("a/b/PLAN-0003-deep.md 层级过深"));

            // 8. 参数与环境校验
            Check("目标不存在退 1", RunTool(out _, "--target", Path.Combine(root, "不存在")) == 1);
            var noGit = Path.Combine(root, "noGit");
            Directory.CreateDirectory(noGit);
            // 「未配身份」这一条在本机已配全局身份时会必然误判：临时屏蔽全局与系统 git 配置再验
            var emptyConfig = Path.Combine(root, "empty-gitconfig");
            File.WriteAllText(emptyConfig, "");
            var oldGlobal = Environment.GetEnvironmentVariable("GIT_CONFIG_GLOBAL");
            var oldNoSystem = Environment.GetEnvironmentVariable("GIT_CONFIG_NOSYSTEM");
            Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", emptyConfig);
            Environment.SetEnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1");
            try
            {
                Check("无身份不传 --user 退 1", RunTool(out _, "--target", noGit, "--no-examples") == 1);
            }
            finally
            {
                Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", oldGlobal);
                Environment.SetEnvironmentVariable("GIT_CONFIG_NOSYSTEM", oldNoSystem);
            }
            Check("用户名含分隔符退 1", RunTool(out _, "--target", bare, "--user", "bad/name") == 1);
            Check("日期格式错退 1", RunTool(out _, "--target", bare, "--user", "lukan", "--date", "2026/09/28") == 1);
            Check("未知参数退 1", RunTool(out _, "--bogus") == 1);
            Check("缺 --target 退 1", RunTool(out _) == 1);
            Check("--help 退 0 且打印用法", RunTool(out var help, "--help") == 0 && help.Contains("--target DIR"),
                $"exit/内容不符：{help.Split('\n')[0]}");
            Check("help 与 --help 输出一致", RunTool(out var help2, "help") == 0 && help2 == help);
            var dryDir = Path.Combine(root, "dry");
            Directory.CreateDirectory(dryDir);
            Check("--dry-run 不落盘", RunTool(out var dry, "--target", dryDir, "--user", "lukan", "--dry-run") == 0
                                     && dry.Contains("预演结束") && !Directory.Exists(Path.Combine(dryDir, "docs")));
        }
        finally
        {
            try { Directory.Delete(root, true); } catch (IOException) { /* 清理失败不影响结论 */ }
        }

        Console.WriteLine();
        Console.WriteLine($"自检：{pass} 项通过，{fail} 项失败。");
        return fail == 0 ? 0 : 1;
    }

    static void Git(string dir, string args)
    {
        try
        {
            var psi = new ProcessStartInfo("git", $"-C \"{dir}\" {args}")
            {
                RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false
            };
            using var p = Process.Start(psi);
            p?.WaitForExit();
        }
        catch (Exception)
        {
            // 自检环境里没有 git 时不影响其余断言
        }
    }

    static int CountFiles(string dir) => Directory.Exists(dir)
        ? Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Count()
        : 0;

    static int CountGitkeep(string dir) => Directory.Exists(dir)
        ? Directory.EnumerateFiles(dir, ".gitkeep", SearchOption.AllDirectories).Count()
        : 0;

    static bool AnyFileContains(string dir, string needle) => Directory
        .EnumerateFiles(dir, "*", SearchOption.AllDirectories)
        .Any(f => File.ReadAllText(f, Encoding.UTF8).Contains(needle));

    static string TreeHash(string dir)
    {
        var sb = new StringBuilder();
        foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories)
                     .OrderBy(p => p, StringComparer.Ordinal))
        {
            sb.Append(Rel(dir, f)).Append(' ')
              .Append(Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(f)))).Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    static int CountOf(string text, string needle)
    {
        var count = 0;
        var idx = 0;
        while ((idx = text.IndexOf(needle, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += needle.Length;
        }
        return count;
    }
}
