# ImageLyre.Presentation.Desktop

## 意义

本工程是 L5 桌面 Presentation，承载桌面宿主的 Presentation 阶段：AtomUI 主题与设计语言注册（`DesktopEntry`）、欢迎窗与文案源、桌面启动入口（`DesktopStartup`）、槽位管理器、取壳与 Feature 附着（`Startup/MainWindowFactory`）、`PresentationPlan`、主窗体显示与欢迎窗关闭、冒烟回调与桌面关停收口。它把“插件就绪之后用户看得见的那段启动”从 `ImageLyre.App` 的方法体移出来，使 Host 阶段的机制启动与 Presentation 阶段的界面构造各自有明确归属，也让 `ImageLyre.App` 退回到只解析参数、固定 UTF-8、分派桌面或 CLI 并转交 Avalonia 生命周期的入口。

本工程不是 `IHost` 的持有者：Generic Host 的构建、插件启动、`HostPlan`、`StartupReport` 与 `ExitArbiter` 都在 `ImageLyre.Bootstrap`，本工程只通过 `BootstrapHostBuilder.Build` 构建并启动宿主、按 `PresentationPlan` 在 UI 线程执行界面步骤，并把宿主生命周期接到 Avalonia 的桌面生命周期上。

## 边界（不放什么）

- 不放宿主机制：不在本工程实现扫描、容器注册或插件生命周期，这些属于 `ImageLyre.Bootstrap`；本工程只调用它的构建器并消费交出的宿主与服务。
- 不放进程入口：不解析命令行、不判断桌面还是 CLI、不调用 Avalonia 的启动入口，这些留在 `ImageLyre.App`；本工程不引用 `ImageLyre.App`，也不引用它的 `StartupOptions`，边界只有 `IClassicDesktopStyleApplicationLifetime`、`ILocalizationService` 与 `ILoggerFactory` 三样。
- 不放插件：不按名字引用任何 `ImageLyre.Module.*` 或 `ImageLyre.Feature.*` 工程，主窗体由壳插件产出，本工程只负责取壳与挂载。
- 不放产品能力：M1 阶段没有图像、ROI、测量、通道或校准代码，后续能力只能以插件形式接入。
- 不放契约：`ILocalizationService`、`IWorkbenchShell`、`ShellSlots`、`IMenuRegistry` 等类型留在 `ImageLyre.Contracts`、`ImageLyre.Shell.Contracts` 与 `ImageLyre.Shell.Menus`，本工程只引用。
- 不放完整关停策略：`Startup/DesktopShutdown` 用 `ImageLyre.Bootstrap` 的 `ExitArbiter` 保证“开始一次、关停一次、返回可等待任务与超时”，`Startup/DesktopStopPlan` 以“关闭壳 → 停宿主 → 释放宿主”三个动作执行，因没有日志管线而不拆组；`ShutdownRequested` 的取消与重入由 `DesktopStartup` 负责。
- 不放 Plan 执行算法：逐 Step 执行、写 `StartupReport`、`Degraded` 继续与 `Failed` 逆序回滚由 Bootstrap 的 `BootstrapPlanRunner` 统一实现，本工程只提供 `PresentationPlan` 与上下文。

## 依赖方向

引用 `ImageLyre.Bootstrap`（Host 阶段编排、Step/Plan 契约与宿主构建器）、`ImageLyre.Contracts`（本地化与服务契约）、`ImageLyre.Shell.Contracts`（槽位与壳契约）、`ImageLyre.Shell.Menus`（菜单注册表实现 `MenuRegistry`）、`Cangming.PicoPlugins`（插件管理器与 `SlotManager`），以及 `Avalonia`、`AtomUI.Desktop.Controls`、`AtomUI.DesignLanguages.AntDesign`、`AtomUI.Desktop.Controls.Themes.AntDesign` 四个 UI 包。被 `ImageLyre.App` 引用；不被 `ImageLyre.Bootstrap` 引用，也不被任何插件工程引用。`PresentationPlan`、`DesktopBootstrapContext` 与各 Step 是内部类型，通过 `InternalsVisibleTo` 只对第 8 套测试可见。

## 边界变化时改哪份文档

先改 `docs/adr/lukan/ADR-0003-启动宿主与Presentation分层.md` 与 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§3.2，再改 `docs/design/lukan/DESIGN-0002-启动宿主与编排模式.md` §7.4、§7.5、§10.2 与 `docs/plans/lukan/PLAN-0002-启动宿主与编排模式实施.md` 的对应任务；涉及工程登记还要同步 `src/ImageLyre.slnx` 与 `scripts/qa/verify-m1.ps1`。若改动让本工程需要引用 App、插件工程或反而被 Bootstrap 引用，先按 ADR-0003 §2 修正分层结论，再动代码。
