# spec/ —— 产品交付物与过程

本目录是产品经理的唯一工作区，也是研发团队取用需求、架构基线与验收口径的入口。交付物按用途分组，过程文档按类型分目录；根目录只保留本说明，避免文件堆叠。

## 交付物

| 目录 | 内容 |
| :--- | :--- |
| `requirements/` | `product-requirements.md` / `.html`，产品需求文档 4.0，md 为源 |
| `requirements/` | `core-1.0-boundary.md`，Core 1.0 的 15 个产品结果与 P0 收口边界 |
| `baseline/` | `startup-requirements.md`，启动需求与架构基线 1.4 |
| `m1/` | `Core-1.0-acceptance-matrix.xlsx`、`M1-imagej-fixture.md`、`M1-interaction-spec.md`；旧 M1 台账在 `archive/` |
| `cli/` | `cli-contract.md` 与 `schemas/lyre.*.schema.json`，CLI 与 Agent 的技术契约 |

交付物是研发排期、实现与验收的共同依据；改动须走评审，不能以过程稿替代。

## 过程子目录

| 目录 | 承载内容 | 命名约定 |
| :--- | :--- | :--- |
| `plans/` | 里程碑计划、任务拆解 | `PM-PLAN-YYYYMMDD-<主题>.md` |
| `reviews/` | 专家评审、设计评审、里程碑评审记录 | `PM-REVIEW-YYYY-MM-DD-<主题>.md` |
| `status/` | 周报、阶段状态、风险与待拍板 | `PM-STATUS-YYYY-MM-DD.md` |

过程文档只回答「怎么推进、卡在哪里、下一步是什么」，不重复需求正文；状态结论一旦定型，回写到交付物或在 `reviews/` 留档。

## 规则

1. 交付物按 `requirements/`、`baseline/`、`m1/`、`cli/` 分组；根目录只留本 README。
2. 过程文档以 `PM-` 开头，后接 `PLAN-`、`REVIEW-`、`STATUS-` 类型标记。
3. 交付物按版本演进；过程文档按日期累积，不追改历史。
4. 历史的 `docs/adr/`、`docs/design/`、`docs/daily/` 继续保留，但产品经理的新产出不再写入那些目录。
5. 执行台账以 GitHub Issues（仓库 `xknife-erian/ImageLyre`）与 `m1/Core-1.0-acceptance-matrix.xlsx` 为准；Gitee Issue 与 Milestone 停用。

## 与其它目录的关系

- 需求与架构基线是交付物的根；具体设计在 `docs/design/`，决策记录在 `docs/adr/`，过程记录在本目录的 `plans/`、`reviews/`、`status/`。
- 外部资料放 `docs/reference/`；面向使用者的指南放 `docs/guide/`；两者都不复制到本目录。
- 验收矩阵是需求与测试之间的桥：需求 ID 与验收用例一一对应，执行记录回填在矩阵中，不在计划里另起一套。
