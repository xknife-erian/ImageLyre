# src/ —— 源码根

本目录是全部源代码的落点。2026-11-22 的重置把既有实现整体弃用，旧状态保存在标签 `archive/pre-reset-2026-11-22`；当前是 M1 的插件骨架，目录结构与工程命名按 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1。

## 目录约定

| 目录 | 放什么 | 不放什么 |
| :--- | :--- | :--- |
| `App/` | 进程入口 `ImageLyre.App` 与启动链 `ImageLyre.Bootstrap` | 不放主窗体布局、不放插件工程 |
| `Infrastructure/` | 共享库：`ImageLyre.Localization` 与附着物词汇 `ImageLyre.Shell.Attachments` | 不放 UI 路线绑定、不放插件 |
| `Infrastructure/Contracts/` | 契约与领域：`ImageLyre.Domains`、`ImageLyre.Contracts`、`ImageLyre.Shell.Contracts` | 不放 UI、不放机制包、不放实现 |
| `Plugins/Modules/` | 提供契约的 Module 插件，含只随 Debug 输出的故障样本 | 不放界面表达 |
| `Plugins/Features/` | 只消费契约的 Feature 插件 | 不放契约实现、不引用 Module 工程 |
| `cangming/` | 只读子模块，源码对照与版本锚点 | 不参与编译、不改动 |

物理位置不改变层归属：层与依赖方向以 design §3.1 的“类型”列为准。测试工程落在同级的 `tests/`，构建入口是 `src/ImageLyre.slnx`。
