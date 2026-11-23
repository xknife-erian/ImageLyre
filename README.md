# ImageLyre

> **状态（2026-11-23）**：2022 年 WPF 版与 2026 年 11 月的两轮重建已整体归档，归档前的完整状态在标签 `archive/pre-reset-2026-11-22`；M1 插件骨架已交付——12 个工程、5 套测试 44 项全绿、A–G 验收通过。插件架构基线见 [ADR-0001](docs/adr/lukan/ADR-0001-插件机制架构基线.md) 与 [ADR-0002](docs/adr/lukan/ADR-0002-平台契约抄录与本地演进.md)，M1 骨架见 [DESIGN-0001](docs/design/lukan/DESIGN-0001-M1-插件骨架.md)，实施与验收见 [PLAN-0001](docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md)。

ImageLyre 是显微镜旁的第一块屏幕。

它要做的不是把 ImageJ 换一层皮，而是把「看图 → 校准 → 选区 → 测量 → 复核 → 导出」这条路重新铺一遍：让研究者更快拿到可信的数字，让老用户不必重新学一套软件，让 Agent 能看见上下文、提出可审阅的下一步。

一盏灯，一张台面，一枚干净的读数。

## 为什么做

ImageLyre 的起点是 [ImageJ](https://imagej.nih.gov/ij/)。向它的作者 [Wayne Rasband](https://imagej.net/people/rasband) 致敬——一个人、一间实验室、一套软件，几十年。

作者 [稀布老咖](https://gitee.com/xknife) 想再造一次车轮，理由并不复杂：

一、ImageJ 的界面与人机交互，今天看仍有大量可以重新设计的地方；二、Java 写桌面应用，终究笨拙了一些；三、作者做了二十余年技术，工作又与科研图像、显微图像纠缠很深，想把那些经验认真用一次。

2022 年春天，这个项目在北京的家中开始。

## 做什么

Core 1.0 只保证四件事：**打开图能测、测完敢信、老用户能迁、插件能长。** 其余能力进官方更新，或者交给插件。

具体是十五个产品结果：

- **看见**：首启即可开始；标准 TIFF/BigTIFF 与元数据；画布缩放、平移与像素探针。
- **测量**：矩形/多边形 ROI 与 `.roi` 往返；校准；面积、均值、标准差、最小、最大、周长六项；CSV 与 sidecar 可复现。
- **可逆**：撤销、自动保存、崩溃恢复；结果版本、对比与回退。
- **界面**：浅色/深色/跟随系统；中英双语；键盘全路径、对比度与应用内缩放。
- **可扩展**：插件即插即用；ImageJ 迁移壳；CLI 与 Agent 的一次可信分析。

Windows 先发，macOS 与 Linux 随后；三端共用同一套设计与同一套插件。核心路径离线可用。

## 怎么建

.NET 10。界面以 AtomUI 为一等公民，Avalonia 做平台底座，SkiaSharp 只负责图像视口、画布变换与 ROI 叠加。

编解码以 SimdCodecs 为主，BioFormats.NET 作为随包插件、默认启用可禁用。插件机制以 cangming 的 `PicoPlugins` 为基座——只借它的发现、生命周期、依赖与槽位，不把它的 Shell 当应用内核。

“一切皆为插件”不是口号，是三件事：平台能力（文档、命令、设置、通知、主题）是插件；编解码、算子、测量、视口是官方必装插件；界面功能是表达插件。官方套装随发行包预装，但在机制上与第三方插件完全一样，可以禁用、替换、按变体裁剪。契约层只留纯 C# 的数据与格式模型：`ImageContainer`、`Calibration`、`ROI`、CSV。

桌面与 CLI 是同一个入口的两种宿主模式，同一套结果。

## 与 ImageJ 的关系

保留的是操作序列与术语：菜单层级、工具栏分组、ROI 手势、测量流程、常用快捷键。不保留的是 Swing 外观与 Java 实现。

老用户第一次打开，应该觉得“还是那套逻辑，但顺手多了”；而不是“我又要学一套软件”。改动的地方要能查到新旧对照，争议交互要留一条一键回旧路径。

## 现在到哪

2026-11-22 的仓库重置把两轮重建的代码整体移出工作区，只留下需求、基线与目录骨架；2026-11-23 的 M1 在这套骨架上落成：`src/ImageLyre.slnx` 是唯一构建入口，14 个工程（含一个只随 Debug 输出的故障样本）按 `App`、`Infrastructure`、`Plugins` 归置，七套测试共 61 项全绿，`scripts/qa/verify-m1.ps1` 的六项检查与 `scripts/qa/smoke-m1.ps1` 的冒烟通过。菜单注册表与第一个菜单功能「帮助 → 关于」已落地，插件贡献菜单不再需要改壳。cangming 以只读子模块锁定 tag `v0.1.9.1122` 作为源码对照与版本锚点，编译只引用同名 NuGet 包，不把子模块工程引进解决方案。M1 不含任何产品能力；图像、ROI、测量与文档留到第一批真实能力包，它们会按同一套插件机制接入。

想读什么，从这里进：

- 产品定位与十五个结果：[PRD 4.4](docs/spec/requirements/product-requirements.md)
- 架构基线：[startup-requirements.md](docs/spec/baseline/startup-requirements.md)
- 插件机制：[ADR-0001](docs/adr/lukan/ADR-0001-插件机制架构基线.md)
- M1 骨架：[DESIGN-0001](docs/design/lukan/DESIGN-0001-M1-插件骨架.md)
- M1 实施：[PLAN-0001](docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md)
- 外观与交互：[product-appearance.md](docs/spec/requirements/product-appearance.md)、[interaction-requirements.md](docs/spec/requirements/interaction-requirements.md)

## 写给未来

作者说，最初真正推着他动手的，是读到 Wayne Rasband 简介时的那一点羡慕：一个人把一件事做了一辈子，做得足够好，好到全世界都在用。

那就开始吧。**但行好事，莫问前程；前路有光，初心莫忘。**

## 授权

ImageLyre 的软件与源代码免费提供，遵循[木兰宽松许可证 v2](http://license.coscl.org.cn/MulanPSL2)。

## 附记

作者 [稀布老咖](https://gitee.com/xknife) 的“稀布”是“西部”的谐音。十余年前，母亲申请 QQ 时想给自己起名“西部沙子”，拼音不太灵光，打成了“稀布沙子”，便一直用下去。母亲已去世多年，音容宛在。借这个谐音纪念她，感念养育之恩。
