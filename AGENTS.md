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

技能只在任务匹配时按需加载，正文不复制到本文件。技能的唯一真源是游刃仓 `https://gitee.com/fangyu_fy/youren.git`，本仓 `.agents/skills/` 只是下游拷贝，只读；修订先提交到游刃仓，再用 `tools/sync-skills.cs` 同步过来，不要直接改副本，否则下一轮同步会把改动覆盖掉。

| 技能 | 何时加载 | 路径 |
| :--- | :--- | :--- |
| `agent-project-rollout` | 新建项目接入 agent，或体检、同步技能副本时 | `.agents/skills/agent-project-rollout/SKILL.md` |
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
- `docs/spec/plans/`、`docs/spec/reviews/`、`docs/spec/status/`：产品经理的过程文档，文件名以 `PM-` 开头并接 `PLAN-`、`REVIEW-`、`STATUS-` 类型标记；
- `docs/adr/lukan/`、`docs/design/lukan/`、`docs/plans/lukan/`、`docs/analyses/lukan/`、`docs/daily/lukan/`：按类型与用户名归档；
- `docs/guide/`、`docs/reference/`：面向使用者与外部资料，平铺存放、不按人分。

## 4 项目现状与约定

2022 年版基于 WPF 与 .NET 6 的实现整体弃用，最后一次完整状态保存在 `legacy/wpf-2022` 分支与 `archive/pre-rebuild-2026-11-18` 标签，主线 `develop` 已清空旧代码。新系统按启动需求重建，唯一构建入口为 `src/ImageLyre.slnx`，桌面入口在 `src/Apps/ImageLyre.Desktop`，`src/Libs/` 按内核、成像、编解码、插件、沙箱与 UI 分层；构建命令为 `dotnet build src/ImageLyre.slnx`。`tests/` 预留给 M1 阶段按 TDD 建立的测试工程。

少而硬的共同约定：目录与文件搬家一律 `git mv`，删除用 `git rm`，禁止复制后把原目录留在原处；行尾与编码以 `.gitattributes`、`.editorconfig` 为准，新增文件交给 git 自动归一化；提交信息用中文并带 scoped 前缀，一次提交只做一件事。

## 5 目录速览

| 路径 | 用途 |
| :--- | :--- |
| `src/ImageLyre.slnx` | 唯一构建入口 |
| `src/Apps/ImageLyre.Desktop` | Avalonia + AtomUI 桌面入口 |
| `src/Libs/` | 内核、成像、编解码、插件、沙箱与 UI 六个类库 |
| `tests/` | 测试工程落点，M1 建立 |
| `docs/` | 文档体系，规则入口 `docs/README.md` |
| `.agents/skills/` | 游刃技能的下游副本，只读 |
| `.agents/user-profile/<用户名>/` | 各人的 AI 配置档案，`.agents/` 下唯一的个人落点 |
| `README.md` | 项目背景与历史缘由 |

## 6 任务闭环

读本文件 → 自己的 AI 配置档案 → 对应技能 → `docs/` 对应文档；交付按 AI 配置档案的口径归档到 `docs/<类型>/<用户名>/`。
