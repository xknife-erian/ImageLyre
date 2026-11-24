# ImageLyre

ImageLyre 是新一代跨平台显微图像分析系统仓库，目标是全面替代 ImageJ / Fiji；本文件只做路由，说明如何确认自己是谁、去读哪份 AI 配置档案、按什么顺序加载技能，具体规范、命令与目录细节一律引用 `docs/` 与 `README.md`，不在这里重复。

## 1 用户路由（先做这一步）

1. **确定用户名**：取 `git config user.name`；显示名与目录名不一致时，取 `git config user.email` 的本地部分。
2. **打开你的 AI 配置档案**：`.agents/user-profile/<用户名>/`，这是 `.agents/` 下唯一按人分目录的落点。
   - **目录不存在**：说明你在本仓尚未登记，按你所用 AI 工具的习惯工作，不要套用他人的模型、提示词或流程；产出仍按 `docs/<类型>/<用户名>/` 归档。
   - **目录存在**：该目录是你的 AI 配置档案根，至少读 `README.md`，按其中写明的可写范围、禁碰项与交付口径执行，需要时再读 `team-info.md`、`conventions.md`、`roles/<岗位>.md` 与 `shared/`。
3. **档案彼此独立**：不要把某人的配置、模型偏好或角色分工套到另一个人身上；修改他人档案必须先由本人点名。
4. **根文件不登记名单**：新增成员只建 `.agents/user-profile/<用户名>/`，不把人员名单堆回本文件。

## 2 技能加载

技能只在任务匹配时按需加载，正文不复制到本文件。技能的唯一真源是游刃仓 `https://gitee.com/fangyu_fy/youren.git`，本仓 `.agents/skills/` 只是下游拷贝，只读；修订先提交到游刃仓，再用游刃仓的同步脚本拉取更新，不要直接改副本，否则下一轮同步会把改动覆盖掉。

| 技能 | 何时加载 | 路径 |
| :--- | :--- | :--- |
| `agent-project-rollout` | 新建项目接入 agent，或体检、同步技能副本时 | `.agents/skills/agent-project-rollout/SKILL.md` |
| `architect-agent-team` | 架构师带多个子 Agent 改代码：拆任务书、判定续用还是新开上下文、Review 与门禁时 | `.agents/skills/architect-agent-team/SKILL.md` |
| `csharp-doc-style` | 写或评审 C# 注释、ADR、Plan、Design、spec 等技术文档时 | `.agents/skills/csharp-doc-style/SKILL.md` |
| `docs-scaffold` | 初始化或体检补全 `docs/` 目录与规则时 | `.agents/skills/docs-scaffold/SKILL.md` |
| `eod-wrapup` | 收工：写当日小结、提交本地备份、更新计划与决策状态时 | `.agents/skills/eod-wrapup/SKILL.md` |
| `morning-review` | 开工：读最近小结、体检仓库与计划、输出今日议程时 | `.agents/skills/morning-review/SKILL.md` |
| `tdd-and-unit-testing` | 写生产代码或单元测试时 | `.agents/skills/tdd-and-unit-testing/SKILL.md` |
| `html-report` | 把 analyses、design、guide、spec 的长文渲染成单文件 HTML 时 | `.agents/skills/html-report/SKILL.md` |

加载方式为读取该路径下的 `SKILL.md`；技能若附带 `scripts/`、`references/`、`templates/`，按技能目录的相对路径取用，不要把仓库路径写死进技能正文。

## 3 文档入口

先读 `docs/README.md`（分类、命名、流转与退役规则），再按任务取用：

- `docs/spec/requirements/product-requirements.md`：产品需求文档，定义用户、功能范围、优先级与验收口径，研发排期以它为准；
- `docs/spec/baseline/startup-requirements.md`：新一代系统的启动需求与架构基线，任何设计、计划与评审先以它为前提；
- `docs/spec/m1/`、`docs/spec/cli/`：M1 验收与交互规范、CLI 与 Agent 的技术契约；
- `docs/adr/<用户名>/`、`docs/design/<用户名>/`、`docs/plans/<用户名>/`、`docs/analyses/<用户名>/`、`docs/daily/<用户名>/`：按类型与用户名归档；2026-11-22 重建的首批产出是 `docs/adr/lukan/ADR-0001-插件机制架构基线.md`、`docs/design/lukan/DESIGN-0001-M1-插件骨架.md` 与 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md`，其余目录待写入；
- `docs/guide/`、`docs/reference/`：面向使用者与外部资料，平铺存放、不按人分。

## 4 项目现状与约定

2022 年版基于 WPF 与 .NET 6 的实现、以及 2026-11 的两轮重建尝试整体弃用：2026-11-22 起工作区已清空，`src/`、`tests/`、`tools/` 下的代码与全部过程文档（ADR、design、analyses、daily、plans、guide、reference）的内容一并删除，`docs/` 的类型目录与各目录说明保留，清除前的完整状态保存在标签 `archive/pre-reset-2026-11-22`，需要取证时从该标签读取而不是凭记忆补写。

M1 已完成（2026-11-23）：唯一构建入口 `dotnet build src/ImageLyre.slnx`，不得另起第二套；目标框架、可空与警告即错误集中在根 `Directory.Build.props`，包版本集中在根 `Directory.Packages.props`，SDK 与 `dotnet test` 的 runner 由根 `global.json` 钉住（.NET 10 上 `dotnet test` 走 Microsoft.Testing.Platform；本机沙箱里 MTP 命名管道会被拒，跑测试请直接执行 `tests/<工程>/bin/Debug/net10.0/<工程>.dll`）。目录按职责分：`src/App/` 放进程入口与启动链，`src/Infrastructure/` 放共享库（其中 `Contracts/` 专放契约与领域），`src/Plugins/Modules/` 与 `src/Plugins/Features/` 放插件。插件机制的架构基线与 M1 骨架分别由 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` 与 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` 规定，工程目录、插件工程命名与扫描前缀以该 design 为准。下一阶段为 G2 只读治理：插件与能力面板、依赖树与分级、诊断与受限模式，按 ADR-0001 §2.5 推进。

少而硬的共同约定：每个工程（csproj）必须带 README.md，写明该工程的意义、边界（不放什么）、依赖方向与边界变化时该改哪份文档，空工程同样如此，新建工程先写 README 再写代码；目录与文件搬家一律 `git mv`，删除用 `git rm`，禁止复制后把原目录留在原处；行尾与编码以 `.gitattributes`、`.editorconfig` 为准，新增文件交给 git 自动归一化；提交信息用中文并带 scoped 前缀，一次提交只做一件事。

## 5 目录速览

| 路径 | 用途 |
| :--- | :--- |
| `docs/spec/` | 需求、架构基线、M1 验收规范与 CLI 契约 |
| `docs/` | 文档体系，规则入口 `docs/README.md`；`adr/`、`design/` 与 `plans/` 已有首批内容，其余类型目录与说明在位、内容待写入 |
| `src/`、`tests/` | 源码与测试落点；目录结构与插件命名按 `DESIGN-0001`，M1 的十三个常驻工程、一个只随 Debug 输出的故障样本与七套测试工程已在其位 |
| `scripts/qa/` | 检查脚本落点：`verify-m1.ps1` 做引用方向、机制层词表、通配引用计数、工程 README、平台符号与壳契约纯度六项检查；`smoke-m1.ps1` 跑无界面清单与桌面中英切换冒烟并断言日志行 |
| `.agents/skills/` | 游刃技能的下游副本，只读 |
| `.agents/user-profile/<用户名>/` | 各人的 AI 配置档案，`.agents/` 下唯一的个人落点 |
| `README.md` | 项目背景与历史缘由 |

## 6 任务闭环

读本文件 → 自己的 AI 配置档案 → 对应技能 → `docs/` 对应文档；交付按 AI 配置档案的口径归档到 `docs/<类型>/<用户名>/`。
