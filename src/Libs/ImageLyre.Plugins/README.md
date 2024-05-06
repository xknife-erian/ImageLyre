# ImageLyre.Plugins：插件 SDK

本工程是官方插件作者写代码时唯一需要认识的基座：模块与 Feature 的薄封装、两条菜单装配路径，以及 `plugindesc` 的解析与命令绑定。它不实现任何宿主服务——槽位注册、菜单投影、命令执行都在模块里，SDK 只负责把插件的数据整理成宿主认得的形状。

## 四个文件

| 文件 | 作用 | 谁用 |
| :--- | :--- | :--- |
| `ImageLyreCodeMenuFeature` | code-first 菜单基类：在 `RegisterAttachments` 里调 `AttachMenu(registry, "帮助", items)` 即完成附着 | 官方插件 |
| `ImageLyreMenuFeature` | `plugindesc` 菜单基类：读程序集内嵌资源、解析成分类、按名绑定命令 | 第三方插件 |
| `PlugindescMenuReader` | 纯解析：XML 文本到分类、分组与菜单项，不接触命令 | 内部 |
| `PlugindescMenuSource` | 把解析结果绑上命令并转成菜单附件，命令缺失时记警告 | 内部 |

## 两条路径怎么选

官方插件一律 code-first，因为菜单结构与命令字典同处一个文件，命令名写错在编译期就暴露，重构时改名也跟着改。第三方插件用 `plugindesc`，因为它的价值正是"不改代码就能调菜单"，代价是描述文件与实现之间没有编译期约束，只能靠启动期告警与评审发现脱节。

两条路径产出同一种数据附件：分类 → 分组 → 菜单项。投影层不区分来源，因此同一棵树里既有来自代码的项也有来自描述的项，合并规则只看标题。

## 命令绑定约定

`plugindesc` 里的命令名与 `BuildCommands()` 返回字典的键逐字一致，区分大小写。绑定不了时有三种表现，都必须可预期：描述文件缺失按无菜单插件处理且不报错；解析失败按无菜单处理并记一条警告；命令名找不到时菜单项**保留但不可点**并记警告，因为让描述与实现的脱节可见，比让它安静消失有用。

## 依赖

`ImageLyre.UI.Contracts`（菜单附件与槽位名）、`Cangming.PicoPlugins`（`BasePicoFeatures` 与附着注册器）、`Cangming.Logs`（警告落盘）。不引 `ImageLyre.Contracts`，因为 SDK 只面向界面侧的装配，服务契约由插件按需自己引。

## 写一个官方插件的最小步骤

1. 继承 `ImageLyreCodeMenuFeature`，实现 `BuildCommands()` 返回命令字典。
2. 在 `RegisterAttachments` 里调 `AttachMenu`，把菜单项连同命令实例交出去。
3. 给类加 `[Plugin]`，程序集名以 `ImageLyre.` 开头，这样启动时会被前缀预加载扫到。
4. 不写构造函数依赖：构造期拿不到依赖，需要宿主服务时走内核的惰性解析。

## 不做的事

不做权限声明、不做可见性聚合、不做插件市场与签名校验；槽位注册与菜单投影不在本工程，因为它们是宿主服务，放进 SDK 会让"换了宿主就得改 SDK"成为必然。
