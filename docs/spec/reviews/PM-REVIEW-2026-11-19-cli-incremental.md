# CLI 增量评审记录

| 版本 | 日期 | 范围 | 记录人 |
| :--- | :--- | :--- | :--- |
| 1.0 | 2026-11-19 | PRD 2.4 §5.17 CLI 工作模式 | lukan |

## 1 评审设置

对象是 PRD 2.4 新增的 7 条 CLI 需求，目标是「纯 CLI 工作模式，适应 AI 调用」。三位评审彼此独立，只读文档、不改文件，分别从方法开发者、桌面架构师、日常分析者（兼 AI 调用者）视角给出结论。

| 角色 | 关注点 | 结论 |
| :--- | :--- | :--- |
| 方法开发者 | CLI 契约、JSON schema、ROI/校准/选项输入、确定性、批处理 | blocker |
| 桌面架构师 | 进程拓扑、AOT/JIT、沙箱宿主、分发、安全、版本化 | major |
| 日常分析者 | 命令可发现性、错误可读性、AI 解析、路径与 shell、安装成本 | blocker |

## 2 主要发现与处置

| # | 发现 | 严重度 | 2.5 处置 |
| :--- | :--- | :--- | :--- |
| 1 | CLI-001 的五步闭环与 CLI-002 的六个子命令对不上，没有校准、ROI、测量选项、线程数参数；`open` 未定义 | blocker | 重写命令表，新增 `info`/`measure`/`export`/`run`/`schema`/`version`；`measure`/`export` 接受 `--pixel-size`、`--known-distance`、`--unit`、`--z-step`、`--roi-json`、`--measurements`、`--options`、`--threads`；删除 `open`；`batch` 明确为 P1 |
| 2 | ROI JSON 全文未定义，与 `.roi/.zip` 的 ImageJ 互操作冲突 | blocker | 钉死 `--roi` 接受 ImageJ `.roi/.zip`；`lyre.roi/v1` 只读 JSON 定义字段、像素坐标、顶点顺序、Z/T，不含校准 |
| 3 | 退出码缺 1、超时、取消、内部错误；错误 code 非稳定枚举；`schema` 输出范围未定义 | major | 退出码扩到 0–9；错误 code 固定 11 个命名空间；`lyre schema` 输出命令、参数、Schema、退出码、错误码；`--json` 默认，`--text` 才人类可读 |
| 4 | 缺 JSON Schema 实体，`--json` 开关语义含糊 | major | 新增 `docs/spec/lyre.cli.v1.schema.json`，含信封、错误、校准、ROI、测量、批处理索引；stdout 只含 JSON；契约只增字段 |
| 5 | stdin/清单/批处理索引格式未冻结，缺输出模板与覆盖语义 | major | manifest 支持 JSON 与 JSONL；`--jobs`、`--out-dir`、`--out-template`、`--overwrite` 明确；索引按输入序；部分失败退出 8；无状态 |
| 6 | 没有 CLI-only 分发物，CI 要装 GUI 包；AUTO-004 与 CLI 入口重复且依赖 P1 Python | major | 新增 ADR-0006：NativeAOT `lyre` + JIT `lyre-sandbox-host`；CLI 包 ≤100 MB、冷启动 ≤1.5 秒；`lyre` 是唯一无头入口，AUTO-004 复用 `lyre run` |
| 7 | CLI-006 要求 sidecar 逐值一致，但 sidecar 含时间戳，字面不可测；未定义容差与线程数 | major | 改为：同一构建、同一 `--threads`、同一选项集；JSON 按 §6.5 B 类 `1e-9`，CSV 按 §6.7 字节，sidecar 只比稳定字段 |
| 8 | CLI-007 安全是 P1，M2 的 P0 CLI 会先公开；无路径与隐式配置规则 | major | CLI-007 升 P0，覆盖断网、只读输入、显式输出、`--overwrite`、`--cwd`、无隐式配置；`--profile` 留 P1 |
| 9 | 渲染版 HTML 把 2.4 标成「定稿」，与 Markdown「评审稿」不一致 | minor | 重生成 HTML，2.5 定稿；版本表、徽章、修订号统一 |

## 3 处置结论

三份评审的 blocker 与 major 均已在上表处置，2.5 据此定稿。仍未闭合的是实现与验证项，不是文档口径：

- `lyre.cli.v1.schema.json` 需进 CI，对 `info`/`measure`/`export`/`run`/`schema` 的每条输出做校验；
- CLI-only 包体、冷启动、无 Avalonia/AtomUI/SkiaSharp 引用的断言需在 M2 门禁实测；
- `lyre-sandbox-host` 的版本协商与 `sandbox.unavailable` 降级路径需在 M2 联调；
- 非默认校准 + 非默认测量选项的 CLI ↔ 桌面对照用例需随 VAL-004 落地。

## 4 处置与收口

| 角色 | 评审结论 | 处置 |
| :--- | :--- | :--- |
| 方法开发者 | blocker | 已处置；schema 与 CI 校验列为 M2 前置 |
| 桌面架构师 | major | 已处置；ADR-0006 随 M2 设计评审确认 |
| 日常分析者 | blocker | 已处置；CLI 任务纳入可用性测试 |

本记录只证明三位评审的输入已处置，不声称三位对 2.5 做过二次签署；2.5 由产品经理收口，不替代 M2 的实现评审与实测报告。若需要二次签署，另起一轮评审。
