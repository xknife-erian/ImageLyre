# ImageLyre.Shell.Contracts

## 意义

本工程承载**与界面有关的标识契约**：壳贡献的四个一级槽位名（`toolbar`、`viewport`、`panel`、`statusbar`）与主窗体标记接口。Feature 需要知道"往哪个槽位挂内容"时引用这里，从而不必引用壳插件工程；壳实现换掉（换布局、换 UI 路线）时，槽位名与标记接口保持不变。它只放字符串常量与空接口，不放控件、不放主题令牌、不放机制类型。

## 边界（不放什么）

- 不放 Avalonia 与 AtomUI 类型：本工程连 `Control` 都不出现，槽位名是纯字符串，窗口标记是空接口，实现约束写在注释里；
- 不放机制包：不引用 `Cangming.PicoPlugins`、`Cangming.Toolkit`，附着物词汇（`DockPaneAttachment` 一类带 `Control` 的类型）落在 `ImageLyre.Shell.Attachments`；
- 不放槽位拓扑与布局：一级槽位名只表达"区域角色"，不表达左右上下；位置属于壳的布局代码；
- 不放界面行为：菜单骨架、状态栏分段、语言切换入口都属于壳插件，本工程只提供它们引用的名字；`statusbar` 槽位只给插件贡献读数片段，M1 的五段占位留在壳里。

## 依赖方向

单向依赖 `ImageLyre.Contracts`；被 `ImageLyre.App`、壳插件 `ImageLyre.Module.Shell` 与各表达插件引用。反向引用一律禁止：不得引用任何插件工程，也不得让契约按某个壳实现的方便定制。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §4.2、§5.1、§5.2 与本文件，再改壳插件与引用方；新增一级槽位属于设计变更，必须在设计里先写清区域、承载类型、容量与顺序，再动代码。若变化涉及"界面契约是否允许引用 Avalonia"，先按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §2.4 与 §5.4 判定，不允许在实现里顺手开口子。
