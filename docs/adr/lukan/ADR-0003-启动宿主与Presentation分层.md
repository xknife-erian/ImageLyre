# ADR-0003：启动宿主与 Presentation 分层

| 项目 | 内容 |
| :--- | :--- |
| 编号 | ADR-0003 |
| 状态 | Accepted（2026-11-24 作者评审通过 `PLAN-0002` 后确认） |
| 日期 | 2026-11-24 |
| 作者 | lukan |
| 关联 | `ADR-0001` §2.4 L5、§2.10 单入口双宿主；`DESIGN-0002` 1.0；`PLAN-0002`；`docs/reviews/lukan/2026-11-24_启动编排模式专家辩论.md` |

## 1 背景与约束

M1 完成后，`ImageLyre.App` 的 `App.cs` 同时承担欢迎窗、Bootstrap 启动、槽位创建、取壳、Feature 附着、窗口显示、关停与三个冒烟钩子，而 `ImageLyre.Bootstrap` 只负责插件宿主；启动顺序写在 `ComposeAsync` 的方法体里，失败回滚与重复关停由手写的 `CompositionGate` 和零散分支保证。随着启动项增加，这条过程式链会让 App 继续膨胀，且新增启动项必须修改 App。

作者明确要求 App 只做入口、Bootstrap 编排启动项，并选择 Generic Host 作为宿主骨架；`DESIGN-0002` 的五席专家辩论与增长前提复核推荐 D 方案：Generic Host + Host/Presentation 两阶段 + 类型化 Step Pipeline + 嵌套 Plan。Web 复用不是本决策目标，不引入为假想宿主服务的抽象。

本决策受以下约束：单入口同时服务桌面与 CLI；插件启动完成前不得构造依赖插件产物的 UI 控件；所有控件在 Avalonia UI 线程构造；关停必须可等待完成；`Bootstrap` 保持 UI-free；既有 61 项测试、`verify-m1.ps1` 与 `smoke-m1.ps1` 不得回退。

## 2 决策

采用 D 方案，并把工程边界固定为：

1. `ImageLyre.App` 只保留进程入口与 Avalonia `Application` 生命周期转交：解析参数、固定 UTF-8、选择桌面/CLI、把桌面生命周期交给 Presentation 入口；不写启动项名称、顺序、槽位、取壳或窗口构造。
2. `ImageLyre.Bootstrap` 持有 Generic Host 构建器、`IBootstrapStep<TContext>` 与 `BootstrapPipeline<TContext>`、`HostPlan`、`StartupReport`、`ExitArbiter` 与插件复合 Step；引用闭包不含 Avalonia 与 AtomUI。
3. 新建 `ImageLyre.Presentation.Desktop`（L5）承载 `PresentationPlan`、欢迎窗、AtomUI 主题注册、槽位管理器、取壳、主窗体、冒烟回调与桌面关停；它单向引用 Bootstrap，Bootstrap 不引用它。
4. 启动分两个阶段：Host 阶段在任意线程，负责日志、DI、插件与宿主服务；Presentation 阶段在 Avalonia 消息循环启动后、UI 线程执行，负责欢迎窗关闭、槽位、取壳、附着与显示。欢迎窗只依赖本地化与启动参数，允许在 Host 阶段前显示。
5. 启动项是 `IBootstrapStep<TContext>`，带稳定 `Id`、`Group`、`Phase`、`CanRollback`、`ExecuteAsync` 与 `RollbackAsync`；结果至少区分 `Completed`、`Aborted`、`Failed`、`Degraded`；Plan 固定为“Plan → 组 → Step”两层，组内 5–9 项，组间顺序由组合根显式列出。
6. 插件启动是一个复合 Step，内部保留 `LogHub.Initialize` → `HasUiRoute`/扫描前缀 → `Launch` → `BuildServiceProvider` → `NotifyServiceProviderBuilt` → `StartAsync` 的固定顺序；单插件失败记 `Degraded`，不触发整链回滚。
7. 正常退出使用显式 `StopPlan`（释放壳 → 停插件 → 刷日志），不是启动链的机械逆序；`ExitArbiter` 保留 `TryStop` 先于 `TryBegin` 的语义与 64 并发测试，并提供唯一、可等待、带超时的 `TryStopAsync`。
8. 启动失败必须归属到 StepId 并写入 `StartupReport`；桌面停在失败界面，CLI 非零退出；回滚只针对已完成且持资源的 Step，按逆序执行，不可逆步骤显式标记并记录。

## 3 备选方案

| 方案 | 结论 | 理由 |
| :--- | :--- | :--- |
| C：Generic Host + 责任链 Handler | 保留为退路 | 顺序显式、实现成熟，但 Handler 上下文弱类型，仍需要 Presentation 工程；团队拒绝泛型 Step 时可退 |
| B：Generic Host + 每步 HostedService | 仅用于机制服务 | 注册顺序不可断言、无回滚契约、无阶段概念，不能作为产品启动项模型 |
| A：方法抽取 + 模板方法 | 仅作迁移前行为冻结 | 只把过程式代码换个位置，P1–P10 原样保留 |
| M5 中间件、M6 DAG、M7 Saga、M8 Mediator、M9 运行期注册表 | 不采用为主模型 | 当前线性顺序、回滚集与规模不支持其复杂度；M9 只允许在固定组内作可选声明，不能决定顺序 |

## 4 后果

正面后果是 App 退回入口体量，新增启动项只进入所属 Plan 组，启动顺序与失败归属可测试，关停不再被进程退出截断，宿主生命周期由 Generic Host 统一。代价是新增一个 L5 工程、第 8 套测试与一组 Step/Plan/Report 类型；`ImageLyre.Themes` 暂未立项，主题注册暂留在 Presentation 工程，待主题设计拍板后再决定是否独立。

对 `ADR-0001` 的补充是：§2.4 的 L5 行增加 `ImageLyre.Presentation.Desktop`，其依赖方向为 Presentation → Bootstrap/Contracts/Shell.Contracts/Avalonia/AtomUI；App → Presentation；Bootstrap 不反向依赖 Presentation。§2.10 的单入口双宿主不变，CLI 仍在 App 内，只是与桌面共用 HostPlan。

## 5 实施与验证

实施按 `PLAN-0002` 的 T01–T12 执行；验收以 `PLAN-0002` §5 的 A–H 为准，其中 App 入口纯度、Bootstrap 引用闭包、Step 顺序快照、失败注入、64 并发关停与 50 步增长压测为硬门槛。任何与本文冲突的实现先改本文或 `DESIGN-0002`，不在代码里另造第二套启动语义。
