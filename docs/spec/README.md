# spec/ —— 需求、基线与验收交付物

本目录是产品经理的工作区，也是研发团队取用需求、架构基线与验收口径的入口。按 2026-11-22 的重置约定，过程类文档（计划、评审、周报）已随既有实现一并清除，本目录只保留需求与规范；根目录只保留本说明，避免文件堆叠。

## 交付物

| 目录 | 内容 |
| :--- | :--- |
| `requirements/` | `product-requirements.md` / `.html`，产品需求文档，md 为源 |
| `requirements/` | `product-appearance.md`，产品外观的第一直觉与反面清单 |
| `requirements/` | `core-1.0-boundary.md`，Core 1.0 的 15 个产品结果与 P0 收口边界 |
| `requirements/` | `measurement-baseline.md`，测量选项、列序、统计口径与 ImageJ 对照容差的唯一真源 |
| `requirements/` | `interaction-requirements.md`，15 条主交互流与横切规则 |
| `baseline/` | `startup-requirements.md`，启动需求与架构基线 |
| `m1/` | `Core-1.0-acceptance-matrix.xlsx`、`M1-imagej-fixture.md`、`M1-interaction-spec.md`；旧 M1 台账在 `archive/` |
| `cli/` | `cli-contract.md` 与 `schemas/lyre.*.schema.json`，CLI 与 Agent 的技术契约 |

交付物是研发排期、实现与验收的共同依据；改动须走评审，不能以过程稿替代。

## 规则

1. 交付物按 `requirements/`、`baseline/`、`m1/`、`cli/` 分组，根目录只留本 README。
2. 需求与架构基线是交付物的根；新一轮的设计、决策与过程记录在重建开始后按 `docs/README.md` 的类型目录重新建立，不再沿用旧目录。
3. 验收矩阵是需求与测试之间的桥：需求 ID 与验收用例一一对应，执行记录回填在矩阵中。
4. 历史的 `docs/adr/`、`docs/design/`、`docs/daily/` 内容已随 2026-11-22 的重置清空，目录与说明保留；需要取证旧内容时查标签 `archive/pre-reset-2026-11-22`。
