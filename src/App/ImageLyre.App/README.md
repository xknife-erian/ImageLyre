# ImageLyre.App

## 意义

本工程是进程入口与宿主分派点：解析启动参数后，桌面分支用 AtomUI 显示欢迎占位窗口并把文案来源固定到 `ILocalizationService`，无界面分支只做参数判定就返回 0，不构造 `AppBuilder`、不触碰任何 Avalonia 类型。把两条分支的判定收在入口一处，是为了让同一份产物既能带界面启动，也能在没有桌面环境时被 CLI 调用，而不出现“先初始化 UI 再想办法把它关掉”的半桌面状态；M1 阶段它只承担欢迎窗口与分派，主窗体、槽位附着与插件装配分别留给 T11、T12 与 `ImageLyre.Bootstrap`。

## 边界（不放什么）

- 不构建主窗体、不贡献槽位：主窗体由壳插件在自己的 `Build()` 里产出（T11 与 T12），槽位名与贡献属于 `ImageLyre.Shell.Contracts` 与壳插件，本工程只把取到的窗口挂到 `desktop.MainWindow`；欢迎窗口是 App 自己的固定语义区域，不进槽位。
- 不放插件扫描与启动：扫描前缀、容器构建、`PicoPluginSystem.Launch`、插件启动与关停都在 `ImageLyre.Bootstrap`，本工程只调用它，不复制它的顺序。
- 不放本地化实现与资源聚合：`LocalizationService` 在 `ImageLyre.Localization`，本工程只创建实例并注册 `AppStringSource` 这一个 App 自有文案源。
- 不放 CLI 命令实现：`--version`、`--plugins`、`--help` 的输出与退出码归 T06；M1 的无界面分支只把参数判成无界面并返回 0。
- 不放产品能力与主界面布局：没有图像、ROI、测量、文档、设置代码，也没有菜单、状态栏、面板容器或主题配置。

## 依赖方向

引用 `Avalonia`、`Avalonia.Desktop` 与 `AtomUI.Desktop.Controls` 承载窗口、控件与桌面生命周期；引用 `ImageLyre.Bootstrap` 供后续任务驱动启动链；引用 `ImageLyre.Localization` 取得 `LocalizationService`；引用 `ImageLyre.Contracts` 取得 `ILocalizationService` 与 `ILocalizationSource`。不引用 `ImageLyre.Shell.Contracts`（尚未建立）、`ImageLyre.Shell.Attachments`、任何 `ImageLyre.Module.*` 或 `ImageLyre.Feature.*` 工程。本工程是进程入口，不被其他工程引用。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1 的 `ImageLyre.App` 行、§3.3 的编排骨架与 CLI 分支、§4.4 的启动画面与欢迎界面、§5.6 的本地化契约，再改本文件；宿主分派规则、欢迎窗口的固定语义或 App 自有资源键变化都要回写上述章节。若变化触及“App 不构建主窗体、不贡献槽位”的分界，先按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §2.4 与 §2.5 修订架构基线再动代码；任务范围变化同步改 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T05、T06 与 T12。
