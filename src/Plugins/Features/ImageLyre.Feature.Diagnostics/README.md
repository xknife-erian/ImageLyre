# ImageLyre.Feature.Diagnostics

## 意义

M1 的诊断 Feature：消费 `IPlatformCapabilities` 与 `IPerformanceMonitor`，向工具条附一个刷新按钮、向辅助面板附一页五行读数、向状态栏附 OS 与内存两段读数，并自带 `lyre.feature.diagnostics.*` 中英文案源。它是 M1 唯一一次把 `toolbar`、`panel`、`statusbar` 三个槽位同时填满的表达插件，用来证明能力插件产出契约、Feature 只按契约消费的链路成立。

## 边界（不放什么）

- 不放平台或性能实现：只消费 `IPlatformCapabilities` 与 `IPerformanceMonitor`，不引用 `ImageLyre.Module.Platform`、`ImageLyre.Module.Performance` 或任何 Module 工程。
- 不放壳布局：只通过 `IAttachmentRegistry` 向 `ShellSlots.Toolbar`、`ShellSlots.Panel`、`ShellSlots.StatusBar` 附着，不贡献槽位、不创建窗口。
- 不放后台采样：面板按需调用 `GetPerformanceSnapshot`，刷新按钮触发一次采样；`SnapshotUpdated` 的永不触发流不是失效。
- 不放运行期摘除：M1 没有槽位撤销，工具条标题、面板页签与状态栏文本在附着时定稿，语言切换只刷新面板内已有控件的 `Text`；就地 ponytail 指向 G3。
- 不放 public 生命周期覆盖：只覆盖 `OnStartAsync`、`OnStopAsync`、`OnDispose`、`DeclareDependencies`、`RegisterAttachments` 与 `ISupportUsingIoC.InjectAsync`。

## 依赖方向

引用 `ImageLyre.Contracts`（本地化与两个能力契约）、`ImageLyre.Shell.Contracts`（槽位名）、`ImageLyre.Shell.Attachments`（`DockPaneAttachment`）、`Cangming.PicoPlugins`（Feature 基类、IoC 注入与槽位注入）、`Cangming.PicoPlugins.Avalonias`（`ToolbarButtonAttachment` 与 `StatusBarAttachment`）、`Cangming.Toolkit`（显式 `Order`）、`Cangming.Logs`（启动日志）、`Avalonia` 与 `AtomUI.Desktop.Controls`（面板控件）。不引用 App、Bootstrap 或任何 Module 工程；由插件扫描发现，桌面宿主在取壳后调用 `RegisterFeatureAttachments` 附着。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§3.6、§4.1、§4.2、§5.5 与 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T14，再改本文件与 `DiagnosticsStringKeys`；槽位名变化先改 `ImageLyre.Shell.Contracts`，文案键变化同步扩本文件的 `TryGet` 字典。
