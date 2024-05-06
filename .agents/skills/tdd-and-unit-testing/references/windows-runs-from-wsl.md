# 在 Windows 侧构建、运行、取数（WSL 会话）

## When to Use

改动落在 `net*-windows` / `UseWPF` / `OutputType=WinExe` 的工程上，或需要真实 Windows 行为（NTFS 时间戳、`%LOCALAPPDATA%` 路径语义、实时防护、进程与文件锁）时加载。WSL 里的 dotnet 构建不了这些目标，**但结论不是「无法验证」**——Windows 侧有 dotnet，就从本会话直接调它。

## 一、调 Windows 的 dotnet

- 解释器在 `C:\Program Files\dotnet\dotnet.exe`，从 WSL 用 `/mnt/c/Program\ Files/dotnet/dotnet.exe` 调。路径参数给**正斜杠的 Windows 形式**（`D:/Art-Code/Repo/proj/X.csproj`），避开 `cmd.exe` 的引号地狱，也不要在参数里混 `/mnt/d/...`。
- 用 WSL 自带的 `dotnet` 构建 windows 目标会得到 `NETSDK1100`（提示设 `EnableWindowsTargeting`）。这是跑错机器，不是代码问题；**不要加 `EnableWindowsTargeting` 把它糊过去**——那只会得到半真的产物，而真实 Windows 依赖仍然没被解析。换 Windows 侧 dotnet 重跑即可。
- 构建输出是本地化的：zh-CN 下摘要行是 `0 个错误` / `N 个警告` / `已成功生成`，`grep -E 'Error\(s\)'` 会**静默返回空**，看起来像「构建没有任何输出」而据此误判。要么 `tail` 看尾部摘要，要么 grep `个错误` / `已成功生成`。同样地，`ls | grep` 数目录内文件时注意正则不要匹配到**路径里的目录名**（`/Testbed.` 会把整目录都算进去）；**要一个「处理到了多少个」的数字时，取被验证对象自己报出来的那个**（库日志里的 `Found N`），别用自己的 glob/正则去数它的集合——两边匹配语义不同，对不上时你无法判断是谁错。
- 目标框架为 `net9.0` 的 xunit v3 测试工程在 WSL 侧会卡在拷贝 apphost：`MSB3030: Could not copy ".../obj/Debug/net9.0/apphost" because it was not found`，而编译阶段本身早已通过（只有 xUnit1051 之类告警），看着像工程坏了其实是跑错机器。不要用 `-p:UseAppHost=false` 绕——xunit v3 会立刻回一句 `test projects must build an app host`。同样是换 Windows 侧 dotnet 构建，`已成功生成` 后再用 `dotnet.exe exec <dll>` 取 `Total/Failed` 数字。
- 测试工程在 Windows 侧照跑：xunit v3 的测试项目本身就是可执行程序，`"<dotnet.exe>" D:/Repo/UnitTests/X/bin/Debug/net8.0/X.dll`，摘要行仍是 `Total/Errors/Failed/Skipped`。同一套测试在 Windows 上再跑一遍才有真实 NTFS 时间戳、路径大小写、用户目录语义——Linux 全绿不等于 Windows 全绿。

## 二、启动 GUI 应用（取基线/取证）

- `cmd.exe /c start "" "D:\path\app.exe"` 在 WSL 互操作下会**阻塞**（挂到超时），而超时被杀时会把刚启动的应用一起带走，看起来像「应用起不来」。改用 `powershell.exe -NoProfile -Command "Start-Process 'D:\path\app.exe'"`（立即返回），`Get-Process <名字>` 确认起来了，用完 `Stop-Process -Name <名字> -Force`——**别把窗口留在用户桌面上**。
- 动手前先确认应用把日志写哪儿。只往 UI 日志视图写 target 的应用（WPF Testbed 常见）拿不到启动耗时这类数字，**不要为了取一个数字去给产品代码加文件 target**：改用下面的仓外探针。

## 三、仓外探针：要真实数字而测试工程覆盖不到时

- 探针工程建在**仓库之外**（与仓库同级的一次性目录），`ProjectReference` 指回仓内源工程，跑完连产物一起删。往仓库里加取证代码会把一次性工具固化进产品。
- 要触达 internal 类型（生产构造不对外）：`typeof(<公开入口>).Assembly.GetType("命名空间.类型")` + `GetConstructors(BindingFlags.Instance|NonPublic|Public)` 反射构造与调用；`ProjectReference` 保证拿到同一个程序集身份，接口类型身份才对得上。
- **依赖解析要跟真实宿主一致**：把探针产物（`*.exe` / `*.dll` / `*.deps.json` / `*.runtimeconfig.json`）拷进目标应用的 bin 目录里跑，跑完把那几个文件删掉。否则被扫程序集的依赖会从探针自己的 bin 解析，判定与耗时都失真。拷进去的探针文件不命中产品的发现前缀，不会干扰被测逻辑。
- 控制台乱码：探针自己的标签用 ASCII；要读的库日志引到文件（NLog：`LogManager.Configuration = config` 加一个 `FileTarget`）再读文件，UTF-8 正常。
- **时间差必须配可核对的数量**：`A/B/A`——删掉缓存跑一次（冷）、连跑两次（热）、再删掉跑一次（回到冷）；只给 ms 的结论不算证据，**文件缓存变热也能造出同样的差值**。同时数出「命中跳过 N 次」这类行为条数（把库日志引到文件后统计行数），并核对持久化文件里的条目数与字段。报告里给「省掉 N 次文件打开 / N 次反射」，不要只给百分比。
- 顺带把「本该生效却没有」的前提也实测掉：例如某个入口注册了处理器导致缓存被设计性地禁用，就跑一遍真实应用，断言那个缓存目录**没有**被创建——把「推论」升级成实测，别只在报告里写代码推理。

## 四、开始前：这个工作区里的改动是谁的

多人/多 worker 共用同一个检出时，动手前先 `git log --oneline -5` + `git status --short`：同一处可能已被别的署名提交、或留有一段**别人未提交**的实现。看清再改——把别人的实现当成自己的空地覆盖掉，两份工作一起毁。整文件重写被工具以「内容不是本次会话读过的」拦下时，先完整读一遍再基于它改写；那正是它要挡的事。
