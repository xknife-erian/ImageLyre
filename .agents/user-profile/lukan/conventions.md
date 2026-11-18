---
title: lukan 的个人约定
project: imagelyre
tags: [约定, git, 文档, 构建, AI]
updated: 2026-11-18
author: lukan
---

# lukan 的个人约定

## Git

- 搬家 `git mv`，删除 `git rm`；禁止复制后把旧目录留在原处。
- Commit 信息用中文，带 scoped 前缀，例如 `feat(core): ...`、`fix(ui): ...`、`docs(spec): ...`、`chore: ...`。
- 一次提交只做一件事；文档重排与代码重命名分提交。
- 提交前确认 `git status --short`，不把他人未提交的改动扫进来。
- 节奏：按可回退的小步推进，不追求一次到位，也不为赶进度跳过验证。
- 推送与分支：长期分支 `develop`；推送与合并前确认，不强制 PR 流程。

## 构建与测试

- 唯一构建入口为 `src/ImageLyre.slnx`；2022 年版工程已归档，不在其中继续加代码。
- 单工程构建：`dotnet build src/<项目>/<项目>.csproj -c Debug`，工程路径随新骨架落地。
- 测试按工程单独执行，不套全仓命令；新代码按 `tdd-and-unit-testing` 技能推进。

## 文档

- 文档类型、命名与归档规则见 `docs/README.md`。
- 我的产出放 `docs/adr/lukan/`、`docs/design/lukan/`、`docs/plans/lukan/`、`docs/analyses/lukan/`、`docs/daily/lukan/`。
- `docs/spec/` 平铺、不按人分；启动需求与架构基线以 `docs/spec/startup-requirements.md` 为准。
- 文档只写现状与结论，迁移与排查过程放 `docs/analyses/lukan/` 或 `docs/daily/lukan/`。

## AI 使用

- 常用工具：DeepSeek Harness、Hermes。
- 各人自选工具与模型，本仓不做统一模型分配；`shared/` 暂不维护。
- 任务开始：读 `AGENTS.md` → 本档案 → 对应技能 → `docs/` 指南。
- 不直接修改 `.agents/skills/` 下的下游技能副本；要改先提游刃仓，再同步。
