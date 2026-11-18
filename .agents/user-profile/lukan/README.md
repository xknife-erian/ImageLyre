# lukan 的 AI 配置档案

本目录是 `lukan`（Erian Lu，`lukan@xknife.net`）在本仓的 AI 配置档案根。它只描述个人工作方式，不代替项目规则；项目级规则见根 `AGENTS.md` 与 `docs/README.md`。

## 身份

| 项 | 值 |
| :--- | :--- |
| 显示名 | Erian Lu |
| 目录名 / 用户名 | `lukan` |
| 邮箱 | `lukan@xknife.net` |
| 岗位 | 技术负责人：架构 + 全栈 + 项目管理 |
| 主要工作范围 | ImageLyre 新一代显微图像分析系统：架构基线与内核模型、插件槽位与沙箱、AtomUI + Avalonia 界面重建 |

## 可写范围

**全部可写**（技术负责人），包括 `src/`、`tests/`、`docs/`、`.agents/` 下自己的档案与项目根级构建配置。

仍保留的边界：

- 他人档案（`.agents/user-profile/<其他人>/`）只读，修改需本人点名；
- 不直接改 `.agents/skills/` 下的下游技能副本，修订回游刃仓后再同步；
- 旧 `ImageLyre.sln` 与 2022 年版工程已弃用，不作为构建入口，也不在其中继续加代码；
- 覆盖他人正在进行的未提交改动前先沟通。

## 交付口径

- 目录与文件搬家一律 `git mv`，删除用 `git rm`，保留可追溯记录；
- 提交信息用中文并带 scoped 前缀（`feat(...)`、`fix(...)`、`docs(...)`、`chore(...)` 等）；
- 新解决方案落定前不做全仓构建，测试按工程单独执行；
- 文档产出归档到 `docs/<类型>/lukan/`，类型与命名按 `docs/README.md`；
- 节奏：小步推进，优先正确性与可回退，不追求一次到位。

## 岗位人设

`roles/architect.md`（架构）、`roles/fullstack.md`（全栈）、`roles/pm.md`（项目管理）。
