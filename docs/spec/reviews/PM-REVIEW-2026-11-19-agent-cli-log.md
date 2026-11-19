# Agent CLI 多轮评审日志

| 轮次 | 日期 | 对象 | 结论 |
| :--- | :--- | :--- | :--- |
| R1 | 2026-11-19 | PRD 2.6 Agent 接口 + 两份 Schema | 6 位专家全部 blocker |
| R2 | 2026-11-19 | PRD 2.7 + 契约 Schema 文件 | 待评 |

## R1 评审结论

| 角色 | 结论 | 主要问题 |
| :--- | :--- | :--- |
| Agent 工具协议架构师 | blocker | `lyre.cli/v1.command` 未扩到 capabilities/doctor/calibrate/roi；capabilities 是信封还是裸文档未定；缺 plan/event Schema；工具清单分页/版本/去重缺失 |
| 桌面与运行时架构师 | blocker | 新增 P0 命令与信封枚举冲突；MCP 常驻与宿主池未定义；CLI-009 范围不可清点；batch 的 cache-budget/超时/崩溃码缺失 |
| 方法开发者 | blocker | P0 分析算子/栈/时序无 CLI 表达；run/plan/event 无 Schema；batch 索引缺输出 SHA/sidecar；ROI 只四类；插件/Python 边界不清 |
| 安全与自治评审 | blocker | 工具描述/图像元数据/插件返回值无信任边界；写入缺 out-root/realpath/symlink 约束；plan 与执行无绑定；审批/拒绝错误码缺失；batch 可被放大 |
| 日常分析者与 AI 用户 | blocker | capabilities 输出无法被自家 Schema 校验；plan/events/部分结果无契约；retryCommand 跨 shell 不安全；batch 大小写冲突；calibrate set 无落点 |
| 产品策略与竞争评审 | blocker | 差异化仍是可复制的功能清单；72 P0 与产能不匹配；指标 n=20 不可测；MCP/企业插件授权未映射；2.6 自述待评审却标定稿 |

## R1 到 2.7 的处置

1. **契约注册表**：`lyre schema` 成为唯一注册表；`lyre.cli/v1.command` 扩到 info/measure/export/run/batch/schema/version/capabilities/doctor/calibrate/roi/mcp/plugin；capabilities 走信封，`data` 按 `lyre.agent/v1` 校验；新增 plan/event/audit/pipeline/batch.manifest/batch.index/roi/calibration Schema 文件。
2. **工具清单**：`lyre.agent/v1` 增加 `contractVersion`、`registryVersion`、`schemaHash`、分页；工具增加 origin/publisher/signed/trust、prerequisites、produces、errors、examples.arguments、retry、validation、limits、requires；允许扩展字段。
3. **计划与执行绑定**：`lyre.plan/v1` 输出 reads/writes/planHash；执行需 `--confirm <planHash>`；plan/execute 共用解析器；plan 零写入进 §12。
4. **事件与审计**：`lyre.event/v1` 一行一对象，stderr 纯 NDJSON；`lyre.audit/v1` 记录 invocationId/parentId/planHash/decision，拒绝与确认也记录。
5. **安全**：`--out-root` 可写根、realpath、拒绝符号链接/设备/ADS；工具描述与元数据为不可信数据；MCP 默认只读，`--allow-write` 白名单、网络恒 false；插件强制沙箱。
6. **批处理**：`--cache-budget`、`--timeout`、`--item-timeout`、`--max-items`、`--max-output-bytes` 进参数表；索引补 inputSha256/outputSha256/sidecar/message/retryArgv；大小写冲突归一化。
7. **范围与指标**：M2 拆 M2a/M2b；新增 VAL-008（30 任务、3 模型 × 3 seeds、holdout、Wilson 下限、plan 保真/审计完整）；计划/事件/审计均进门禁。
8. **竞争**：差异化改为「可验证的定量结果契约」，工具清单加 validation 元数据；VAL-008 用 ImageJ 包装层做对照。

## R2 待验证

- 6 位专家是否接受上述处置，是否仍有 blocker/major；
- 契约文件与 PRD 是否逐条一致（command 枚举、Schema 注册表、错误码、side effects）；
- M2a/M2b 与 73 P0 的产能是否仍有不可执行处；
- VAL-008 的语料、模型版本、holdout 与门禁是否可复现。


## R3–R6 结果

| 轮次 | 对象 | 结果 |
| :--- | :--- | :--- |
| R3 | PRD 2.8 v1 范围收口 | 1 位 sign_off（产品），5 位 blocker/major |
| R4 | 移除 run/pipeline、10 命令 | 1 位 sign_off（产品），5 位 blocker |
| R5 | 补 sidecar/measure/host、审计 | 1 位 sign_off（Agent 协议），5 位 blocker |
| R6 | 补 policy/host pin/sidecar/calibration | 1 位 sign_off（Agent 协议），5 位 blocker |

## R6 未收口 blocker

1. **CLI-only 权限策略**：`--policy` 仍是全局参数，与「agent 不能覆盖策略」冲突；免安装包没有默认 grantedRoots 与授予通道，export 在 CI 无法过。
2. **注册表**：缺 info/version/doctor 输出 Schema、缺逐命令参数 Schema；registry 的 inputSchema/outputSchema 只是字符串。
3. **plan 载体**：`mode=plan` 时 data 应是 `lyre.plan/v1`，但 CLI 信封与 registry 只定义执行模式 data。
4. **宿主字段**：ADR 要求 plan/audit/doctor 都有 hostPath/hash/version/protocol，plan 与 host schema 缺字段，planHash 不覆盖 hostHash。
5. **sidecar/calibration/outputs**：VAL-001 字段与 sidecar Schema 仍不完全一致；measure outputs 未引用 `$defs.output`；校准 source 语义前后矛盾。
6. **options/point/line**：`--options` 依赖 P1 LAB-001；point/line 已收窄为只创建/转换，但 measure 拒绝语义需写死。

## 结论

2.8 仍是评审稿，未达到六位全部 sign_off。建议 v1.1 做一次范围收口：删除 `--policy`、`--plan`/`--confirm`/`--events`/`--audit`/`--options` 的 v1 承诺，把 CLI-010 移到 P1/M3；只保留 info/measure/export/batch/schema/version/capabilities/doctor/calibrate/roi 十条命令与固定审计 sink。这样可一次性移除上述 1、3、5、6 中的多数 blocker，再复审一次即可收口。
