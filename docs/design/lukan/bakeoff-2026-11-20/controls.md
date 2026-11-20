# ImageLyre 常用控件规格

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 1.0 | 2026-11-20 | 现行 | 林一 | 以 30 为基准：逐个控件给尺寸、内边距、字档、五态、键盘与 do/don't；尺寸只引 spacing，颜色只引 color-tokens 1.2 |

## 0 读法

- **基准**：稿 30（亮）是当前视觉基准，见 `product-appearance.md` §12–13；暗色按同一结构、同一 token 名单走。
- **三个来源**：尺寸与间距只引 `spacing.md` 的 token；字号、字重、行高只引 `typography.md` 2.0 的四档；颜色只引 `color-tokens.md` 1.2 的语义名。
- **不新造**：不新增字号、间距阶、色值与状态数。spacing 没有命名的组件尺寸（图标 16、滚动条 8、滑轨 4、手柄 16、步进 24、开关轨道 40×24）在 §1 登记为 `ctrl.*`，值全部落在 4 的倍数上，不引入 3、5、7、10、14、18。
- **选中不是一块色，是一条线**：横向控件贴下沿 2px，纵向列表贴左沿 2px，画面对象沿形描边；`signal` 只做这两件事，任何控件都不许拿它铺底，也不许反白。
- **五态**：默认 / 悬停 / 按下或选中 / 焦点 / 禁用。按钮没有选中态，用「按下」；分段、页签、列表、开关用「选中」。
- **焦点环**：1px `line.focus`，画在控件外 1px，控件尺寸不变；它是线，不是光晕。

## 1 控件尺寸（token 落点）

| Token | 值 | 来源 | 用在哪 |
| :--- | :--- | :--- | :--- |
| `ctrl.button.height` | 28 | `size.button.height` | 主/次/静默按钮、图标按钮外的所有按钮 |
| `ctrl.button.padX` | 12 | `space.buttonPadX` | 按钮左右 |
| `ctrl.button.padY` | 4 | `space.buttonPadY` | 按钮上下 |
| `ctrl.button.gap` | 8 | `space.buttonGap` | 按钮之间、图标与文字 |
| `ctrl.icon` | 16 | `space.4`（图标尺寸，不是内边） | 工具图标、输入框内图标、按钮内图标 |
| `ctrl.tool` | 36 | `size.tool` | 工具格、图标按钮的方形边 |
| `ctrl.key` | 11 | `fs-11` | 工具格右下键位、快捷键提示 |
| `ctrl.field.height` | 28 | `size.field.row` | 文本框、数值框、搜索框 |
| `ctrl.field.padX` | 8 | `space.control` | 输入框左右内边 |
| `ctrl.seg.height` | 28 | `size.button.height` | 分段控件外高 |
| `ctrl.seg.padX` | 12 | `space.inset` | 分段项左右 |
| `ctrl.check.box` | 16 | `space.4` | 复选框方框 |
| `ctrl.switch.track` | 40 × 24 | `size.tool + space.1` / `space.5` | 开关轨道；手柄 16 = `space.4`，内缩 4 = `space.1` |
| `ctrl.menu.item` | 28 | `size.menuItem.height` | 菜单项、下拉项 |
| `ctrl.menu.padX` | 12 | `space.menuItemPadX` | 菜单项左右 |
| `ctrl.table.head` | 24 | `size.table.head` | 表头 |
| `ctrl.table.row` | 28 | `size.table.row` | 表行 |
| `ctrl.table.padX` | 8 | `space.tablePadX` | 单元格左右 |
| `ctrl.scrollbar` | 8 | `space.2` | 滚动条宽、滑块手柄行程边距 |
| `ctrl.slider.track` | 4 | `space.1` | 滑轨高 |
| `ctrl.slider.handle` | 16 | `space.4` | 滑块手柄 |
| `ctrl.stepper` | 24 | `space.5` | 步进按钮宽（高 28 = `ctrl.field.height`） |
| `ctrl.tooltip.padY` | 4 | `space.1` | 工具提示上下 |
| `ctrl.tooltip.padX` | 8 | `space.2` | 工具提示左右 |
| `ctrl.tooltip.max` | 320 | `size.panel.right.min` | 工具提示最大宽，超出折行 |
| `ctrl.signal` | 2 | `color-tokens` §6.3 | 选中标线宽；不随缩放变粗 |

## 2 五态通则

| 态 | 底 | 线 | 字 | 标 |
| :--- | :--- | :--- | :--- | :--- |
| 默认 | `surface.panel`（或 `surface.input`） | `line.border` | `text.t1` | 无 |
| 悬停 | `surface.hover` | 不变 | 不变 | 无 |
| 按下 | `state.selected.bg` | 不变 | `text.t1` | 无（按下是瞬时态） |
| 选中 | `state.selected.bg` | 不变 | `text.t1` | 下沿或左沿 2px `signal.active` |
| 焦点 | 不变 | `line.focus` 1px，外偏 1px | 不变 | 无 |
| 禁用 | `state.disabled.bg` | `line.divider` | `text.disabled` | 去掉标 |

- 悬停只换底，不换线、不换字重；禁用项没有悬停。
- 选中与按下共用同一个底，区别只在有没有 2px 标；一个控件不能既放下沿又放左沿。
- 复选框是唯一例外：它的「选中」用 2px `signal` 勾画在 16px 方框里，不放下沿（见 §7）；这是勾，不是第二条线。
- `signal` 只做 2px 的线与勾：**不做文字色、不做填充、不做整块底**；选中行的字仍是 T1，数字不会变红。
- 焦点只画在控件本身：表格画在当前单元格**内侧** 1px，不给整行、整块或整张表套外框。
- 禁用时一定要把信号标去掉——禁用控件里没有「当前」。
- 五态都不出现深色实底反白；暗色下选中底比面板亮一档。

## 3 按钮

三档：主、次、静默。三档同高同字，差别只在**线**与**字色**：主 = 实线 + T1，次 = 细线 + T2，静默 = 无线 + T2。主按钮也不用 `signal`，也不反白。

**规格**

| 项 | 主 | 次 | 静默 |
| :--- | :--- | :--- | :--- |
| 高 | 28 | 28 | 28 |
| 左右内边 | 12 | 12 | 8 |
| 字档 | 13 / 20；拉丁 600、中文 400 | 13 / 20；中文 400 | 13 / 20；中文 400 |
| 底 | `surface.panel` | 透明（露出所在底） | 透明 |
| 线 | 1px `line.border` | 1px `line.divider` | 无 |
| 字色 | `text.t1` | `text.t2` | `text.t2` |
| 图标 | 16，与字 gap 8 | 同左 | 同左 |
| 数量 | 一屏一处，只给本屏主动作 | 常规动作 | 低频与面板内联动作 |

**五态**（三档同表；悬停与按下只换底，不换线、不换字色）

| 态 | 底 | 线 | 字 |
| :--- | :--- | :--- | :--- |
| 默认 | 见上表 | 见上表 | 见上表 |
| 悬停 | `surface.hover` | 不变（仍是本档的线） | 不变（仍是本档的字色） |
| 按下 | `state.selected.bg` | 不变 | 不变 |
| 焦点 | 不变 | 外加 `line.focus` 1px（外偏 1px），原线保留 | 不变 |
| 禁用 | `state.disabled.bg` | `line.divider` | `text.disabled` |

- 三档的分层只发生在默认态与焦点态；悬停、按下如果改线或改字色，三档会塌成一档，因此明令不动。
- 禁用时三档同表：底 `state.disabled.bg`、线 `line.divider`、字 `text.disabled`；这是唯一允许三档同形的一态。

**键盘**：Tab 进入，Enter 或 Space 触发；同一组按钮内 ←/→ 或 ↑/↓ 移动焦点；Esc 取消正在进行且可取消的动作；快捷键写在按钮右侧，用 11 mono，如 `Ctrl M`。

**do / don't**

- do：一个动作一个动词，中文 400、拉丁 600；宽度随字长，最短 72。
- do：主按钮靠「实线 + T1 + 拉丁 600」立起来，不靠颜色、不靠加粗中文。
- do：三档同屏时按「一主、一次、其余静默」排，不出现两个主按钮。
- don't：不用 `signal` 铺底、不做反白、不做圆角、不加投影。
- don't：不要把次按钮做成「主按钮的浅色版」——它们差的是线的层级（`line.border` vs `line.divider`），不是透明度。
- don't：按钮里不换行、不截断、不放两个动词（「保存并导出」拆两个）。

## 4 图标按钮与工具格

同一枚 36 方控件，两种用法：纯图标按钮（面板头、窗口控件），工具格（左轨，带键位）。

**规格**

| 项 | 值 |
| :--- | :--- |
| 外框 | 36 × 36 |
| 图标 | 16 × 16 居中，描边 1.25 |
| 键位 | 11 mono，右下角，距右 4、距下 2 |
| 内边 | 0（图标居中，不用 padding 推） |
| 工具间距 | 4（`space.toolGap`） |

**五态**

| 态 | 底 | 图标 | 标 |
| :--- | :--- | :--- | :--- |
| 默认 | 透明 | `text.t2` | 无 |
| 悬停 | `surface.hover` | `text.t1` | 无 |
| 选中 | `state.selected.bg` | `text.t1` | 下沿 2px `signal.active` |
| 焦点 | 不变 | 不变 | `line.focus` 外偏 1px |
| 禁用 | 透明 | `text.disabled` | 去掉标 |

**键盘**：Tab 进组，方向键在组内移动（roving tabindex），Space 或 Enter 激活；单键快捷键（V / R / P / H / Z）直接切工具，不抢输入框焦点。

**do / don't**

- do：图标永远配 tooltip（11 字），tooltip 里重复键位。
- do：键位用 11 mono，不用大写字母加粗。
- don't：不做 40、44 的工具格；不做圆角方块；选中不用深色实底反白。
- don't：同一组里混用有图标和无图标。

## 5 文本、数值、搜索

**规格**

| 项 | 文本框 | 数值框（带单位） | 搜索 / 命令 |
| :--- | :--- | :--- | :--- |
| 高 | 28 | 28 | 28 |
| 内边 | 左右 8、上下 0 | 左右 8、上下 0 | 左右 8 |
| 字档 | 13 / 20 | 13 / 20，等宽 | 13 / 20 |
| 底 / 线 | `surface.input` / `line.border` | 同左 | 同左 |
| 结构 | 纯值 | 值（右对齐）+ 1px 分隔 + 单位 11 `text.t2` | 16 图标 + 值 + 右侧 `Ctrl K` 11 `text.t3` |
| 单位 | — | 可点，就地换单位，不开对话框 | — |
| 数值对齐 | — | 等宽、右对齐、固定小数位，不截断 | — |

**五态**

| 态 | 底 | 线 | 字 |
| :--- | :--- | :--- | :--- |
| 默认 | `surface.input` | `line.border` | `text.t1`；占位 `text.t3` |
| 悬停 | `surface.hover` | `line.border` | 不变 |
| 选中（文本选区） | 不变 | 不变 | 选区用 `state.selected.bg`，不反白 |
| 焦点 | 不变 | `line.focus`，外偏 1px | 不变 |
| 禁用 | `state.disabled.bg` | `line.divider` | `text.disabled` |

**键盘**：Tab 进入；Enter 提交并退出；Esc 还原为进入前的值；数值框 ↑/↓ ±1、Shift+↑/↓ ±10、PageUp/PageDown ±10；进入数值框全选，方便直接覆盖；单位后缀用 Alt+↓ 展开就地菜单，↑/↓ 换单位、Enter 确认、Esc 取消；搜索框 `Ctrl K` 聚焦，Esc 清空并退出。

**do / don't**

- do：数字与单位之间一个不换行空格；单位不进数字列，单独 11 档。
- do：校验失败时就地给 1px `line.focus` + 11 字说明，不弹窗。
- don't：不用 placeholder 当标签；标签在字段左侧或上方。
- don't：不把单位做成第二个输入框；不截断小数。

## 6 分段控件

横向单选，一屏内的视图切换。测量单位只有两格：**µm² / px²**；同型控件还用于 1 位/2 位、浅/深。

**规格**：外高 28；项左右 12；字 13 / 20；外框 1px `line.border`；项间 1px `line.divider`；底 `surface.panel`。

**五态**

| 态 | 底 | 线 | 字 | 标 |
| :--- | :--- | :--- | :--- | :--- |
| 默认（未选） | `surface.panel` | `line.divider` | `text.t2` | 无 |
| 悬停（项） | `surface.hover` | 不变 | `text.t1` | 无 |
| 选中 | `state.selected.bg` | 不变 | `text.t1` | 下沿 2px `signal.active` |
| 焦点 | 不变 | 外框 `line.focus` 外偏 1px | 不变 | 保持选中标 |
| 禁用 | `state.disabled.bg` | `line.divider` | `text.disabled` | 去掉选中标 |

**键盘**：Tab 进组停在当前项；←/→ 立即切换选中（单选组行为）；Home/End 到首尾；Space 选中当前焦点项。

**do / don't**

- do：两格为主，最多三格；等宽或按字长，不拉伸到整行。
- do：选中靠「底 + 下沿 2px」，不靠加粗、不靠反白。
- don't：不做三格以上的单位切换（µm² / px² 只有两格）。
- don't：不用分段做多选；多选改用复选。
- don't：不把分段做成胶囊、不做圆角底。

## 7 复选、开关、下拉、菜单项

**规格**

| 控件 | 尺寸 | 内边 / 间距 | 字档 |
| :--- | :--- | :--- | :--- |
| 复选框 | 方框 16 × 16，行高 28 | 框与标签 8；行左右 4（只为悬停底留边） | 13 |
| 开关 | 轨道 40 × 24，**旋钮 16 圆形**（全库唯一允许圆角处） | 旋钮内缩 4；标签与轨道 8 | 13 |
| 下拉触发器 | 高 28 | 左右 8；值 + 8 + 箭头 8 | 13 |
| 下拉菜单 | 项高 28，菜单最小宽 = 触发器宽 | 项左右 12；上下 4；容器 1px 边 | 13；快捷键 11 mono |
| 菜单项 | 高 28 | 左右 12；标签与快捷键 8 | 13；快捷键 11 `text.t3` |

**五态**

| 控件 | 默认 | 悬停 | 选中 / 开 | 焦点 | 禁用 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| 复选框 | 框 `surface.input` + `line.border`，空框 | 行底 `surface.hover` | 框 16 底 `state.selected.bg`，勾 2px `signal.active` | 框外 `line.focus` 1px | 框 `line.divider` + 字 `text.disabled`，无勾 |
| 开关 | 轨道 `surface.input` + `line.border`；旋钮空芯圆（透明底 + 1px `line.border`）在左 | 轨道底 `surface.hover` | 旋钮 **实心 `text.t1` 圆**移到右端；轨道下沿 2px `signal.active`（signal 只做线，不铺底） | 外框 `line.focus` | 轨道 `state.disabled.bg`、旋钮 `line.divider`，无 signal |
| 下拉 | `surface.input` + `line.border` | `surface.hover` | 展开中：外框 `line.focus`，箭头翻转 | `line.focus` | `state.disabled.bg` + `text.disabled` |
| 菜单项 | `surface.panel` | `surface.hover` | 当前项：`state.selected.bg` + 左沿 2px `signal.active` | 焦点在菜单容器上，当前项靠 2px 标 | `text.disabled`，无悬停 |

**键盘**：复选 Space 切换，混合态显示 8 × 2 短横；开关 Space 切换，←/→ 也可切换；下拉 Enter/Space/Alt+↓ 展开，↑/↓ 移动，Enter 提交，Esc 还原，输入首字母跳项；菜单 Alt+F 打开菜单栏，↑/↓ 移动，→/← 进出子菜单，Esc 关闭并把焦点还给触发者。

**do / don't**

- do：复选选中的三件套是固定的：16px 方框、框底 `state.selected.bg`、2px `signal.active` 的勾；三者同时出现，缺一不算选中。
- do：禁用时不画勾，框线换 `line.divider`、字换 `text.disabled`，仍能读出标签。
- do：`signal` 在复选框里只用于那一道 2px 勾，不扩散到标签、行底或焦点环。
- do：开关用「圆」讲物理动作：关 = 空芯圆在左，开 = 实心 T1 圆在右 + 下沿 2px signal；开态不铺信号底、不染字。
- do：圆只给开关旋钮；复选框是方框，两者不能长得像。
- don't：不用 1px 勾、不用 `text.t1` 勾、不用对勾字符（✓）——勾永远是 2px 线，随缩放不变粗。
- don't：不用开关表示立即生效的危险动作；那用按钮。
- don't：下拉菜单不加投影；用 1px 边与底分层。

## 8 表头与表行

**规格**：表头高 24，11 / 16，拉丁 600，`text.t3`，左右内边 8；表行高 28，13 / 20，数值用等宽右对齐，左右内边 8；列间只用 1px `line.divider` 横线，不画竖线。

**五态**

| 态 | 底 | 线 | 字 | 标 |
| :--- | :--- | :--- | :--- | :--- |
| 默认行 | `surface.panel` | 下 1px `line.divider` | `text.t1` | 无 |
| 悬停行 | `surface.hover` | 不变 | `text.t1` | 无 |
| 选中行 | `state.selected.bg` | 不变 | `text.t1`（不升、不染） | 左沿 2px `signal.active` |
| 焦点单元格 | 不变 | 单元格内 1px `line.focus`（内偏 1px） | 不变 | 保留所在行已有的左沿标 |
| 选中 + 焦点 | `state.selected.bg` | 左沿 2px + 当前格内 1px `line.focus` | `text.t1` | 两种标同时存在，互不覆盖 |
| 禁用行 | `state.disabled.bg` | `line.divider` | `text.disabled` | 去掉标 |

- 选中只做两件事：一档中性底 + 左沿 2px；**不改字色、不加整行边框、不加粗**。
- 焦点只落在**当前单元格**：1px `line.focus`，内偏 1px；**不给整行套框**，也不把焦点画到表头或表体外面。
- 选中与焦点同时存在时，左沿贴行首、细框贴当前格，两者间距至少 8，不叠在同一条边上。

**键盘**：↑/↓ 移动当前行；Home/End 到首尾；Space 选中/取消；Shift+↑/↓ 扩选；Enter 打开；PageUp/PageDown 翻页；表头获得焦点时 Enter 排序。

**do / don't**

- do：数值列右对齐、等宽、固定小数位；单位放表头，不进单元格。
- do：选中行只加左沿 2px，不整行反白、不加粗、不改字色。
- do：焦点画在当前格内侧 1px，选中画在行首左沿；两者不同边，可同时出现。
- don't：不把选中行的文字染成信号色——`signal` 不做文字色，数字永远是 T1。
- don't：不给整行套焦点外框；焦点只落在当前单元格。
- don't：不用斑马纹；行与行靠 1px 线。
- don't：不在行里放两个可点区域；行的点击目标是一整行。

## 9 状态项

只读，不可聚焦、不可点击。

**规格**：字 11 / 16；标签 `text.t3`，值 等宽 `text.t2`（当前值可用 `text.t1`）；项间 24（`space.statusGap`）；带左右 16（`space.statusPadX`）；高 28（`size.statusbar.height`）。

**do / don't**

- do：状态项只放「读数」——坐标、像素值、缩放、校准、位深。
- don't：不把按钮塞进状态栏；不放需要点击才能读的信息。

## 10 提示、滚动条、滑块、步进

**工具提示**：11 / 16，`text.t1`；底 `surface.panel`；1px `line.border`；内边 4 / 8；最大宽 320，超出折行；无投影；悬停 400ms 后出现，焦点触发同效，Esc 关闭；不得承载唯一信息。

**滚动条**：轨道宽 8（`space.2`），底透明；滑块宽 8，`line.border`，最短 24；悬停 `text.t3`，拖动中 `text.t2`；无箭头按钮；区域聚焦时 PageUp/PageDown 翻页。

**滑块**：轨道高 4（`space.1`），`line.divider`；已选段 4，`line.border`（中性，不用 signal）；手柄 16（`space.4`）方形，`surface.panel` 底 + 1px `line.border`；悬停时手柄描边 `text.t3`；拖动中手柄描边 `text.t2`、底 `state.selected.bg`（表示正在拖，不铺 signal）；焦点时手柄外圈 `line.focus`；禁用时轨道 `state.disabled.bg`、手柄 `line.divider`。键盘 ←/→ ±1，Shift ±10，Home/End 到两端。

**步进**：每个按钮 24 宽 × 28 高；`surface.panel` + `line.border`；− 与 + 各一枚，紧贴数值框右侧，中间 1px `line.divider`；悬停 `surface.hover`，按下 `state.selected.bg`，焦点 `line.focus`，禁用 `text.disabled`；↑/↓ 与按钮同步，按住 400ms 后连续步进。

**do / don't**

- do：提示、滚动条、滑块、步进都只用中性色与 1px 线。
- don't：滚动条不做悬停变宽、滑块不用 signal 填充、步进不做圆角胶囊。

## 11 键盘总表

| 场景 | 按键 | 行为 |
| :--- | :--- | :--- |
| 全局 | `Ctrl K` | 聚焦命令 / 搜索输入 |
| 全局 | `Ctrl M` | 测量当前 ROI |
| 全局 | `Ctrl Z` / `Ctrl E` | 撤销 / 导出 CSV |
| 全局 | `Esc` | 关闭浮层、清空搜索、还原输入 |
| 菜单栏 | `Alt`+首字母 / `F10` | 打开菜单 |
| 菜单 / 下拉 | ↑ ↓ Home End Enter Esc → ← | 移动、提交、关闭、进出子菜单 |
| 工具组 | 方向键、`V R P H Z` | 组内移动、直接切工具 |
| 分段 | ← → Home End Space | 单选组切换 |
| 复选 / 开关 | `Space` | 切换 |
| 表格 | ↑ ↓ Home End Space Shift+↑↓ Enter PageUp/Down | 移动、扩选、打开、翻页 |
| 数值框 | ↑ ↓ Shift+↑↓ PageUp/Down | ±1 / ±10 |
| 滑块 | ← → Shift+←→ Home End | ±1 / ±10 / 两端 |

- Tab 顺序按视觉顺序；浮层关闭后焦点回到触发者；不允许出现焦点陷阱。

## 12 验收

1. **尺寸**：所有控件的高、宽、内边、间距都能在 §1 或 `spacing.md` §5 找到名字，没有裸数值。
2. **颜色**：所有底、线、字、标都能在 `color-tokens.md` 1.2 找到语义名；没有第二强调色。
3. **五态**：§3–§10 每张表的五态在样张里都能找到；禁用项去掉了信号标。
4. **不反白**：任何控件的任何态都没有深色实底配浅色字。
5. **信号**：一屏只有当前项的下沿或左沿有 2px `signal`；控件内部没有 signal 填充；**没有任何文字被 signal 染色**。
6. **焦点**：焦点只出现在控件或当前单元格内；表格、列表都不出现整行外框；选中与焦点同时出现时，左沿与单元格细框互不覆盖。
7. **键盘**：只用键盘可以走完「选工具 → 画 ROI → 改数值 → 换单位 → 撤销 → 导出」。
8. **并排**：亮暗两版控件尺寸逐项一致，只有颜色不同。

## 13 边界

- 只定控件，不定页面结构；页面骨架、区域尺寸见 `spacing.md` 与稿 30。
- 字号、字重、行高见 `typography.md` 2.0；颜色与状态见 `color-tokens.md` 1.2；三者冲突时以本文的尺寸、颜色引用为准，不新造值。
- 稿 30 里 20 高的 chip 与分段属于间距基线之前的尺寸，按本文统一提到 28；其余结构、字段、入口不变。
