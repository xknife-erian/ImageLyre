# ADR-0006 CLI 一等入口、Headless 会话层与 AOT/JIT 边界

| 项 | 值 |
| :--- | :--- |
| 状态 | 已接受 |
| 日期 | 2026-11-19 |
| 决策人 | lukan |
| 关联 | PRD 2.5 §5.17、ADR-0004、启动需求 §8.5 |

## 1 背景

PRD 2.5 要求 `lyre` 在无显示、无 TTY 的环境完成分析闭环，并供 AI 与外部流水线调用。现有架构基线把 UI 层定为 AtomUI + Avalonia，把厂商格式解码与插件执行放在沙箱进程；CLI 既不能依赖 UI 层，也不能假设插件宿主与自身使用同一运行时。若 CLI 与桌面各自实现一遍「打开 → 校准 → ROI → 测量 → 导出 → sidecar」的编排，CLI-006 的逐值一致只能靠人工比对，无法作为门禁。

同时，§8.5 只写「NativeAOT 仅用于主启动器与 CLI」，没有记录 CLI 与插件宿主的进程/运行时边界，也没有定义 CLI-only 分发物。ADR-0004 的沙箱模型与 AOT 边界需要在同一份记录里对齐。

## 2 决策

1. **新增 headless 会话层**：把校准、ROI 解析、测量选项、CSV/sidecar、批处理编排收敛到不依赖 UI 的会话层；Desktop 与 CLI 只负责参数解析、呈现与进程生命周期，二者共享该层作为唯一结果来源。
2. **CLI 作为独立应用**：`src/Apps/ImageLyre.Cli` 只引用 Kernel、Imaging、Codecs、Plugins、Sandbox 与 headless 会话层，不引用 AtomUI、Avalonia、SkiaSharp 或 `ImageLyre.UI`。
3. **AOT/JIT 分工**：`lyre` 使用 NativeAOT 单文件分发；`lyre-sandbox-host` 使用 JIT，承载厂商格式解码与插件执行。CLI 在主进程内处理标准 TIFF/BigTIFF 与内置算子；需要宿主时按 ADR-0004 启动沙箱，宿主缺失或版本不匹配返回 `sandbox.unavailable`，标准格式仍可降级运行。
4. **唯一无头入口**：`lyre` 是唯一无头入口，AUTO-004 的流水线执行复用 `lyre run`，不再另建第二套无头入口；Python 脚本执行仍由 AUTO-001/002 提供，CLI 不依赖 Python。
5. **CLI-only 分发**：提供 NativeAOT 单文件、免安装 tar.gz 与 OCI 镜像，不含 AtomUI、示例数据与 Python；CLI 包体 ≤ 100 MB、冷启动 ≤ 1.5 秒，与桌面安装包分开计量。
6. **契约先行**：`lyre.cli/v1` 的 JSON Schema、退出码与错误码随 `lyre schema` 发布，并入库为 `docs/spec/lyre.cli.v1.schema.json`；契约只增字段，主版本不兼容时拒绝运行。

## 3 备选方案

- **复用桌面可执行文件加 `--headless`**：需要加载 Avalonia/AtomUI 依赖，启动慢、体积大，且难以在 CI 容器中裁剪，否决。
- **CLI 全 JIT、与宿主同运行时**：实现简单，但单文件分发与冷启动不达标，也无法与 GUI 的 AOT 启动器共用策略，否决。
- **CLI 直接调用 Python 脚本层**：与「无需预装 Python」冲突，且把 AI 调用绑到 Python 环境，否决。

## 4 后果

- 需要维护 `lyre`、`lyre-sandbox-host` 两个产物及版本协商；CI 增加「CLI 输出目录不含 Avalonia/AtomUI/SkiaSharp/ImageLyre.UI」的引用断言。
- headless 会话层成为桌面与 CLI 的共同测试面，VAL-004 增加非默认校准与非默认测量选项的对照用例。
- `src/ImageLyre.slnx` 需登记 CLI 工程与测试工程；SYS-007/SYS-011 增加 CLI 产物行与体积计量。
- 厂商格式在宿主缺失时只能降级，用户会看到 `sandbox.unavailable` 与可执行的安装提示；这是有意的失败模式，避免静默给出错误结果。

## 5 参考

- `docs/spec/product-requirements.md` §5.17、§8.5、§12
- `docs/spec/lyre.cli.v1.schema.json`
- `docs/adr/lukan/ADR-0004-sandbox-data-path.md`
- `docs/spec/startup-requirements.md` §8.5
