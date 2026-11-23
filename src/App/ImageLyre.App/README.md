# ImageLyre.App

## 意义

本工程是进程入口与宿主分派点：解析启动参数后，桌面分支用 AtomUI 显示欢迎占位窗口并把文案来源固定到 `ILocalizationService`，CLI 分支把参数交给 `Cli/CliHost.cs` 执行 `--version`、`--plugins`、`--help` 或拒绝无效参数，期间不构造 `AppBuilder`、不触碰任何 Avalonia 类型。把两条分支的判定收在入口一处，是为了让同一份产物既能带界面启动，也能在没有桌面环境时被 CLI 调用，而不出现“先初始化 UI 再想办法把它关掉”的半桌面状态；M1 阶段它只承担欢迎窗口、分派与三条 CLI 命令，主窗体、槽位附着与插件装配分别留给 T11、T12 与 `ImageLyre.Bootstrap`。

## 边界（不放什么）

- 不构建主窗体、不贡献槽位：主窗体由壳插件在自己的 `Build()` 里产出（T11 与 T12），槽位名与贡献属于 `ImageLyre.Shell.Contracts` 与壳插件，本工程只把取到的窗口挂到 `desktop.MainWindow`；欢迎窗口是 App 自己的固定语义区域，不进槽位。
- 不放插件扫描与启动：扫描前缀、容器构建、`PicoPluginSystem.Launch`、插件启动与关停都在 `ImageLyre.Bootstrap`，`CliHost` 只调用它并读取结果，不复制它的顺序。
- 不按名字登记插件：插件只经 `ImageLyre.App.csproj` 的通配 `ProjectReference` 被发现，新增插件不改本工程文件；按名字的插件引用一律禁止，验收 E 检查按名引用数为 0。
- 不放本地化实现与资源聚合：`LocalizationService` 在 `ImageLyre.Localization`，本工程只创建实例并注册 `AppStringSource` 这一个 App 自有文案源。
- 不放第四条命令与业务命令：`Cli/CliHost.cs` 只有 `--version`、`--plugins`、`--help` 三条命令，业务命令按 `ADR-0001` §2.5 归 G2；CLI 文本是机器可读接口，不经过 `ILocalizationService`，也不做本地化。
- 不放产品能力与主界面布局：没有图像、ROI、测量、文档、设置代码，也没有菜单、状态栏、面板容器或主题配置。

## 调试启动

`Properties/launchSettings.json` 把 M1 的每种启动形态各设一个调试档案，`dotnet run --project src/App/ImageLyre.App --launch-profile <名称>` 或 Visual Studio 的启动下拉框都能选到；档案只服务本机调试，不参与发布产物。

| 档案 | 等价启动参数 | 观察点 |
| :--- | :--- | :--- |
| `ImageLyre（桌面）` | 无参数 | 欢迎窗口常驻，需手动关闭 |
| `ImageLyre --version` | `--version` | stdout 一行版本，退出码 0 |
| `ImageLyre --plugins` | `--plugins` | stdout 插件清单与汇总行，Bootstrap 日志在 stderr |
| `ImageLyre --help` | `--help` | 用法与退出码约定，退出码 0 |
| `ImageLyre 未知参数（退出码 2）` | `--bogus` | stderr 回显参数原文，退出码 2 |
| `ImageLyre 桌面冒烟（自动关闭）` | 环境变量 `LYRE_SMOKE_CLOSE_MS=1500` | 窗口 1.5 秒后自关，退出码 0 |

`LYRE_SMOKE_CLOSE_MS` 是 `App.OnFrameworkInitializationCompleted` 里的自动冒烟钩子，只在取值为正整数时生效，接入真实 UI 测试后删除；它被写进冒烟档案，是为了让“桌面分支能起来并正常退出”这条验证不依赖人眼盯着窗口。

## 依赖方向

引用 `Avalonia`、`Avalonia.Desktop` 与 `AtomUI.Desktop.Controls` 承载窗口、控件与桌面生命周期；引用 `ImageLyre.Bootstrap` 驱动启动链；引用 `ImageLyre.Localization` 取得 `LocalizationService`；引用 `ImageLyre.Contracts` 取得本地化契约；CLI 清单读取直接使用 `Cangming.PicoPlugins` 的 `IPluginManager`、`PluginInfo` 与 `IPicoFeatures`，stderr 日志工厂直接使用 `Microsoft.Extensions.Logging.Abstractions`，这两个包因此显式声明而不依赖传递引用。不按名字引用任何插件工程，也不为插件依赖做编译引用：插件主程序集经 `ImageLyre.App.csproj` 里唯一一条指向 `src/Plugins/**/*.csproj` 的通配 `ProjectReference` 进入构建顺序与输出目录，插件自己的依赖由 `src/Plugins/Directory.Build.props` 的 `CopyLocalLockFileAssemblies` 与仓库根 `PluginOutputMerge.targets` 归并进来（测试工程导入同一份目标）。按名字的 `ImageLyre.Module.*`、`ImageLyre.Feature.*` 插件引用一律禁止。本工程是进程入口，不被其他工程引用。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1 的 `ImageLyre.App` 行、§3.3 的编排骨架与 CLI 分支、§3.7 的产物复制机制、§4.4 的启动画面与欢迎界面、§5.6 的本地化契约，再改本文件；宿主分派规则、CLI 命令集合与退出码、欢迎窗口的固定语义或 App 自有资源键变化都要回写上述章节。若变化触及“App 不构建主窗体、不贡献槽位”的分界，先按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §2.4 与 §2.5 修订架构基线再动代码；任务范围变化同步改 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T05、T06、T11 与 T12。
