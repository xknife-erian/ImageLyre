# ADR-0009：主题令牌落地：键空间、覆盖层与缩放

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 0.1 | 2026-11-20 | 已撤销 | lukan | 首版：`Lyre.*` 键空间、全局覆盖、缩放重算令牌 |
| 0.2 | 2026-11-20 | 历史 | lukan | 按三位独立评审的勘误重写：键组与段数闭合、覆盖改走 AtomUI 令牌 API、缩放改为物理换算并补线宽补偿 |
| 0.3 | 2026-11-20 | 定稿 | lukan | 复核轮收口：四档字号与行高逐键列出、字距三档给名、线宽并入 `Size` 去掉双源、补登记对 `spacing.md` §1 与 `color-tokens.md` §10.6 的覆盖、图标描边与线宽键分开 |

> 0.2 的修订依据：三位独立评审（基线冲突、可执行性、代价）在 2026-11-20 指出 0.1 有六处事实错误与三处自相矛盾，逐条证据见 `docs/analyses/lukan/2026-11-20_arch-review-loop.md`。0.1 的三条决策方向保留，实现机制与键形全部重写。

## 1 背景

设计侧已经把表现层定成可执行的值：亮 30 与暗 31 两套同构主题、18 个语义色（`surface` 5、`line` 3、`text` 4、`signal` 2、`state` 2、`canvas` 2）、四档字号（11/13/16/20）、六级间距（4/8/12/16/24/32）、五态五通道，并要求"组件里不出现裸数值"。而 `src/Libs/ImageLyre.Themes` 至今只有工程文件，一个令牌都没有。

三件事必须先定，否则令牌层无法开工：Avalonia 的资源键是字符串，谁来保证设计与实现引用同一个名字；AtomUI 自带令牌体系，覆盖放在哪一层；应用内缩放从 100% 到 200% 四档，按什么机制换算。

## 2 决策

### 2.1 键空间：`Lyre.<组>.<名…>`，组为闭集

- 第一段固定 `Lyre`，第二段是组，取值只有九个：`Surface`、`Line`、`Text`、`Signal`、`State`、`Canvas`、`Space`、`Size`、`Type`。前六个逐一对应 `color-tokens.md` §1 的六个前缀（`surface`、`line`、`text`、`signal`、`state`、`canvas`）；`Space` 与 `Size` 对应 `spacing.md`，线宽也落在 `Size` 而不另设组；`Type` 对应排印。
- 组之后保留设计键的点分结构，逐段转成 PascalCase，段数不固定但必须与设计键一一对应。设计写 `state.selected.bg`，代码就是 `Lyre.State.Selected.Bg`；设计写 `size.statusbar.height`，代码就是 `Lyre.Size.Statusbar.Height`。不做二次命名，不合并同义键。
- `Type` 组固定五个子维，顺序固定：`Family`、`Size`、`LineHeight`、`Weight`、`LetterSpacing`。字号按值建键，四档各一个键：`Lyre.Type.Size.S11`=11、`S13`=13、`S16`=16、`S20`=20，只此四个，不出现 `S12`、`S15` 一类中间值；角色名只用在样式里，不建键。
- 字重进资源：`Lyre.Type.Weight.Regular`=400、`Lyre.Type.Weight.SemiBold`=600，只有这两档，因为排印 §3 只定义这两档，且 `typography-avalonia.md` §4.1 已有对应键。
- 行高与字距的键名同样按值建，与字号一一对应：`Lyre.Type.LineHeight.L11`=16、`L13`=20、`L16`=24、`L20`=28（四档与字号一一对应，没有第五个值）；字距三档 `Lyre.Type.LetterSpacing.Tight`=−0.20、`Normal`=0、`Caps`=+1.10，数值固定写在键上，不新增第四档。
- 线宽键与设计侧 `size.hairline`、`ctrl.signal` 是映射关系而不是第二份值：键在这里定义一次，设计文档只提供来源名与用途。线宽归 `Size` 组，按设计键一一映射，只两个键：`Lyre.Size.Hairline`=1（对应 `size.hairline`）、`Lyre.Size.Signal`=2（对应 `ctrl.signal`）。后者是跨表映射：`ctrl.signal` 在 `controls.md` 里属控件尺寸表而不是间距表，此处按语义并入 `Size`，不适用"逐段同构"那条规则；不另建 `Lyre.Line.Width.*`，因为同一根线建两个键就是两份真源。`Line` 组只放线的颜色，不放粗细。`controls.md` §4 的图标描边 1.25 属矢量图形自带属性，由图标资源携带，不进线宽键，因此与"线宽只允许 1 与 2"不冲突。圆角固定 0，不建键；阴影不做，不建键。
- 类型：颜色给 `Color` 不给 `Brush`，`Brush` 与 `Thickness` 由壳在使用处用键组合生成。这条是把"值只有一个来源"收窄为"设计定义的数值只有一个来源"，组合产物不算新值。
- `warn` 不进组表：`color-tokens.md` 的 18 个键里没有 warn，而产品外观 §6.2 把警告归入信号色，因此两键暂挂 `Lyre.Signal.Warning.Background` 与 `Lyre.Signal.Warning.Line`，值标 `TODO(设计确认)`，要么设计补值，要么删。

### 2.2 覆盖层：走 AtomUI 6.2.15 自带的令牌 API

- 覆盖是**代码级**而不是资源级：AtomUI.Core 6.2.15 已提供 `DesignTokens`、`ThemeTokenResourceProvider`、`WithToken`、`WithInitialTheme`、`WithDefaultFontFamily`（包内实测确认）。A4 用这套 API 把十个字号位映射到四档，逐控件令牌只处理 A4 清单里的六个控件（按钮、输入、分段、复选框、菜单项、表格），清单之外的不预做，不靠改本仓资源键去影响 AtomUI 控件。
- 亮暗两套字典各一本：`Themes/Light.axaml` 与 `Themes/Dark.axaml`，键名完全相同，以 `ThemeDictionaries` 的 `Light`/`Dark` 两个变体键挂进同一父字典。
- `Application.RequestedThemeVariant` 是唯一开关，由外观模块写入；跟随系统写 `ThemeVariant.Default`。控件只引用 `Lyre.*`，切主题时框架按变体换值，控件树不重建。
- `IThemeManager` 现有契约（`RegisterTheme(name, uri)` 加 `ApplyTheme(name)`）是"按 URI 换整本字典"，与上面这套共存会造成两种机制。本次只登记冲突，不在本 ADR 改契约：改 `IThemeManager` 的提案另开 ADR-0010，在那之前外观模块只用 `RequestedThemeVariant`，不复用 URI 切换。
- 字号白名单的扫描对象是**源码**里的 `FontSize` 字面量，不是编译产物；通过条件是字面量 ∈ {11, 13, 16, 20}。缩放后的值只在运行期由换算产生，不参与静态扫描。
- 键集合断言必须能抓到同名键：Avalonia 对同一字典内的重复 `x:Key` 静默取后者、不报错，因此断言的判定里要加一条"同一字典内 `x:Key` 不得重复"，只比两套键集合是否相等是抓不到的。
- 字体家族与回退只在一个落点设置：`src/Apps/ImageLyre.App/Program.cs` 的 `BuildAvaloniaApp` 里，紧接 `UseCangmingAtomUI()` 之后一次性配置 `FontManagerOptions` 的 `DefaultFamilyName`、`FontFamilyMappings` 与 `FontFallbacks`（CJK 码段用 `UnicodeRange` 声明），同时用 AtomUI 的 `WithDefaultFontFamily` 保证控件跟随。两条家族栈写成 `Lyre.Type.Family.Ui` 与 `Lyre.Type.Family.Mono` 资源键，值是家族栈字符串；三平台族名以 `typography-avalonia.md` §3.2 与 §3.3 为准，其中 macOS 与 Linux 的族名标注为待实测。
- 既有裸值同批迁移：`src/Modules/ImageLyre.Module.Workbench/Internal/WorkbenchShell.cs` 里的 `1280×800`、`Brushes.LightGray`、`Thickness(8, 3)` 属 A2 与 A3 的迁移清单，迁完才允许把字面量扫描接入。

### 2.3 缩放：物理换算，不重算令牌

- 以 `spacing.md` §6 为准：有固定值的 `space.*`、`size.*`、字号与行高都是不缩放的 DIP 基准值，应用内缩放只做物理换算（DIP × 缩放），不是重新定 token。因此不重建字典、不重算键值，字典里永远只有基准值。
- 施加方式是在根级统一缩放，不用逐控件换算。缩放的输入只有应用内档位一个来源（由外观模块读设置），系统 DPI 由 `TopLevel.RenderScaling` 单独提供，两者相乘得到最终物理倍率。
- 线宽补偿：1px 发丝线与 2px 信号轨都必须保持物理像素，因此它们的厚度按 `1 / (RenderScaling × 应用缩放)` 反向换算后交给渲染层，令牌本身仍是 1 与 2。0.1 里"只有 1px 不随缩放重算"的说法是错的：`color-tokens.md` §6.3 与 `controls.md` §1 都要求 2px 信号轨缩放后仍为 2 物理像素，两者都要补偿。
- 本节口径与两处设计表述相抵，此处显式登记覆盖：`spacing.md` §1 写"1px 发丝线是唯一不随缩放重算的尺寸"，`color-tokens.md` §10.6 写"200% 下所有线仍是 1 DIP"；但同一批文档的 `color-tokens.md` §6.3 与 `controls.md` §1 要求 2px 信号轨缩放后仍是 2 物理像素。两者不能同时成立，取后者：1px 与 2px 都按物理像素补偿，请设计侧同批勘误前两处。
- 删除 0.1 中"整树变换会把文字按位图缩放、200% 下发虚"的断言：Avalonia 12.1.6 的 `LayoutTransformControl` 有独立的测量与排列，与 `RenderTransform` 不是同一种机制，这条论据不成立。文字在放大后是否清晰属实测项，见 §5。

## 3 后果

- 设计与实现引用同一套名字，改色改字号是改值不是改代码；键与设计文档的映射是机械的，可以逐条比对。
- 覆盖集中到 AtomUI 的令牌 API 与一份逐控件清单，升级时受影响的点是一处代码而不是几十个模板。
- 令牌保持基准值不缩放，字典只有两份且切换不重建控件树；代价是线宽要做反向补偿，补偿系数写在外观模块一处。
- `Themes` 因此不能放逻辑：缩放补偿、主题开关、跟随系统都在外观模块，主题包只承载资源与常量，否则无法整包替换。
- 字重、行高、字距进资源后，样式里不再写字号数值；四档字号成为可扫描的白名单。

## 4 备选方案

| 方案 | 结论 |
| :--- | :--- |
| 沿用 AtomUI 自带键，不设 `Lyre.*` | 否：设计与实现各说一套，改值要翻模板 |
| 每个控件单独钉字号与颜色 | 否：覆盖点随控件数增长，漏一处多一档 |
| 只用资源键覆盖 AtomUI 控件 | 否：AtomUI 的令牌是代码级注入，只改资源键不影响其控件 |
| 缩放重算令牌（0.1 的方案） | 否：与 `spacing.md` §6 的"不重算"正面冲突，且每次切档要重建字典 |
| 令牌放 `UI.Contracts` | 否：契约不放具体值，主题包才能整包替换 |

## 5 失效条件

1. 若实测发现线宽反向补偿在 150% 与 200% 下出现亚像素抖动或明显模糊，改为"按缩放重算线宽令牌"的局部方案；这属于换机制，另开 ADR 并回写本文。
2. 若文字在 200% 下清晰度不可接受，评估是否改为重算字号并放弃"不重算"口径；同样要另开 ADR，并同批改 `spacing.md` §6。
3. 若 AtomUI 升版移除或改名 `DesignTokens` 一类 API，覆盖层按新 API 重写，键空间不变。
4. 若出现第二个界面宿主，按 `docs/design/lukan/DESIGN-0003-libs-layering.md` §4 处理，本文不重复其条款。

## 6 与其它文档的关系

`docs/design/lukan/DESIGN-0004-architecture-principles.md` 的 S1 与 S2 是上位约束：S1 要求设计定义的数值只有一个来源，S2 要求结构先于皮肤；本文把两者落成键形、两个 `ThemeVariant` 与不重建控件树。

`docs/design/lukan/DESIGN-0003-libs-layering.md` §2.6 规定 `Themes` 只放资源与常量，本文 §3 与之对齐。

`docs/plans/lukan/PLAN-0002-architecture-rollout.md` 的 A2、A3、A4、A5 是本文的落地位；A2 的键表必须按 §2.1 的组闭集与段规则生成，A4 按 §2.2 的 AtomUI API 执行。

`docs/design/lukan/bakeoff-2026-11-20/typography-avalonia.md` §4.1 的 `Lyre.Typography.*` 键形与本文不符，且该文件自述仍是八档旧映射；以本文为准，A2 时同批把该文件改为四档与新键形。
