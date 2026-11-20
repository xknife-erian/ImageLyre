# ImageLyre 间距与栅格基线

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 1.0 | 2026-11-20 | 现行 | lukan | 4px 基线、六级间距、21/22 区域尺寸、行高与内边距；所有值落到语义 token |

## 0 三条前提

1. **4px 基线。** 垂直节奏只用 4 的整数倍；行高 16 / 20 / 24 / 28，区域与组件高度也是 4 的整数倍。唯一的非 4 倍数是 1px 发丝线与 78 的批次外高（76 内容 + 2 条 1px 边）。
2. **六级间距。** 间距、内边距、gap 只用 `4 / 8 / 12 / 16 / 24 / 32`。没有第七级，没有 5、7、10、18。
3. **每个值有名字。** 组件里不出现裸数值；任何一个 padding、gap、高度、宽度都必须能指回本节 token。找不到名字，就是错。

## 1 4px 基线

- 基线单位 `grid.base = 4`。
- 排版行高由排印 2.0 给：11/16、13/20、16/24、20/28；全部落在 4 的倍数。
- 区域高度与组件高度从行高推出：16 + 4 + 4 = 24；20 + 4 + 4 = 28；24 + 4 + 4 = 32。
- 水平方向以 4 对齐；区域外框允许 1px 边框，边框不参与 4px 节奏，不另设 2px、3px 边。
- **1px 发丝线**是唯一不随缩放重算的尺寸；其余尺寸都是 DIP，随界面缩放换算物理像素（见第 6 节）。
- 禁止用 line-height 凑高度、用 transform 平移 2px、用负 margin 补 1px。1px 只留给线。

## 2 间距阶

| Token | 值 | 语义名 | 用在哪 |
| :--- | :--- | :--- | :--- |
| `space.1` | 4 | `space.pair` | 标签与值之间的微距、图标与文字、缩略图与题注、按钮/字段/表格单元格上下 |
| `space.2` | 8 | `space.control` | 相邻控件、按钮组、画布内边、段标题与内容之间、批次内上下 |
| `space.3` | 12 | `space.inset` | 按钮左右 padding、菜单项左右、标题栏内元素间距、批次卡之间 |
| `space.4` | 16 | `space.panel` | 右栏内边距、标题栏左右、状态栏左右、批次左右、面板头与首段之间 |
| `space.5` | 24 | `space.section` | 面板段与段之间、状态栏项之间、面板内的大块分隔 |
| `space.6` | 32 | `space.major` | 顶层空态、命令面板分组、跨画布与档案的硬分隔；默认工作界面不出现，防止段距膨胀 |

- `space.pair` 只做配对微距，不承担段距；`space.section` 只做段距，不做行内 gap。
- 所有值都是设计 token；不写 `padding: 10px`、`gap: 18px` 这类“看着差不多”的数。
- `space.major` 在 21/22 的默认记录视图里不出现；它属于空态与命令面板这类顶层场景，不能拿来撑面板内部。

## 3 栅格与区域尺寸

### 3.1 区域尺寸

| Token | 值 | 类型 | 行为 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| `size.titlebar.height` | 44 | 固定高度 | 不拖 | 标题栏外高，含 1px 下边线；13/20 行高居中 |
| `size.menubar.height` | 32 | 固定高度 | 不拖 | 菜单栏外高，含 1px 下边线；13/20 行高居中 |
| `size.rail.width` | 44 | 固定宽度 | 不拖 | 左轨外宽，含 1px 右边线；44 = 4 + 36 + 4 |
| `size.panel.right` | 400 | 默认外宽 | **可拖宽** | 右栏默认 400；拖拽步长 8，夹在 320–560 |
| `size.panel.right.min` | 320 | 宽度下限 | 不拖 | 窗口不足时保底；画布让位，不缩字 |
| `size.panel.right.max` | 560 | 宽度上限 | 不拖 | 防止档案吞掉画布 |
| `size.panel.right.step` | 8 | 拖拽步长 | — | 每次移动 8，落在 4px 栅格上 |
| `size.batch.height` | 78 | 固定高度 | 不拖 | 批次外高：1 + 8 + 60 + 8 + 1；76 内容 + 2 条 1px 边 |
| `size.statusbar.height` | 32 | 固定外高 | 不拖 | 画布下沿读数带；11/16 行高 + 上下 8 |
| `size.canvas` | fluid | 流式区域 | 不拖 | 剩余空间重算；参与布局，不参与 4px 内栅格 |
| `size.tool` | 36 | 固定方形 | 不拖 | 左轨工具位；44 轨宽内 4 + 36 + 4 |
| `size.panel.head` | 32 | 固定高度 | 不拖 | 右栏面板头；16/24 行高 + 4 + 4 |
| `size.section.head` | 24 | 固定高度 | 不拖 | 段标题；11/16 行高 + 4 + 4 |
| `size.field.row` | 28 | 固定行高 | 不拖 | 13/20 行高 + 4 + 4；不含 1px 分隔线 |
| `size.field.label` | 64 | 固定列宽 | 不拖 | 字段标签列；右栏内容 64 + 8 + 1fr，不用 margin 推 |
| `size.table.head` | 24 | 固定行高 | 不拖 | 表头 11/16 + 4 + 4 |
| `size.table.row` | 28 | 固定行高 | 不拖 | 六项读数行；13/20 + 4 + 4 |
| `size.button.height` | 28 | 固定高度 | 不拖 | 13/20 + 4 + 4；左右 padding 12 |
| `size.thumb.card` | 56 × 60 | 固定卡 | 不拖 | 批次缩略卡；宽 8 + 40 + 8，高 40 + 4 + 16 |
| `size.thumb.image` | 40 × 40 | 固定图 | 不拖 | 缩略图本体；卡内左右各 8 |
| `size.hairline` | 1 | 固定线 | **不缩放** | 所有分隔线、边框、焦点线；1 物理像素，不是 1 DIP |

### 3.2 栅格

- 垂直：所有行高、段距、内边距落在 4 的倍数；面板头 32、段头 24、字段行 28、按钮 28、表头 24、表行 28。
- 水平：区域边落在 4；右栏在 320–560 之间以 8 为步长拖拽，默认 400；左轨 44；标题/菜单/状态不动。右栏只拖宽，不拖高；高度随窗口，内容滚动。
- 字段用两列：`size.field.label` 64 + `space.fieldGap` 8 + `1fr` 值列；**禁止**用 `margin-left` 把值推到“看起来对齐”的位置。右栏到 320 时字段列仍成立，值列变窄、必要时换行；不缩字号。
- 表格用等宽右对齐；单元格左右 `space.tablePadX = 8`、上下 `space.tablePadY = 4`；不手写列宽去对齐数字。
- 画布四周 `space.canvasInset = 8`；画布内部不做 4px 栅格，图像像素不受界面栅格影响。
- 画布 `size.canvas` 是唯一流式区域：不设固定值，不可拖；随窗口与右栏当前宽重算。批次条高度固定 78，不拖宽；卡片横向滚动。

## 4 行高与内边距表

| 部件 | 高度 / 尺寸 | 内边距 | Gap / 间距 | Token 落点 |
| :--- | :--- | :--- | :--- | :--- |
| 右栏面板 | 400 宽，纵向流 | 上下左右 16 | 段间 24 | `size.panel.right`、`space.panel`、`space.section` |
| 面板头 | 32 | 上下 4 | 头后 16 | `size.panel.head`、`space.pair`、`space.panelHeadGap` |
| 段头 | 24 | 上下 4 | 头后 8 | `size.section.head`、`space.pair`、`space.sectionHeadGap` |
| 字段行 | 28 | 上下 4 | 标签与值 8 | `size.field.row`、`space.fieldPadY`、`space.fieldGap` |
| 表格头 | 24 | 上下 4，左右 8 | — | `size.table.head`、`space.tablePadY`、`space.tablePadX` |
| 表格行 | 28 | 上下 4，左右 8 | — | `size.table.row`、`space.tablePadY`、`space.tablePadX` |
| 按钮 | 28 | 上下 4，左右 12 | 按钮间 8 | `size.button.height`、`space.buttonPadY`、`space.buttonPadX`、`space.buttonGap` |
| 批次 | 78 | 上下 8，左右 16 | 卡间 12 | `size.batch.height`、`space.batchPadY`、`space.batchPadX`、`space.batchGap` |
| 缩略卡 | 56 × 60 | 图 40；左右 8 | 图与题注 4 | `size.thumb.card`、`size.thumb.image`、`space.thumbGap` |
| 状态栏 | 32 | 上下 8，左右 16 | 项间 24 | `size.statusbar.height`、`space.statusPadY`、`space.statusPadX`、`space.statusGap` |
| 标题栏 | 44 | 上下 12，左右 16 | 元素间 12 | `size.titlebar.height`、`space.titlePadY`、`space.titlePadX`、`space.titleGap` |
| 菜单栏 | 32 | 不设垂直 padding（居中），左右 8 | 菜单项间 4 | `size.menubar.height`、`space.menuPadX`、`space.menuGap` |
| 左轨 | 44 宽 | 上下左右 4 | 工具间 4 | `size.rail.width`、`space.railPad`、`space.toolGap` |
| 画布内边 | — | 8 | — | `space.canvasInset` |

补充：

- 菜单项高度 28（`size.menuItem.height`），左右 12（`space.menuItemPadX`）；菜单栏 32 内垂直居中，不再加 6px padding。
- 状态栏 32 = 8 + 16 + 8；11/16 居中，上下 padding 8；1px 上边线含在 8 内。
- 标题栏 44 = 12 + 20 + 12；13/20 居中；1px 下边线含在 12 内。
- 批次 78 = 1px 上边 + 8 + 60 + 8 + 1px 下边；60 卡高 = 40 图 + 4 + 16 题注。
- 区域外尺寸（标题/菜单/左轨/右栏/批次/状态）含 1px 边线；组件高度（字段行/表头/表行/按钮/面板头/段头）不含 1px 分隔线。
- 1px 线是唯一例外：批次 78 含上下两条 1px 边（1 + 8 + 60 + 8 + 1），其余行高不为线加 padding。

## 5 语义 token 清单（逐值可查）

| Token | 值 | 用在哪 |
| :--- | :--- | :--- |
| `space.pair` | 4 | 图标/标签/题注微距 |
| `space.control` | 8 | 控件 gap、画布内边、段头 gap |
| `space.inset` | 12 | 按钮/菜单项左右、标题元素间距、批次卡 gap |
| `space.panel` | 16 | 右栏、标题、状态、批次左右、面板头后 |
| `space.section` | 24 | 面板段间距、状态项间距 |
| `space.major` | 32 | 空态/命令面板大分隔 |
| `size.titlebar.height` | 44 | 标题栏 |
| `size.menubar.height` | 32 | 菜单栏 |
| `size.rail.width` | 44 | 左轨 |
| `size.panel.right` | 400 | 右栏默认外宽 |
| `size.panel.right.min` | 320 | 右栏下限 |
| `size.panel.right.max` | 560 | 右栏上限 |
| `size.panel.right.step` | 8 | 右栏拖拽步长 |
| `size.batch.height` | 78 | 批次外高 |
| `size.statusbar.height` | 32 | 状态/读数带外高 |
| `size.canvas` | fluid | 画布流式区域 |
| `size.tool` | 36 | 工具位 |
| `size.panel.head` | 32 | 面板头 |
| `size.section.head` | 24 | 段头 |
| `size.field.row` | 28 | 字段行 |
| `size.field.label` | 64 | 字段标签列 |
| `size.table.head` | 24 | 表头 |
| `size.table.row` | 28 | 表行 |
| `size.button.height` | 28 | 按钮 |
| `size.menuItem.height` | 28 | 菜单项 |
| `size.thumb.card` | 56 × 60 | 缩略卡 |
| `size.thumb.image` | 40 × 40 | 缩略图 |
| `size.hairline` | 1 | 线/边/焦点 |

- 这里的每个值都有名字；组件里只允许引用这些名字或由它们算出的 `1fr`。

### 5.1 语义别名

别名只能指向 `space.1`–`space.6`，不得另造数值。

| 别名 | 值 | 等于 | 用在哪 |
| :--- | :--- | :--- | :--- |
| `space.panelHeadGap` | 16 | `space.panel` | 面板头与首段之间 |
| `space.sectionHeadGap` | 8 | `space.control` | 段头与内容之间 |
| `space.fieldGap` | 8 | `space.control` | 字段标签与值之间 |
| `space.fieldPadY` | 4 | `space.pair` | 字段行上下 |
| `space.tablePadX` | 8 | `space.control` | 表格单元格左右 |
| `space.tablePadY` | 4 | `space.pair` | 表格单元格上下 |
| `space.buttonPadX` | 12 | `space.inset` | 按钮左右 |
| `space.buttonPadY` | 4 | `space.pair` | 按钮上下 |
| `space.buttonGap` | 8 | `space.control` | 按钮之间 |
| `space.thumbGap` | 4 | `space.pair` | 缩略图与题注 |
| `space.batchPadY` | 8 | `space.control` | 批次上下 |
| `space.batchPadX` | 16 | `space.panel` | 批次左右 |
| `space.batchGap` | 12 | `space.inset` | 缩略卡之间 |
| `space.statusPadY` | 8 | `space.control` | 状态栏上下 |
| `space.statusPadX` | 16 | `space.panel` | 状态栏左右 |
| `space.statusGap` | 24 | `space.section` | 状态项之间 |
| `space.titlePadY` | 12 | `space.inset` | 标题栏上下留白 |
| `space.titlePadX` | 16 | `space.panel` | 标题栏左右 |
| `space.titleGap` | 12 | `space.inset` | 标题栏元素之间 |
| `space.menuPadX` | 8 | `space.control` | 菜单栏左右 |
| `space.menuItemPadX` | 12 | `space.inset` | 菜单项左右 |
| `space.menuGap` | 4 | `space.pair` | 菜单项之间 |
| `space.railPad` | 4 | `space.pair` | 左轨内边 |
| `space.toolGap` | 4 | `space.pair` | 工具位之间 |
| `space.canvasInset` | 8 | `space.control` | 画布四周 |

- 别名只允许出现在以上表内；出现表外名字，视为新造阶，拒绝。
- 右栏 400 与左轨 44、标题 44、批次 78、状态 32 是区域尺寸，不是间距；不能拿来当 gap/padding。

## 6 缩放：100–200%

界面缩放分为两层：**物理换算**与**逻辑重算**。

| 对象 | 100–200% 时 | 是否重算逻辑值 |
| :--- | :--- | :--- |
| 有固定值的 `space.*`、`size.*`、字号/行高 | DIP 值不变，物理像素 = DIP × 缩放 | 否 |
| 1px 发丝线 `size.hairline` | 始终 1 物理像素 | 否，且不随缩放变 2px |
| 画布宽高 | 由窗口减固定区域与右栏当前宽算出 | **是**，随窗口/右栏宽重算 |
| 右栏宽度 | 用户拖拽 320–560，步长 8；不随缩放重算 | 否（用户值）；物理宽 ×缩放 |
| 批次可见卡数、横向滚动 | 由批次可用宽算出 | **是** |
| 类型/间距的物理大小 | 11 DIP 在 200% 为 22 物理像素；44 为 88；400 为 800 | 物理换算，不是重新定 token |
| 图像缩放（画布 200%） | 只放大样本，不改界面 | 否 |

- **不重算**：标题 44、菜单 32、左轨 44、右栏默认 400、批次 78、状态 32，以及全部行高、间距、字号；它们在 100–200% 只做 DIP × 缩放的物理换算。**重算**的只有画布、右栏用户宽度夹取、批次可见卡数。

- 缩放 100 / 125 / 150 / 200% 时，**不把 4 改成 5、不把 8 改成 10、不把 12 改成 15**；设计值保持 DIP，渲染层做物理换算。
- 最小字号 11 DIP；200% 时最小 22 物理像素。任何面板都不得靠缩字或截断适配宽度。
- 窗口变窄时右栏先到 320 下限，画布让位；到 320 仍不足则字段换行、段内滚动，不缩字、不裁数字。
- 画布缩放与界面缩放是两件事；画布 200% 不改变 44 标题、400 右栏、78 批次、32 状态。

## 7 禁止

- 禁止任何不在 `4 / 8 / 12 / 16 / 24 / 32` 的 gap / padding / margin：5、7、10、18，以及 6、9、14、15、20 都不行。20 是字号，不是间距。
- 禁止用 margin 凑对齐：标签列用 64 + 8 的 grid，不用 `margin-left: 5px`、`margin-top: 3px`。
- 禁止用 padding 代替间距阶：段与段之间用 `space.section`，不要用 `padding-bottom: 18px` 撑出来。
- 禁止把区域尺寸当间距：44、78、400 只能做尺寸，不能做 gap。
- 禁止 2px/3px 边框、半像素、负 margin、transform 位移来“补”1–2px。
- 禁止用 line-height 改变行高：行高来自排印 2.0，组件高度来自本表。
- 禁止为“视觉居中”在 32 菜单里加 6px padding；固定区域用 flex 居中，不加非阶值。
- 禁止“看起来差不多”的近似值；找不到 token 就不写。
- 禁止截断数字、单位、读数；禁止靠缩字号解决空间不足。

## 8 验收

1. **4px 审计**：全屏搜索间距与高度，除 1px 线与 78 批次外，全部落在 4 的倍数。
2. **token 审计**：任意 padding/gap/宽度/高度都能指回第 5 节；出现裸数值即缺陷。
3. **区域审计**：标题 44、菜单 32、左轨 44、右栏 400、批次 78、状态 32；右栏可拖 320–560，步长 8；其余固定。
4. **字段审计**：右栏字段 64 + 8 + 1fr；拖动右栏时值列变化，标签列不漂移，不出现 margin 补位。
5. **缩放审计**：100% 与 200% 并排；字号、间距、区域物理尺寸按比例，1px 线仍 1px；无截断、无重叠。
6. **23/24 并排**：遮住颜色后，标题/菜单/左轨/右栏/批次/状态的行高与间距逐项一致。
