# 2026-11-21 现有应用层架构审查

> 本文件只陈述事实与证据，回答"现有代码长什么样、与沧溟基座的关系如何"；由此得出的决策见 `docs/adr/lukan/ADR-0011-base-adoption-rebuild.md` 与 `docs/adr/lukan/ADR-0012-m1-baseline-applicability.md`，目标形态见 `docs/design/lukan/DESIGN-0006-rebuild-blueprint.md`。

## 1 问题陈述

业主判定当前应用层架构不合格，理由是它用了沧溟基座却没有领会基座能力，并决定重构、必要时从零重建。审查要回答三个可判定问题：现有代码与沧溟基座是什么关系、重复的部分有多少、产品内核是否已经开工。

## 2 证据

### 2.1 规模与构成

| 项 | 数值 | 说明 |
| :--- | :--- | :--- |
| `src/` 下 C# 文件 | 90 个 | 不含 `bin`、`obj` |
| `src/` 下 C# 行数 | 6,446 行 | 同上 |
| `tests/` 下 C# 文件 | 18 个 | 架构测试 8 个、插件测试 10 个 |
| `tests/` 下 C# 行数 | 1,281 行 | 同上 |
| 工程数 | 21 个 | `src` 18、`tests` 2、`tools` 1 |
| 产品内核类型 | 0 个 | 全量搜索 `ImageContainer`、`Calibration`、ROI 类与测量类，命中 0 |

产品内核一行未写，而 M1 计划（`docs/spec/plans/PM-PLAN-0002-m1-vertical-slice.md`）要求两周内打通"打开 ImageJ TIFF → 缩放 → ROI → 测量 → CSV"，因此当前代码全部是外壳与装配成本，没有一个字节直接服务验收条款。

### 2.2 契约被复刻

`src/Libs/` 下 61 个 C# 文件里，有 44 个的文件名与沧溟源码中的类型文件名逐字相同，例如 `ICommandRegistry.cs`、`IDocumentDirtyTracking.cs`、`IBusyManager.cs`、`IPreferencesManager.cs`、`IEventAggregator.cs`。抽样比对五组同名文件后确认它们不是同源引用，而是手工抄写并删减注释的副本：`IEventAggregator` 900 字符对沧溟 993 字符，`IDocumentDirtyTracking` 1537 对 1683，`IPreferencesManager` 2251 对 2332，`IBackgroundTaskManager` 4788 对 4941，`IBusyManager` 2681 对 2776。

`Directory.Packages.props` 里列了 `Cangming.Shell`、`Cangming.Shell.Contracts`、`Cangming.Platform`、`Cangming.Platform.Contracts`、`Cangming.Workspace`、`Cangming.Data.Sqlite` 六条版本，但 21 个工程里没有任何一个引用它们；本机 NuGet 缓存也从未还原过 `cangming.shell*` 与 `cangming.platform*`，说明这些包自始至终没有被真正使用。与之相对，沧溟的 `AddCangmingShellDefaults()` 一次注册 15 项外壳服务，`AddCangmingPlatformDefaults()` 一次注册 15 项平台服务，两行调用即可替代下文 2.3 的全部模块。

### 2.3 服务模块重造

`src/Modules/` 下八个模块各自实现一份沧溟已有的能力，合计 1,033 行：

| 模块 | 行数 | 与沧溟对照 |
| :--- | :--- | :--- |
| `ImageLyre.Module.Workbench` | 690 | 窗口与槽位渲染属应有自写，但同批重写了 `SlotRegistry` 与菜单投影 |
| `ImageLyre.Module.Preferences` | 201 | 对应 `IPreferencesManager`，沧溟有 `InMemoryPreferencesManager` |
| `ImageLyre.Module.Notifications` | 193 | 对应 `INotificationCenter` 与 `IOutputChannelManager`，沧溟两者都有默认实现 |
| `ImageLyre.Module.Busy` | 134 | 对应 `IBusyManager`，沧溟有 `InMemoryBusyManager` 与 `BusyHandle` |
| `ImageLyre.Module.Documents` | 117 | 对应撤销与文档会话，沧溟有 `InMemoryDocumentSessionPersistence` 与 `InMemoryMostRecentlyUsedDocuments` |
| `ImageLyre.Module.Files` | 113 | 对应 `IExternalChangeNotifier`，沧溟有 `FileSystemWatcherChangeNotifier` |
| `ImageLyre.Module.PluginCenter` | 102 | 自建插件管理面，沧溟 `IPluginManager` 已给同一能力 |
| `ImageLyre.Module.Commands` | 100 | 对应 `ICommandRegistry` 与 `IContextKeyService`，沧溟两者都有默认实现 |

配套测试 43 条（`tests/ImageLyre.Plugins.Tests/`）全部用于验证这些复刻实现，等于把同一份行为测了两遍。

### 2.4 槽位双轨

沧溟提供 `ISlotManager`、`SlotManager`、`IAttachmentRegistry`、`FeatureAttachmentExtensions` 与 Avalonia 渲染宿主 `SlotPanel`，并说明槽位名与所有者由应用定义。本仓另建一套词汇：`ISlotRegistry`、`SlotDescriptor`、`SlotKind`、`SlotCapacity`、`SlotSnapshot`、`SlotIssue`、`SlotHost` 与 356 行的 `SlotRegistry`，再让 `SlotRegistry` 同时实现沧溟的 `ISlotManager` 以便复用 `SlotPanel`。

拆开看，其中只有一条需求是沧溟确实缺的：框架没有 detach API，`FeatureAttachmentExtensions` 的注释也自认运行期禁用不会摘除已挂附着物。补这条缺口的正确形状是在沧溟契约之上加一层按所有者记账的装饰器（Mercury 的做法是 `PluginSlotManager`），而不是另造一套槽位词汇与诊断模型；现状把"缺口扩展"做成了"平行实现"，于是 `SlotHost` 与 `SlotPanel` 职责重叠、`GetSlotContents` 手写排序而没有走沧溟的 `SortBySpecifiedOrder`。

### 2.5 同名异义

最危险的不是多写代码，而是同名类型语义不同。本仓的 `ImageLyre.UI.Contracts.Attachments.ContentAttachment` 持 `Func<Control>`，沧溟 `Cangming.PicoPlugins.Avalonias.Core.ContentAttachment` 持 `Control`；`StatusBarAttachment` 同样各有一份。读代码的人看到相同类型名会以为在说同一件事，而两者的构造、生命周期与渲染路径都不同，这类分叉一旦进入插件生态就无法合并。

### 2.6 文档自相矛盾

`docs/adr/lukan/ADR-0008-self-built-base.md` 定"基座在本仓自建，沧溟只作参考"；`docs/design/lukan/DESIGN-0002-cangming-engineering-base.md` §1.2 却写"不自建能力域，插件内核、平台、外壳、日志、工作区、数据访问与工具库一律来自沧溟"。两份同时有效，实现跟了前者、名义上挂着后者，评审时无论引用哪一份都能自证正确，这正是角色档案里"返工可归因"判据要防的情形。

### 2.7 细节缺陷

外壳代码本身也有可判定的问题：`WorkbenchShell.OnSlotChanged` 里的分支体是重复初始化忙状态遮罩的死代码；`SlotRegistry.CompleteRegistration()` 永久置 `closed`，之后运行期启停产生的附着全被记为 `AttachedAfterClose`，插件中心刷新一次就变吵；`WorkbenchShell` 每次槽位变化重建整棵菜单；`PreloadPluginAssemblies` 用 `Assembly.LoadFrom` 按前缀扫目录，而样例 Studio 的做法是让插件工程作为 `ProjectReference` 自然落进输出目录，不需要预加载。

## 3 候选方案对比

| 方案 | 改造面 | 主要风险 | 与 M1 工期的关系 | 结论 |
| :--- | :--- | :--- | :--- | :--- |
| 增量改接沧溟 | 44 个同名契约与 9 个模块逐个对齐 | 同名异义无法消除，双真源长期并存 | 改造期与验证期叠加，M1 必然延期 | 否 |
| 维持自建底座 | 继续补 30 项服务的测试与文档 | 与 M1 内核争夺工期 | 内核继续 0 行 | 否 |
| fork 沧溟 | 分叉同步成本固定 | 上游修复合不进来 | 不影响工期但长期负债 | 否 |
| 只删重复、保留外壳结构 | 删 8 个模块后重写外壳 | 旧结构惯性、双份文档 | 等价于重建但更慢 | 否 |
| 从零重建九工程 | 一次性删除 21 个工程、按蓝图新建 | 首个提交较大，需要 fixture 才能验收测量 | 内核可以立即开工 | 是 |

## 4 结论

现有代码的失败不是"写得差"，而是**把基座又写了一遍**：6,446 行里约 5,000 行是沧溟已提供或本可由两行注册替代的外壳能力，产品内核 0 行；两套契约并存使后续任何一次对接都要写适配，M1 无法在两周内开工。据此可判定该层不具备增量修复价值，决策理由与替代方案见 `docs/adr/lukan/ADR-0011-base-adoption-rebuild.md`。

## 5 遗留

本审查没有覆盖沧溟上游的质量风险：`Cangming.Shell.Contracts` 中 64 处接口标记 `Experimental`（全仓 94 处），`IDocumentManager` 的默认实现是 `internal`、外部不可注册，`IMenuService`、`IHotkeyManager`、`IThemeManager`、`INotificationService` 只有契约没有实现。这些缺口是重建方案必须正面回答的部分，已在 `docs/design/lukan/DESIGN-0006-rebuild-blueprint.md` §3.4 逐条给出落点。
