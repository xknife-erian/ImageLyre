# 2026-11-20 架构评审收口

## 1 问题陈述

`docs/adr/lukan/ADR-0009-theme-token-landing.md` 0.1 于当日定稿，`docs/design/lukan/DESIGN-0004-architecture-principles.md` 与 `docs/plans/lukan/PLAN-0002-architecture-rollout.md` 同批成文。按 `docs/adr/lukan/ADR-0008-self-built-base.md` 与角色档案的评审机制，单人造不出第二双眼睛，因此由三个独立 subagent 各持一个角度做对抗评审，范围限定为可判定问题：与基线冲突、与既有决策冲突、决策是否可执行、代价是否过度。

三份总评：代价评审“有条件可以收口”，可执行性评审“不能收口”，基线冲突评审“不能收口”。两条“不能收口”都卡在同一处：ADR 对缩放的机制与设计基线正面相抵，键空间接不住 18 个语义色，A2 的键表与 A5 的验收因此都不成立。

## 2 证据

评审逐条给出的依据都能落到文件与章节，关键几条如下：

- 缩放：`docs/adr/lukan/ADR-0009-theme-token-landing.md` 0.1 §2.3 要求“字号乘缩放、间距乘缩放”，而 `docs/design/lukan/bakeoff-2026-11-20/spacing.md` §6 明写“有固定值的 `space.*`、`size.*`、字号与行高 DIP 值不变，只做物理换算，不是重新定 token”。
- 键形：0.1 §2.1 说三段，例子却给四段 `Lyre.Type.Family.UI`；组表缺 `state` 与 `canvas`，多出无值的 `Warn`；`typography-avalonia.md` §4.1 用的又是 `Lyre.Typography.*` 另一套。
- 颜色：`color-tokens.md` §2 至 §8 合计 18 个键（surface 5、line 3、text 4、signal 2、state 2、canvas 2），其中没有 warn。
- AtomUI：包内实测 `atomui.core/6.2.15/lib/net10.0/AtomUI.Core.dll` 存在 `DesignTokens`、`ThemeTokenResourceProvider`、`WithToken`、`WithInitialTheme`、`WithDefaultFontFamily`，故“等上游提供正式 API”的假设不成立。
- 变换：Avalonia 12.1.6 的 `LayoutTransformControl` 有独立测量与排列，0.1 把它与 `RenderTransform` 并称为“按位图缩放”是错的。

## 3 处置

| # | 问题 | 处置 | 落点 |
| :--- | :--- | :--- | :--- |
| 1 | 缩放机制与 `spacing.md` §6 冲突（三位评审同指） | 接受：改为物理换算，令牌保持不缩放的基准值，根级缩放施加，1px 与 2px 线按 `1/(RenderScaling × 应用缩放)` 反向补偿 | ADR-0009 §2.3 |
| 2 | 键形不闭合、与 `typography-avalonia.md` 不一致 | 接受：组改为闭集九组，组后照设计键逐段 PascalCase，`Type` 固定五个子维，行高与字距进键 | ADR-0009 §2.1 |
| 3 | 18 色无 warn，组表却列了 Warn | 接受：不设 Warn 组，两键挂 `Lyre.Signal.Warning.*` 并标 `TODO(设计确认)`；同批改 PLAN A2 | ADR-0009 §2.1、PLAN §2 A2 |
| 4 | 字重既说进资源又说不进 | 接受：进资源，只留 400 与 600 两档 | ADR-0009 §2.1 |
| 5 | 线宽、圆角、阴影没有决策依据，且设计明令不做圆角与投影 | 接受：线宽只留 `Hairline`=1 与 `Signal`=2，圆角固定 0 不建键，阴影删除 | ADR-0009 §2.1、§4 |
| 6 | 字号白名单的通过条件自相矛盾（四档 vs 四档乘缩放） | 接受：扫描对象是源码字面量，通过条件为 ∈ {11,13,16,20}，缩放值只在运行期产生 | ADR-0009 §2.2、PLAN §4.5 |
| 7 | 三个判定脚本在 A2 空转，“扫编译产物”不成立 | 接受：A2 只留“亮暗键集合相等且引用键存在”一项并进现有测试，其余推迟到首个屏幕；`keys.txt` 单一真源延后到有字典后再定 | PLAN §2 A2、§4.1 |
| 8 | 亮暗字典组织未写清，与 `IThemeManager` 的 URI 切换契约冲突 | 部分接受：组织方式写进 ADR；契约改动本身另开 ADR-0010，在那之前只用 `RequestedThemeVariant` | ADR-0009 §2.2、PLAN §7.2 |
| 9 | 字体回退落点未写 | 接受：写明只在 `Program.BuildAvaloniaApp` 设置 `FontManagerOptions`，并点名 `WithDefaultFontFamily` | ADR-0009 §2.2 |
| 10 | AtomUI 已有令牌 API、`LayoutTransform` 断言错误 | 接受：改为事实陈述并实测确认；删除错误断言 | ADR-0009 §2.2、§2.3、§4 |
| 11 | S1 与“壳现场生成 Brush/Thickness”冲突 | 接受：S1 收窄为“设计定义的数值只有一个来源”，组合产物不算新值 | DESIGN-0004 §3 |
| 12 | S6 属文档纪律、S2/S3/S5 当前无法自动判定 | 接受：S6 移出不变式列表，S2、S3、S5 降级为人工清单并登记判定人与自动化时点 | DESIGN-0004 §3、§9 |
| 13 | S5“不做动效”与 PRD §4.3、产品外观 §6.5 冲突 | 接受：收窄为“不做装饰性动效”，为解释空间关系的动效留出准入 | DESIGN-0004 §3 |
| 14 | ADR §5.2 预写未实测的混合缩放、§5.3 第二宿主条款重复 | 接受：§5.2 降为待实测项，§5.3 改为指向 DESIGN-0003 §4 | ADR-0009 §5 |
| 15 | 覆盖清单既漏又过（缺分段、复选框、菜单项；列表与标签无规格） | 接受：改为按钮、输入、分段、复选框、菜单项、表格六个，先做单控件六态试点 | PLAN §2 A4 |
| 16 | 计划计数失真（写十七步，实为二十五步） | 接受：按实际重列，不再写总数；A1 转入前置，B2 并入 B1 | PLAN §2 |
| 17 | 为 M1 之后的能力预付（会话、脏标记、关闭守卫、历史、导出、Agent） | 接受：C4 收窄为 M1 最小撤销，其余推迟并只保留接口 | PLAN §2 C、§2 D |
| 18 | 设计文档自身数据冲突：`size.statusbar.height` 32 与 28 | 转出：不属架构可裁，登记为设计侧勘误请求 | PLAN §7.5 |

驳回 0 条。延后 3 条（`IThemeManager` 契约改动、`keys.txt` 单一真源、缩放补偿系数的具体数值），都在 PLAN §7 留了复核点。

## 4 结论与遗留

评审的核心价值不在挑错，而在证明 0.1 犯了两类错：把机制写得比依据更肯定（缩放、`LayoutTransform`、AtomUI API），以及把键形写得比设计文档更随意（组不闭合、四段与三段并存）。两处都属于“结论可被推翻”这条判据要求写清依据而我写得太快。

三个文件已按上表修订：`docs/adr/lukan/ADR-0009-theme-token-landing.md` 到 0.2，`docs/design/lukan/DESIGN-0004-architecture-principles.md` 到 0.2，`docs/plans/lukan/PLAN-0002-architecture-rollout.md` 到 0.2。

遗留三条：其一，`IThemeManager` 与 `RequestedThemeVariant` 两套机制并存的窗口期有多长，取决于 ADR-0010 何时写；其二，缩放补偿的实际系数与文字清晰度要等 A5 实测，在那之前任何数字都是猜；其三，设计侧两处勘误（状态栏高度、`typography-avalonia.md` 的八档键形）不解决，A3 的键表会有一处与设计稿不一致。
