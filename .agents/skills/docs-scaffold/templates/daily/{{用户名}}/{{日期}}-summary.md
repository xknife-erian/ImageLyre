# {{日期}} 工作小结

> 本文件同时充当 `docs/daily/` 的格式示例与可复制模板，正式使用时请复制到本人用户目录、按 `<YYYY-MM-DD>-summary.md` 改名，并替换正文内容。

| 项目 | 内容 |
| :--- | :--- |
| 日期 | {{日期}} |
| 记录人 | {{用户名}} |
| 当日主线 | 搭建 `docs/` 文档体系并固化按用户归属的目录规则 |

## 完成事项

- 梳理 `docs/` 现有结构，确认 `adr`, `analysis`, `daily`, `design`, `plans` 五个类型目录的职责边界，并明确各自回答的问题。
- 将“每位同事以本人用户名建立独立文件夹”的规则写入 `docs/README.md`，同时给出目录表、命名约定与协作条款。
- 在 5 个类型目录下各放置一份本人名下的示例文件，使规则具备可参照的实物样本。
- 修正 README 中与实际目录不符的条目（原描述的 `definitions/` 与 `archive/` 均已不存在）。

## 进行中

- 结合评审意见调整示例文件的信息密度，重点检查 ADR 示例是否把备选方案与后果写完整。

## 阻塞

- 暂无。

## 次日计划

- 请同事按新约定创建各自用户目录并试写一份日志，验证规则在真实使用中是否顺手。
- 若两周内无人对编号规则提出异议，则把该约定同步到项目根 `README.md` 的贡献指南一节。

## 关联文档

- `docs/adr/{{用户名}}/ADR-0001-docs-per-user-layout.md`
- `docs/analysis/{{用户名}}/{{日期}}_docs-layout-options.md`
- `docs/design/{{用户名}}/DESIGN-0001-docs-convention.md`
- `docs/plans/{{用户名}}/PLAN-0001-docs-convention-rollout.md`
