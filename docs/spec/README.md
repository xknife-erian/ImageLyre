# spec/ —— 产品交付物与过程

本目录是产品经理的唯一工作区，也是研发团队取用需求、架构基线与验收口径的入口。根目录只放给研发直接使用的成果；计划、评审、状态等过程文档放在子目录，并用大写前缀标记，避免过程稿与交付物混在一起。

## 根目录（交付物）

| 文件 | 说明 |
| :--- | :--- |
| `product-requirements.md` / `.html` | 产品需求文档 2.8，md 为源，HTML 为阅读版 |
| `startup-requirements.md` | 启动需求与架构基线 1.4 |
| `M1-acceptance-matrix.xlsx` | 72 条 P0 验收台账，含 M1 范围与说明 |
| `M1-imagej-fixture.md` | M1a 的 ImageJ 对照环境、fixture 组成与黄金数据集 v0.1 |
| `M1-interaction-spec.md` | M1a 的窗口骨架、操作流程、键位、状态与交互验收 |
| `lyre.*.schema.json` | CLI/Agent 契约：信封、工具清单、ROI、plan、event、audit、pipeline、batch、calibration |
| `README.md` | 本目录的布局与规则 |

根目录文件是研发排期、实现与验收的共同依据；改动须走评审，不能以过程稿替代。

## 过程子目录

| 目录 | 承载内容 | 命名约定 |
| :--- | :--- | :--- |
| `plans/` | 里程碑计划、任务拆解 | `PM-PLAN-YYYYMMDD-<主题>.md`；执行状态回填到 `M1-acceptance-matrix.xlsx`，不另设 Issue 清单 |
| `reviews/` | 专家评审、设计评审、里程碑评审记录 | `PM-REVIEW-YYYY-MM-DD-<主题>.md` |
| `status/` | 周报、阶段状态、风险与待拍板 | `PM-STATUS-YYYY-MM-DD.md` |

过程文档只回答「怎么推进、卡在哪里、下一步是什么」，不重复需求正文；状态结论一旦定型，回写到根目录的交付物或在 `reviews/` 留档。

## 规则

1. 根目录只放研发要直接使用的成果，草稿、过程记录与会议纪要一律进子目录。
2. 过程文档以 `PM-` 开头，后接 `PLAN-`、`REVIEW-`、`STATUS-` 类型标记；`PM-` 表示这份文档由产品经理生成，用来与研发自己的记录区分。
3. 根目录文件按版本演进；过程文档按日期累积，不追改历史。
4. 历史的 `docs/adr/`、`docs/design/`、`docs/daily/` 继续保留，但产品经理的新产出不再写入那些目录。
5. 执行台账以 GitHub Issues（仓库 `xknife-erian/ImageLyre`）与 `M1-acceptance-matrix.xlsx` 为准；Gitee Issue 与 Milestone 停用，避免同一状态维护两遍。

## 与其它目录的关系

- 需求与架构基线是本目录的根；具体设计在 `docs/design/`，决策记录在 `docs/adr/`，过程记录在本目录的 `plans/`、`reviews/`、`status/`。
- 外部资料放 `docs/reference/`；面向使用者的指南放 `docs/guide/`；两者都不复制到本目录。
- 验收矩阵是需求与测试之间的桥：需求 ID 与验收用例一一对应，执行记录回填在矩阵中，不在计划里另起一套。
