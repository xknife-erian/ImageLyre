# ADR-0011：基座只取不复刻，应用层从零重建

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 0.1 | 2026-11-21 | 定稿 | lukan | 取代 `docs/adr/lukan/ADR-0008-self-built-base.md`：弃用自建底座的路线，基座能力一律取自沧溟包，应用层在新分支从零重建 |

## 1 背景与约束

现状事实由 `docs/analyses/lukan/2026-11-21_current-architecture-audit.md` 给出，其中三条直接决定本次取舍：其一，`src/` 下 6,446 行 C# 里约 5,000 行是外壳与基座复刻，产品内核（图像容器、校准、ROI、测量）为 0 行，而 M1 计划要求两周内交付"打开 ImageJ TIFF → 缩放 → ROI → 测量 → CSV"的垂直切片；其二，`src/Libs/` 的 61 个契约文件中有 44 个与沧溟同名，`Cangming.Shell` 与 `Cangming.Platform` 两条包引用只写在中央版本表里、从未被任何工程使用，而这两个包各自一行注册即可提供 15 项外壳服务与 15 项平台服务；其三，`docs/adr/lukan/ADR-0008-self-built-base.md` 的"基座自建"与 `docs/design/lukan/DESIGN-0002-cangming-engineering-base.md` §1.2 的"不自建能力域"同时有效，实现跟了前者、名义上挂后者，评审无法归因。

约束按优先级排列：M1 交付期不能移动，因为产品经理的计划不因架构重排而延期；产品内核必须在本里程碑内具备可对照的测量结果，这是"科研数据可比"承诺的第一个可验证落点；技术栈固定为 .NET 10 + AtomUI + Avalonia + SkiaSharp + SimdCodecs，界面堆栈不动；沧溟是自有上游仓（`E:\~InTheNearFuture5\cangming`），缺口可以向上游补，不存在第三方节奏不可控的问题。

## 2 决策

1. **基座唯一来源是沧溟**：外壳、平台、插件内核、工作区、数据、日志、工具库一律引用 `Cangming.*` 包，统一钉 0.1.8.1117 一条版本线；应用侧只写沧溟没有的实现，且必须实现沧溟的接口，不得新建同名或同义契约。
2. **应用层从零重建**：新分支上重建 `src/`，现有 21 个工程（`src` 18 个、`tests` 2 个、`tools` 1 个）整体退役，用 `git rm` 删除而非保留旁路；`docs/` 不退，需求、规格与计划是产品资产，与实现成败无关。
3. **产品内核先行**：M1 的代码重心放在 `ImageLyre.Kernel`（图像容器、校准、ROI 几何、测量、CSV），界面压缩到一个 `Workbench` 模块加一个产品 Feature，够用即止；M1 不做沙箱、厂商格式与 CLI，这些条款的验证阶段由 `docs/adr/lukan/ADR-0012-m1-baseline-applicability.md` 明确规定并已同步进基线 §10.3。
4. **缺口显式补齐而不是绕过**：沧溟只有契约没有实现的能力（菜单、热键分发、主题、文档管理、通知呈现），由应用侧实现其契约或登记临时机制与替换里程碑；同一缺口补完向上游提交，不在本仓长期分叉。
5. **反向禁止写成可测规则**：任何应用程序集里出现与 `Cangming.*` 同名的公开类型、任何 `AddCangming*Defaults` 已覆盖的能力出现自建实现，都按架构测试红牌处理，防止复刻再次发生；具体判据与 R1 至 R8 见 `docs/design/lukan/DESIGN-0006-rebuild-blueprint.md` §2.2。

## 3 备选方案

| 方案 | 结论与理由 |
| :--- | :--- |
| 在现有代码上增量改接沧溟 | 否：44 个同名契约与 9 个服务模块要逐个对齐，`ContentAttachment` 这类同名异义无法靠改名之外的任何手段消除歧义，改造面与重写相当且新旧混杂会把验证周期拉长一倍 |
| 维持 `docs/adr/lukan/ADR-0008-self-built-base.md` 的自建底座 | 否：自建要重写 30 项已实现服务并长期承担测试与文档成本，收益只是不受上游节奏影响，而沧溟是自有仓、上游改动可以由本仓提交推动；代价却是 M1 无法开工 |
| fork 沧溟并在 fork 上补缺口 | 否：缺口（菜单实现、热键分发、主题、文档管理器可见性）本可以提 PR，fork 把同步成本固定下来，与 ADR-0008 §2.5 的"不 fork、不 vendoring"一致地被否决 |
| 只删重复、保留外壳结构 | 否：删掉复刻的八个服务模块后，剩下的外壳仍要按沧溟契约重写槽位、菜单与启动链，等价于重建，却保留了旧结构惯性与双份文档 |

## 4 后果

正面后果是可计量的：外壳从约 5,000 行压到 1,500 行以内，产品内核从 0 行变成 M1 的全部代码重心，每个能力只有一个真源，将来接入沙箱与厂商解码时直接落在沧溟的 `Sandbox` 三件套上。

代价有三条，都必须承认：其一，接受沧溟的 `Experimental` 契约（全仓 94 处标注、其中 `Cangming.Shell.Contracts` 占 64 处），意味着升级时可能改签名，本仓的薄层要跟着改；具体到 M1 只消费 `IMenuItem` 的六个成员（`Header`、`Order`、`Group`、`Execute`、`IsEnabled`、`Children`），不碰 `Observe*` 流与 `CommandId`，把暴露面压到最小。其二，`IDocumentManager` 的默认实现是 `internal`，`IMenuService`、`IHotkeyManager`、`IThemeManager`、`INotificationService` 只有契约，M1 自写最小实现或登记临时机制，这部分代码是"补缺"而不是"自建底座"，判据是它实现的是沧溟的接口而非自造的新接口。其三，一次性迁移成本集中在首个提交里，旧的界面资产（主题令牌、截图工具）能复用但要重新接线。

**什么条件下这条决策要改**：沧溟某个能力域连续两个里程碑没有可用版本，或一次破坏性变更在一个里程碑内消化不掉，该域回本仓实现并另开 ADR 记账；反之，若沧溟把菜单、热键、主题、文档管理补齐到可用，本仓对应的自写实现随之删除，不留兼容层。ADR-0008 自此冻结为历史，其"自建底座"结论被本文件取代。

## 5 连带处置

本决策同时使 `docs/design/lukan/DESIGN-0002-cangming-engineering-base.md` 的类库规划、`docs/design/lukan/DESIGN-0003-libs-layering.md` 的工程分层与红线、`docs/design/lukan/DESIGN-0005-slot-plan.md` 的槽位规划失效，三份统一退役，目标形态由 `docs/design/lukan/DESIGN-0006-rebuild-blueprint.md` 承接；`docs/adr/lukan/ADR-0009-theme-token-landing.md` 与 `docs/adr/lukan/ADR-0010-theme-switch-mechanism.md` 关于主题令牌与切换机制的结论不依赖被退役的工程结构，继续有效，其令牌资产落点与 `IThemeManager` 归属的勘误登记在同批的 `docs/adr/lukan/ADR-0012-m1-baseline-applicability.md` 与目标设计 §3.4。
