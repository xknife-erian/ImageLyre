# ImageLyre.Shell.Attachments

## 意义

本工程承载**壳怎样接住一个界面项**的应用词汇：`DockPaneAttachment` 把「一页面板」表达成机制能识别、渲染层能挂载、排序规则能比较的对象。它同时要实现 `Cangming.PicoPlugins` 的 `IFeatureAttachment`、`Cangming.Toolkit` 的 `IOrderable`，并携带 Avalonia 的 `Control`——这三件事决定了它既不能进机制包，也不能进契约层。

它与 cangming 路线包把 `ToolbarButtonAttachment`、`ContentAttachment` 放在 `Cangming.PicoPlugins.Avalonias` 是同一位置：基座只留机制，应用词汇归应用。本工程因此不叫 `.Contracts`，也不算契约层。

## 边界（不放什么）

- 不放槽位名与窗口标记：它们在 `ImageLyre.Shell.Contracts`，本工程引用它而不是复制常量；
- 不放壳的布局与渲染：本工程只描述"一页面板是什么"，`SlotPanel` 的覆写与挂载逻辑在壳插件工程里；
- 不放机制实现：只引用 `Cangming.PicoPlugins` 的附着物契约与 `Cangming.Toolkit` 的排序契约，不引用任何 cangming 实现包；
- 不放业务内容：内容控件由使用方传入，本工程不创建视图、不解析模板、不持有数据；
- 不放生命周期管理：M1 不提供运行期摘除，内容控件与窗口同生命周期。

## 依赖方向

引用 `Cangming.PicoPlugins`（`IFeatureAttachment`）、`Cangming.Toolkit`（`IOrderable`、`OrderRule`）、`Avalonia`（`Control`）与 `ImageLyre.Contracts`；被壳插件与各表达插件引用。不引用任何插件工程，也不引用 `ImageLyre.Shell.Contracts`——槽位名在那边，附着物与槽位名是两个正交的词汇。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§5.3 与本文件，再改壳插件与引用方；若要把某类附着物塞回契约层，必须先回答基线 §10.2 那条「契约层不得引用 UI 类型」，并按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §5.4 另开 ADR，不允许在实现里顺手开口子。
