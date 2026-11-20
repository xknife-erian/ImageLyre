# ADR-0010：主题切换的唯一机制

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 0.1 | 2026-11-20 | 定稿 | lukan | 定下主题切换只走 `RequestedThemeVariant`，把 `IThemeManager` 的 URI 换字典语义收回 |

## 1 背景

`docs/adr/lukan/ADR-0009-theme-token-landing.md` 定下亮暗两套字典以 `ThemeDictionaries` 承载、`Application.RequestedThemeVariant` 作唯一开关，控件树不重建。但仓库里同时存在另一套机制：`ImageLyre.UI.Contracts.Theming.IThemeManager` 的 `RegisterTheme(name, uri)` 加 `ApplyTheme(name)` 是"按 URI 换整本字典"。

两套并存会出三种问题：其一，同一时刻到底哪套在生效说不清；其二，按 URI 换字典会让控件树的资源引用重新解析，切主题变成重建；其三，插件若按 `IThemeManager` 编程，等它被换掉时行为会变。

## 2 决策

1. **主题切换只认 `RequestedThemeVariant`**：外观模块写它，跟随系统写 `ThemeVariant.Default`，亮暗由 `ThemeDictionaries` 自动取值。
2. **`IThemeManager` 降级为登记面**：`RegisterTheme` 只登记"有哪些主题可用"（名称与来源），不再承担切换；`ApplyTheme(name)` 的实现改为设置 `RequestedThemeVariant`，不再换字典。
3. **令牌与主题分离**：主题只决定颜色与材质，字号、间距、线宽这些不随亮暗变化的令牌放在共享字典里，切主题不重载它们。
4. **插件不直接切主题**：需要切换时通过外观模块的入口，不允许插件自己写 `RequestedThemeVariant` 或自己换字典。

## 3 后果

- 切主题是一次属性变更，控件树不重建，S2"结构先于皮肤"因此在运行期可验证。
- `IThemeManager` 的实现简单了，只剩一张名称表与一次属性写入；第三方插件按 URI 换主题的老写法失效，需要迁移到外观模块的入口。
- 主题包仍可整包替换：换的是 `Themes` 里两份字典的内容，键名与结构不变。

## 4 备选方案

| 方案 | 结论 |
| :--- | :--- |
| 全部走 `IThemeManager` 的 URI 切换 | 否：换字典会带动控件树重解析，与 S2 冲突 |
| 两套并存，按调用方自选 | 否：生效机制说不清，是最难查的一类缺陷 |
| 直接改 `Application.ActualThemeVariant` | 否：那是只读的运行期结果，写它属于绕过机制 |

## 5 失效条件

1. 若 AtomUI 的主题体系要求由它自己的 `IThemeManager` 驱动（见 `docs/adr/lukan/ADR-0009-theme-token-landing.md` §2.2 的 Schema 路线），则本仓外观模块改为调用 AtomUI 的入口，本 ADR 的机制条款让位，但"只有一套机制"不退让。
2. 若将来需要运行期加载外部主题包（用户自带主题），`RegisterTheme` 要扩成可加载资源，切换入口不变。
3. 若出现第二个界面宿主，本 ADR 只保证"单宿主内只有一套机制"，跨宿主另议。

## 6 与其它文档的关系

`docs/design/lukan/DESIGN-0004-architecture-principles.md` 的 S2 要求"结构先于皮肤"，本 ADR 给它一条可验证的实现路径：切主题不动控件树。

`docs/plans/lukan/PLAN-0002-architecture-rollout.md` §7.2 把本 ADR 列为未决项；本文定稿后该项关闭，`IThemeManager` 的契约修改随 D 批的主题切换一起做。
