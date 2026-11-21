# ADR-0012：M1 分期对基线准入条款的适用与偏差登记

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 0.1 | 2026-11-21 | 定稿 | lukan | 与 `docs/adr/lukan/ADR-0011-base-adoption-rebuild.md` 同批：规定 `docs/spec/baseline/startup-requirements.md` §10.1 各条款的验证里程碑，登记 M1 的六项已知偏差、解除条件与责任边界；基线据此升 1.5 |

## 1 背景与约束

`docs/spec/baseline/startup-requirements.md` §10.1 是一份发布准入清单，条款本身按"最终产品"写就；`docs/spec/plans/PM-PLAN-0002-m1-vertical-slice.md` 则把 M1 收窄到 CORE-02 至 CORE-07 的一条垂直切片，并明确沙箱、厂商格式、插件生态属于 M2 及以后。两份文档同时有效，评审时按 §10.1 逐条核对就会得出"M1 蓝图不满足基线"的结论，而 §11 规定条款变更必须先出决策记录并递增版本，因此本次不再靠"排期解释"消化这个矛盾。

约束有三条：其一，M1 的交付期不变，沙箱与厂商解码塞进 M1 会挤掉内核这条主线，而内核是唯一能证明"科研数据可比"的资产；其二，基线条款的语义不能为了赶进度被改写，能改的只是"何时必须满足"；其三，`docs/adr/lukan/ADR-0006-cli-headless-entry.md` 仍然有效，M1 不建 CLI 工程不等于允许内核重新长出 UI 依赖。

## 2 决策

1. **§10.1 按里程碑适用，条款语义不动**：基线升 1.5（MINOR），新增 §10.3 的"验证里程碑"表，逐条指定该条款在 M1a、M1b、M2、M3 哪个阶段必须成立；条款文字、判定方式与"不得出现"清单一律不改，因此这属于适用性补充而非条款变更。
2. **CLI 与 headless 推迟到 M2，内核的无头能力提前到 M1**：`docs/adr/lukan/ADR-0006-cli-headless-entry.md` 结论不变，M1 不建 `ImageLyre.Cli` 工程；作为交换，`ImageLyre.Kernel` 必须以 R1 与"单测不启动 Avalonia 即可跑"这两条证明其无头能力，M2 建 CLI 时直接复用 Kernel 与测量引擎，不重写。
3. **六槽位与界面槽位分层解释**：基线 §6.3 的 IO、View、Operator、Measure、Script、System 是插件能力边界与依赖边界，`docs/design/lukan/DESIGN-0006-rebuild-blueprint.md` §3.2 的 Content、StatusBar 是沧溟 `ISlotManager` 的界面附着槽，两者不在同一层，因此两个界面槽位不构成对六槽位的削减；M1 由首方 Feature `ImageLyre.Feature.ImageLab` 覆盖 IO、View、Measure 三条边界，内部以 `IO/`、`View/`、`Measure/` 命名空间隔离，M2 拆分时以这三个命名空间为切点。
4. **首方 Feature 与第三方插件的 UI 边界**：基线 §4 规则 2 的"插件层不得直接操作 UI 控件"约束对象是随包分发的第三方插件与算子插件，首方 Feature 是应用组成单元、与本仓同批编译、不经过沙箱，允许引用 Avalonia 与 AtomUI；判据是分发方式与是否走沙箱，不是"是否叫 Feature"。第三方插件一律走六槽位与沙箱，不得持有控件。
5. **M1 的六项偏差登记在下表，逐条给出解除条件**：偏差不是豁免，每条都要在指定里程碑复核并消除，否则升为 blocking 缺陷。

| # | 偏差 | M1 现状 | 解除条件与里程碑 |
| :--- | :--- | :--- | :--- |
| 1 | 沙箱与 broker | 不建沙箱域，标准 TIFF 主进程解码 | 首个厂商格式需求出现时，M2 按 `docs/adr/lukan/ADR-0004-sandbox-pixel-data-path.md` 落地 |
| 2 | BioFormats 内置插件 | 不随包分发 | M2 与沙箱同批，按 `docs/adr/lukan/ADR-0005-bioformats-bundled.md` 默认启用可禁用 |
| 3 | 插件双运行模式 | 只有 UI 交互路径 | M2 建 CLI 时，同一切片经 Kernel 在无头路径复用，作为验收证据 |
| 4 | ROI `.roi`/`.zip` 互操作 | M1a 不做，M1b 随 ROI Manager 落地 | M1b 结束前完成双向读写与 fixture 对照 |
| 5 | 大图分块与懒加载 | 全量读入，边长上限 16384 像素，超出拒绝打开 | M1b 引入分块读取，解除上限 |
| 6 | sidecar 与 ImageJ 操作对照 | 只出 CSV 六列，不做 sidecar；菜单、工具栏、手势按最小集实现 | sidecar 在 M2 随 CORE-07 收口；操作对照在 M1b 输出偏差表 |

## 3 备选方案

| 方案 | 结论 |
| :--- | :--- |
| 把沙箱、BioFormats、CLI 全部塞进 M1 蓝图 | 否：九工程会膨胀到十五个以上，M1 的两周工期必然用于装配而不是内核，等于重复上一轮"外壳先行"的失败 |
| 不改基线，只在计划里口头说明 | 否：两份文档继续互斥，评审每次都要重新论证，违反"返工可归因"的要求 |
| 修改 §10.1 条款文字，把沙箱等降级为 P1 | 否：条款语义是产品承诺，删改会让 M1 的通过标准失去意义；适用性补充可以回退，删条款不能回退 |
| 新建 CLI 工程但留空占位 | 否：空工程没有验收价值，却要把 Kernel 的公开面提前定型，属于为未验证的需求预设计 |

## 4 后果

正面后果是 M1 的通过标准从此唯一：蓝图只需满足 §10.3 中标注为 M1a、M1b 的条款，评审不再对"为什么没有沙箱"反复争论。代价是基线进入 1.5 版本，将来若 M2 再次收窄，需要再出一次 ADR，而不能继续用本文覆盖。

**什么条件下这条决策要改**：M2 启动时若沙箱或 BioFormats 仍无可用计划，本文的偏差表第 1、2 条必须升级为阻断项并调整里程碑，不允许连续两个里程碑挂账；若 PRD 把 CORE-07 的 sidecar 提前到 M1b，第 6 条随之修改。基线 §10.3 的表格由本文维护，基线正文只保留指向本文的引用，避免两处真源。

## 5 连带勘误

`docs/adr/lukan/ADR-0009-theme-token-landing.md` 与 `docs/adr/lukan/ADR-0010-theme-switch-mechanism.md` 的令牌键空间、覆盖清单与"只认 `RequestedThemeVariant`"的结论继续有效，但两者引用的 `docs/design/lukan/DESIGN-0003-libs-layering.md` 已退役，且 `IThemeManager` 在旧结构中的位置 `ImageLyre.UI.Contracts.Theming` 在重建后不再存在。重建后的落点是令牌与样式资源全部进 `ImageLyre.Themes`、主题切换在 M1 只接 AtomUI 的 `IAtomUIThemeService` 并在 App 显式注册、`IThemeManager` 的实现推迟到 M2 并届时重开 ADR 登记，不保留旧位置的空壳；两份 ADR 正文按"定稿即冻结"不追改，勘误以本节为准。
