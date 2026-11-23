# ImageLyre.Module.Broken

## 意义

M1 的故障样本插件：以 `[Plugin(id: "lyre.capability.broken", name: "故障样本")]` 出现在插件清单里，构造函数故意抛出 `InvalidOperationException`，用来验证插件管理器的构造失败隔离、`BrokenPlugin` 占位条目与「一个坏插件不拖垮其余插件」的启动行为。它不提供任何契约，也不承担产品功能。

## 边界（不放什么）

- 不放任何能力实现：不实现 `IPlatformCapabilities`、`IPerformanceMonitor`、`IWorkbenchShell` 或界面逻辑，构造函数抛出后没有后续生命周期。
- 不进 Release 输出：仓库根 `PluginOutputMerge.targets` 从通配归并里排除本工程，只在 `Configuration == Debug` 时单独加入；构建仍会产出它，但发行输出不得包含 `ImageLyre.Module.Broken.dll`。
- 不参与验收 A–C 的能力链：只用于验收 D 的故障隔离与构造失败路径；其余插件应保持 `Running`。
- 不放多语言、槽位或诊断：故障样本越简单，越能把异常归因到构造失败本身。

## 依赖方向

只引用 `Cangming.PicoPlugins`（`BasePicoModule` 与 `[Plugin]`），不引用契约、Avalonia、AtomUI、任何 Module 或 Feature 工程。由 `src/Plugins/**/*.csproj` 通配纳入构建；Debug 构建后由 `PluginOutputMerge.targets` 复制到宿主输出，Release 不复制。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` 的工程清单与 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T16，再改本文件；Debug/Release 归并规则变化同步改 `PluginOutputMerge.targets` 与本文件。故障样本保留到 T17 验收结束，G4 装包后用发行清单取代这条 Debug-only 约定。
