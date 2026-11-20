# CLI 与 Agent 技术契约

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 1.0 | 2026-11-19 | 研发契约 | lukan | 从 PRD 2.9 §5.17 拆出；命令、参数、Schema、退出码与实现约束在此维护，产品口径见 `../requirements/product-requirements.md` §5.17 |
| 1.1 | 2026-11-19 | 研发契约 | lukan | 产品口径改挂 PRD 4.3 的 CORE-14；测量选项、列序与对照容差改挂 `../requirements/measurement-baseline.md`；章节编号由 5.17.x 改为 1.x，内容不变 |

> 本文是研发契约，不是产品需求。产品经理只定义“要什么、为什么、怎样算好”；命令形状、JSON Schema、退出码、宿主拓扑与实现约束由研发在此维护。本文的产品落点是 PRD 4.3 的 CORE-14，测量口径与对照容差以 `../requirements/measurement-baseline.md` 为准，两者冲突时以交付物为准并回写本文。

## 1 CLI 工作模式

CLI 是独立入口，不依赖 AtomUI、Avalonia、SkiaSharp 或 `ImageLyre.UI`，只引用 Kernel、Imaging、Codecs、Plugins、Sandbox 与 headless 会话层；桌面与 CLI 共用该层，因此同一输入、同一构建、同一线程数下的结果必须逐值一致。AI 与外部流水线只消费 stdout 的 JSON、事件流与退出码，不解析自然语言。项目落点为 `src/Apps/ImageLyre.Cli`，CLI-only 产物与 `lyre-sandbox-host` 的拓扑见 ADR-0006；MCP 属 v2，不在 v1。`lyre` 是唯一无头入口，AUTO-004 复用 `lyre` 命令组合，Python 脚本执行仍属 AUTO-001/002。

CLI 把「工具说明、图像元数据、插件返回值」一律视为**数据**，只有命令名与参数名可作为指令；v1 不提供 plan/confirm 开关；写命令必须显式输出路径，执行结果用 outputs 的 SHA 与 sidecar 复核。

### 1.1 命令与参数

CLI 默认输出 JSON；`--text` 才输出人类可读文本。未给 `--cwd` 时，所有路径参数必须为绝对路径。`--out-dir`、`--cwd`、`--timeout`、`--cache-budget` 为全局参数；v1.1 不提供 `--plan`/`--confirm`/`--events`/`--audit`/`--policy` 的 CLI 开关，对所有写命令生效。

| 命令 | 优先级 | 用途 | 关键参数 |
| :--- | :--- | :--- | :--- |
| `lyre info <image>` | P0 | 只读：位深、尺寸、通道、校准、输入 SHA-256；不改文件 | `--json`/`--text` |
| `lyre measure <image> --roi <path>` | P0 | 单图测量，JSON 到 stdout | `--pixel-size-x`/`--pixel-size-y`（`--pixel-size` 为等值简写）、`--known-distance`/`--pixels`、`--unit`、`--z-step`、`--calibration`、`--roi`/`--roi-json`/`--roi-stdin`、`--whole-image`、`--measurements`、`--threads`、`--slice`、`--frame`、`--out` |
| `lyre export <image> --roi <path> --csv <path>` | P0 | 测量并写 CSV 与 sidecar | 同上，加 `--sidecar`、`--overwrite` |
| `lyre batch --manifest <path\|->` | P0 | 批处理，输出索引 JSON | `--jobs`、`--out-dir`、`--out-template`、`--timeout`、`--item-timeout`、`--max-items`、`--max-output-bytes`、`--overwrite` |
| `lyre schema` | P0 | 输出契约注册表：命令、参数、全部 Schema、退出码、错误码 | `--json`（默认）、`--schema-version` |
| `lyre version` | P0 | 输出 `toolVersion`、`contractVersion`、构建哈希 | `--json` |
| `lyre capabilities` | P0 | 输出 `lyre.agent/v1` 工具清单，放在信封 `data` | `--detail=summary\|schema\|full`、`--tool`、`--limit`、`--cursor`、`--plugins none` |
| `lyre doctor` | P0 | 环境自检：运行时、沙箱宿主、路径、权限、版本 | `--json` |
| `lyre calibrate set` | P0 | 只写显式 `--calibration-out` 文件，不写隐式会话 | `--image`、`--pixel-size`、`--known-distance`/`--pixels`、`--unit`、`--z-step`、`--calibration-out` |
| `lyre roi create\|convert` | P0 | 创建或转换 ROI：JSON 集合 ↔ `.roi/.zip` | `--roi`/`--roi-json`/`--roi-stdin`、`--type`、`--points`、`--center`、`--radii`、`--angle`、`--out`、`--overwrite` |

```bash
lyre capabilities --detail=schema --tool measure
lyre measure /data/sample.tif --roi /data/cells.zip --pixel-size 0.1 --unit um
lyre export /data/sample.tif --roi /data/cells.zip --csv /data/out/out.csv --sidecar auto --out-dir /data/out
lyre batch --manifest /data/files.json --out-dir /data/out --jobs 4 --cache-budget 2048
```

**测量范围**：v1 的 `measure`/`export` 只接受面积型 ROI（rectangle/polygon/ellipse/freehand）；point/line 可创建/转换，但测量入口返回 `usage.invalid`，不进 v1 数值结果。

**校准**：`--calibration` 文件自身必须是完整来源（x/y 比率与 unit 均非空），单独使用即合法；只给 x、只给单位或文件字段不全返回 `usage.invalid`。`--unit` 只在 CLI 覆盖时必填，文件与内嵌校准使用自带单位；优先级为 `--pixel-size-x/y`+`--unit` > `--calibration <file>` > 图像内嵌 > 缺失报 3。`--pixel-size-x`/`--pixel-size-y` 与 `--unit` 只在本次会话覆盖，`--pixel-size` 是 x=y 的简写；`--known-distance` 加 `--pixels` 是另一种写法。文件无校准且无覆盖时返回 3 与 `input.calibration.missing`。校准优先级固定为：`--pixel-size-x/y`/`--known-distance`+`--pixels` + `--unit` > `--calibration` 文件 > 图像内嵌 > 缺失报 3；物理单位必须显式 `--unit`，`source` 区分 embedded/calibration-file/cli-override/none，sidecar 记录校准文件 path 与 sha256。`lyre calibrate set` 只写显式 `--calibration-out` 文件，不提供隐式会话状态。

**ROI**：`--roi` 接受 ImageJ `.roi`/`.zip`；`--roi-json` 与 `--roi-stdin` 接受 `lyre.roi/v1` 集合，支持 rectangle、polygon、ellipse、freehand、point、line 六类；`roi create` 的 ellipse 用 `center`、`radii`、`angle`，rectangle 用两个对角点，freehand 为闭合点列，多边形按顶点顺序；坐标固定为像素，校准由图像或命令行覆盖决定。集合内每个 ROI 带稳定 `roiId`（内容哈希）、`name`、`z`、`t`；`composite` 用 `operator` 与 `children` 表示 AND/OR/XOR，`point` 允许一个或多个点，freehand 必须首尾闭合。

**输出**：`measure`/`export` 的 `data` 按 `lyre.measure/v1` 校验，`data.outputs[]` 按 `lyre.cli/v1.$defs.output`；export 必填 `path`、`sha256`、`bytes` 与 `sidecar`，sidecar 固定命名为 `<csv>.sidecar.json`；sidecar 与 measure data 必须记录 ROI 的 path、sha256、roiIds，CSV 数据行按 roiIds 同序，按 `lyre.sidecar/v1` 校验，`--sidecar` 只允许 `auto|on`。

**测量选项**：`--measurements` 取 `../requirements/measurement-baseline.md` §3 选项集的子集，缺省为全集；`Mode` 随 CLI-009 留到 P1，v1 不纳入该基线。`--threads` 缺省 1，`--slice` 支持 `n` 或 `all`，线程数与 slice 写入结果与 sidecar。ROI 的 z/t 为 0 或 null 表示未指定，施加到所选片；1 表示 1-based 片号。`--slice` 是 1-based Z 索引，`--frame` 是 1-based T 索引；2D 图缺省 z=1、t=1，T 栈缺省 t=1，Z 栈必须显式给 `--slice n` 或 `--slice all`，否则 `usage.invalid`。`--slice all` 只允许 measure 并输出逐片结果；export 与 batch 禁止 `all`，CSV 仅在单 slice 时导出。export/batch 只处理 z/t 为空或等于所选 `--slice`/`--frame` 的 ROI；不匹配的 ROI 在 warnings 或索引中显式标 skipped，一个都不匹配返回 `usage.invalid`。每个 measure 结果的 `slice{z,t}` 必填，sidecar 的 `options.slice{z,t}` 必填，batch 索引每项记录 `slice`/`frame`。

**批处理**：manifest 支持 `{"files":[...]}`（可省略 `schema`，缺省 v1）或 JSON Lines（每行一个 `files[]` 项），每项可含 `pixelSizeX`/`pixelSizeY`、`calibration` 文件、`slice`/`frame`（1-based 整数，不接受 all）；Z 栈项必须显式 `slice`，缺失按 item 返回 `usage.invalid`，`frame` 缺省 1。相对路径按 manifest 所在目录解析；`--out-template` 缺省 `{stem}_results.csv`，产物必须落在 `--out-dir` 内，而 `--out-dir` 必须 realpath 到安装策略固定的可写根内。`--jobs` 缺省 1，`--cache-budget` 单位 MiB，缺省为可用内存的 50%，硬上限 64 GiB；`--max-items` 缺省 1000、硬上限 10000；`--timeout` 缺省 30 分钟，`--item-timeout` 缺省 10 分钟；输出总量缺省 1 GiB。超预算返回 9。批处理终态 `termination` 必填：completed/max_items/max_output_bytes/timeout/cancelled/circuit_open/item_timeout；取消返回 7，单项超时按 item 记 failed/8，熔断后剩余项标 skipped、退出 8。输出冲突按目标文件系统大小写规则归一化后判定，冲突即 `output.exists`，`--overwrite` 同时作用于 CSV 与 sidecar。批处理无状态，不提供断点续跑；有状态批处理属 AUTO-003（P1）。退出码优先级为 6（超时）> 8（部分失败）> 9（资源不足）；`--max-items` 截断属正常完成，退出 0，索引标 `notAttempted`。

### 1.2 契约注册表、JSON 与退出码

`lyre schema` 发布命令、参数、横切 Schema、退出码与错误码；`lyre capabilities` 另行发布工具元数据，二者由同一构建生成并做一致性校验；MCP 与插件 CLI 是 v2 设计，不在 v1 契约、命令枚举与 capabilities 中登记。stdout 只输出 JSON（UTF-8 无 BOM、LF），日志走 stderr；`--events` 模式下 stderr 每行都是 `lyre.event/v1`，不再混入自由文本。

信封固定为：

```json
{
  "schema": "lyre.cli/v1",
  "contractVersion": "1.0",
  "toolVersion": "0.1.0+abc123",
  "command": "measure",
  "mode": "execute",
  "invocationId": "01J...",
  "planHash": "sha256:...",
  "ok": true,
  "data": { "input": { "path": "/data/sample.tif", "sha256": "…" }, "results": [], "outputs": [] },
  "warnings": [],
  "error": null
}
```

`capabilities` 使用同一信封，`data` 按 `lyre.agent/v1` 校验；`lyre schema` 的 `schemas` 登记 `lyre.registry/v1`、`lyre.args/v1`、`lyre.cli/v1`、`lyre.agent/v1`、`lyre.info/v1`、`lyre.version/v1`、`lyre.doctor/v1`、`lyre.measure/v1`、`lyre.sidecar/v1`、`lyre.roi/v1`、`lyre.batch.manifest/v1`、`lyre.batch/v1`、`lyre.calibration/v1`、`lyre.host/v1`；plan/event/audit 属 v1.1。

错误对象固定为 `{code, codeVersion, message, hint, hintCode, input, details, retryable, retryArgv, retryPlanHash}`；`code` 是跨版本封闭枚举，与 locale 无关；`retryArgv` 是可执行文件加参数数组，不经过 shell。

| code | 含义 | retryable |
| :--- | :--- | :--- |
| `usage.invalid` | 参数组合非法或未知参数 | false |
| `input.not_found` | 输入或 ROI 不存在 | false |
| `input.decode.corrupt` | 图像解码失败或损坏 | false |
| `input.calibration.missing` | 无校准且未提供覆盖 | false |
| `analysis.timeout` | 超过 `--timeout` | true，放宽超时 |
| `analysis.cancelled` | 收到取消 | true，显式重试 |
| `output.exists` | 目标存在且未 `--overwrite` | true，加 `--overwrite` |
| `output.write_failed` | 写 CSV/sidecar 失败 | true，换路径 |
| `output.outside_root` | 输出越出固定可写根 | false |
| `resource.exhausted` | 内存、并发或磁盘预算不足 | true，降 `--jobs`/`--threads` |
| `permission.denied` | 只读模式或被策略拒绝 | false |
| `consent.required` | 需要人工确认 | true，确认后重试 |
| `plan.stale` | planHash 与执行输入不一致 | false，重新 plan |
| `license.missing` / `license.expired` | 企业插件未授权或过期 | false |
| `plugin.unauthorized` | 插件命令被授权矩阵拒绝 | false |
| `plugin.failed` | 插件执行失败 | true，最多 3 次 |
| `sandbox.unavailable` | 宿主缺失或版本不匹配 | false |
| `sandbox.start_failed` | 宿主启动失败 | false |
| `sandbox.crashed` | 宿主执行中崩溃 | true，熔断 3 次 |
| `mcp.protocol` | MCP 版本或消息不合法 | false |
| `internal.unexpected` | 未分类内部错误 | false |

| 退出码 | 含义 |
| :--- | :--- |
| 0 | 成功 |
| 1 | 内部错误（含未捕获异常与 OOM） |
| 2 | 用法错误 |
| 3 | 输入错误（不存在、损坏、缺校准） |
| 4 | 分析错误（测量失败） |
| 5 | 输出错误（存在、越界、写入失败） |
| 6 | 超时（JSON 含部分结果） |
| 7 | 取消（JSON 含部分结果） |
| 8 | 批处理部分失败（索引逐项给 code） |
| 9 | 资源不足 |

### 1.3 v1.1 计划、事件与审计（不在 v1）

`--plan`/`--confirm`/`--events`/`--audit`/`--policy` 与 plan/event/audit 三个 Schema 推迟到 v1.1/M3（CLI-010 P1）。v1 的写命令不提供这些开关；审计由安装策略固定的后台 sink 自动记录，不需要 CLI 参数，也不开放路径。v1 最小 sink 契约：固定路径、append-only；写 start（exitCode/durationMs 为 null）并 fsync 后才允许写任何产物；结束写 end；sink 不可写先返回 `permission.denied` 且不得产生输出；容量上限与轮转由安装策略固定。pipeline 与 `.lyre` AnalysisRun 同属 v2。

### 1.4 安全、路径与资源

- 可写根由安装策略固定：Windows `%LOCALAPPDATA%\ImageLyre\out`，POSIX `$XDG_STATE_HOME/imagelyre/out`（未设时 `$HOME/.local/state/imagelyre/out`），macOS `$HOME/Library/Application Support/ImageLyre/out`；`--out-dir`、`--calibration-out`、`measure --out`、`roi --out`、`--csv` 与 sidecar 路径都必须 realpath 后落在该根内，且不得与任一输入同路径；v1 没有可扩大根的 CLI 开关；OCI 需挂载可写状态卷。审计 sink 在同一状态目录的 `audit/`，append-only，不开放路径参数。
- 输入与输出拒绝符号链接、硬链接、设备、FIFO 与 ADS；输出按原子提交写入（先写同目录临时文件再改名），CSV 与 sidecar 成组提交，任一侧失败则两者都不保留。
- 默认不联网；输入只读；无 `--cwd` 时相对路径拒绝；manifest 内相对路径相对 manifest 所在目录解析。
- 工具描述、图像元数据、插件返回值都是不可信数据，不进入指令位；`lyre.agent/v1` 用 `origin`、`publisher`、`signed`、`trust` 标注来源与信任级别。
- `--jobs` 上限为 `min(核数, 缓存预算 / 单任务峰值)`；批处理还有 `--timeout`、`--item-timeout`、`--max-items`、`--max-output-bytes`；超限返回 9 并保留已完成项。
- 重试策略：`sandbox.crashed` 最多 3 次并熔断；`plugin.failed` 最多 3 次；`resource.exhausted` 降并发；`permission.denied`、`consent.required` 不自动重试。

### 1.5 Agent 接口、MCP 与插件

- `lyre capabilities` 输出 `lyre.cli/v1` 信封，`data` 按 `lyre.agent/v1` 校验；工具项含 `id`、`name`、`summary`、`originKind`、`originRef`、`availability`、`inputSchema`、`outputSchema`、`sideEffects`、`permissions`、`prerequisites`、`produces`、`errors`、`examples[{argv,arguments,description,expectExitCode}]`、`retry`、`validation`、`limits`、`version`。
- 工具清单支持 `--detail=summary|schema|full`、`--tool`、`--limit/--cursor`；summary ≤ 8 KB，单工具 full ≤ 16 KB；工具名只允许 `^[a-z][a-z0-9_-]{0,63}$`，内置名不可被插件遮蔽。
- `inputSchema` 由 `lyre.args/v1#/$defs/<command>` 内联展开，`outputSchema` 由对应 data Schema 内联；二者必须是自包含 object Schema，禁止外部 `$ref`，输入侧用 `mode` 枚举代替 `oneOf`；示例必须能被对应 Schema 校验。
- `validation` 含 `imagejBaseline`、`toleranceClass`、`goldenDatasetVersion`、`reportUri`、`lastVerified`，让 agent 按可信度选工具。
- MCP 与插件 CLI 作为 v2 设计，不进入 v1 契约、command 枚举与 capabilities；v1 的 `originKind` 只有 `builtin`，`originRef` 为 v2 预留。MCP 的写入白名单、常驻宿主池与版本协商在 v2 设计评审中单独冻结。

### 1.6 分发与一致性

CLI-only 标准格式包为 NativeAOT 单文件 `lyre`（免安装 tar.gz 或 OCI 镜像），不含 AtomUI、示例数据与 Python，包体 ≤ 100 MB、`lyre version`/`lyre info` 冷启动 ≤ 1.5 秒；厂商格式与插件另带 `lyre-sandbox-host`（含 .NET 10 JIT 运行时，≤ 120 MB），两者合计 ≤ 220 MB，厂商格式冷启动单独计量；厂商格式与插件经 `lyre-sandbox-host`（JIT）执行，宿主缺失时标准格式仍可用，厂商格式报 `sandbox.unavailable`。SYS-007/SYS-011 增加 CLI 产物行，CI 断言 CLI 输出目录不含 `Avalonia*`、`AtomUI*`、`SkiaSharp*` 与 `ImageLyre.UI`。

CLI 与桌面的对照：同一构建、同一 `--threads`、同一选项集下，JSON 指标按 `../requirements/measurement-baseline.md` §6 的 B 类 `1e-9` 相对差，CSV 按同文件 §7 逐字节，sidecar 只比稳定字段（排除 timestamp，保留 buildHash、inputSha256、threads、options）；`buildHash` 是产品级（提交 + 契约版本，CLI 与桌面相同），`artifactHash` 记录单产物哈希。VAL-004 增加「非默认校准 + 非默认测量选项」的 CLI ↔ 桌面用例。

| 编号 | 需求与验收要点 | 优先级 |
| :--- | :--- | :--- |
| CLI-001 | 纯 CLI 闭环：`info` + `measure`（含校准覆盖）+ `export` 在无显示、无 TTY 环境完成；验收：CI 脚本跑通并产出 JSON/CSV/sidecar | P0 |
| CLI-002 | 子命令与参数契约：`info`、`measure`、`export`、`batch`、`schema`、`version`、`capabilities`、`doctor`、`calibrate`、`roi`；参数、必选/互斥、默认值、输出与退出码冻结；未知参数退出 2 | P0 |
| CLI-003 | 机器契约：默认 JSON 信封、`lyre.cli/v1`、`contractVersion`、封闭错误码、退出码 0–9、`lyre schema` 注册表与全部 Schema 文件；stdout 只含 JSON | P0 |
| CLI-004 | AI 调用约束：无交互提示；支持 stdin ROI/清单；`--timeout` 与取消产出部分结果与专门退出码；同一输入重复执行一致；不依赖 locale 与 TTY | P0 |
| CLI-005 | 批处理：manifest（JSON/JSONL）、`--jobs`、`--cache-budget`、`--timeout`/`--item-timeout`、`--max-items`、`--out-template`、冲突与覆盖、索引按输入序、部分失败 8；无状态 | P0 |
| CLI-006 | 与桌面一致：同一构建、同一线程数、同一选项集下，JSON 按 B 类容差、CSV 逐字节、sidecar 稳定字段一致；含非默认校准用例 | P0 |
| CLI-007 | 安全与路径：默认不联网、输入只读、`--out-dir` 落在固定可写根、相对路径需 `--cwd`、覆盖需 `--overwrite`、无隐式配置 | P0 |
| CLI-008 | Agent 能力发现：`capabilities` 走信封且 `data` 按 `lyre.agent/v1` 校验，含输入/输出 Schema、副作用、权限、前置、产出、错误、示例、验证元数据；支持分页与 detail | P0 |
| CLI-009 | 全能力 CLI（P1/M3）：以 PRD 4.3 §7.1 首个更新的算子范围为口径，覆盖阈值、滤波、栈投影、通道拆分等；v1 只承诺分析闭环（info/measure/export/calibrate/roi/batch） | P1 |
| CLI-010 | 计划、事件与审计（v1.1/P1）：`lyre.plan/v1`、`lyre.event/v1`、`lyre.audit/v1` 的 CLI 开关与回放；v1 不提供 | P1 |
| CLI-011 | MCP 服务（v2）：`lyre mcp serve --stdio` 暴露同一工具与资源，默认只读、`--allow-write` 白名单、不联网、版本协商与 task/cancel；不在 v1 契约 | P1 |
| CLI-012 | 插件 CLI 扩展（v2）：插件注册带 Schema 的命令，强制沙箱，带授权与 hostApiRange；仅 headless-safe 槽位；不在 v1 契约 | P1 |
