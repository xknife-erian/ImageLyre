# M1 ImageJ fixture 与黄金数据集

| 版本 | 日期 | 状态 | 作者 | 说明 |
| :--- | :--- | :--- | :--- | :--- |
| 1.0 | 2026-11-19 | 待执行 | lukan | 定义 M1a 的 ImageJ 对照环境、fixture 组成与黄金数据集 v0.1；执行后回填校验和与生成日期 |

## 1 目的与范围

M1a 的验收不能靠「看起来差不多」，必须有一份可重复的 ImageJ 对照基线。本文定义这份基线由什么组成、在哪里存放、由谁生成、怎样判定它合格；它只覆盖 M1a 需要的标准 TIFF、8/16 位、单/多通道、带/不带校准、矩形与多边形 ROI、面积/均值/标准差/最小/最大/周长六项指标，不覆盖厂商格式、栈、颗粒分析与插件。

没有这份 fixture，M1a 的 CSV 对照与 VAL-002 都无法判定，因此它是 M1a 编码的前置条件，而不是收尾工作。

## 2 基准环境

| 项 | 基线值 | 说明 |
| :--- | :--- | :--- |
| ImageJ | 1.54f 官方发行版 | 只认 Wayne Rasband 官方包，不用 Fiji 代替测量基线 |
| Java | Java 8 | 与 ImageJ 1.54f 配套；版本写入 manifest |
| 操作系统 | Windows 10 22H2 | 基准机；其他平台结果另记，不混入基线 |
| 显示 | 100% 缩放，无 LUT | 避免显示状态影响测量 |
| 测量选项 | 见 3.2 的模板 | 逐项冻结，不得在执行时临时勾选 |
| 导出 | Results 表另存 CSV | 使用 ImageJ 默认分隔与换行，不做二次编辑 |

ImageJ 安装包的 SHA-256、Java 版本、操作系统版本与生成日期写入 `tests/fixtures/imagej-1.54f/manifest.json`；任何一项变化都必须重新生成基线并升 fixture 版本。

## 3 Fixture 组成

### 3.1 文件清单

| 文件 | 内容 | 来源 |
| :--- | :--- | :--- |
| `imagej-1.54f.zip` | 官方发行包；**不入库**，只记录下载 URL 与 SHA-256 | ImageJ 官网 |
| `manifest.json` | 安装包 SHA-256、下载 URL、Java 与 OS 版本、生成日期、执行人 | 生成时写入 |
| `measure-options.ijm` | 设置 Set Measurements 与 Set Scale 的宏 | 本文 3.2 模板 |
| `measure-all.ijm` | 遍历 ROI 集合并测量、导出 CSV 的宏 | 本文 3.3 模板 |
| `golden-v0.1/` | 8 张测试图与 8 个 ROI zip | 合成生成，见第 4 节 |
| `baseline/` | 每张图对应的 ImageJ 基准 CSV | 由宏导出 |
| `README.md` | 复现步骤与已知偏差 | 生成时写入 |

安装包体积大且属于外部软件，不进入 git；`manifest.json` 记录官方 URL 与 SHA-256，需要离线复现时由执行人放到本机缓存目录 `tests/fixtures/cache/`，该目录不提交。

### 3.2 测量选项模板

`measure-options.ijm` 必须逐项设置并保存以下状态：

- 勾选：Area、Mean、StdDev、Min、Max、Perim.、IntDen、Median、Skew、Kurt、X、Y、Feret、FeretAngle、MinFeret、Circ.、N。
- 数值：小数位 3；科学计数法关闭；NaN 空单元开启。
- 开关：Limit to threshold 关闭；Invert Y 关闭；Black background 关闭；Redirect 无。
- 校准：每张图按第 4 节的标定值执行 Set Scale，不沿用上一张图的校准。

M1a 只使用 Area、Mean、StdDev、Min、Max、Perim. 六列；其余列保留在 fixture 中，供 M1b 及后续版本使用，执行时不得删列。

### 3.3 测量与导出

`measure-all.ijm` 对每张图执行：打开图像 → 打开同名 ROI zip → 选择全部 ROI → Multi Measure → 保存 Results 为 CSV。CSV 文件名与图像同名，放在 `baseline/` 下；导出后不得用 Excel 打开再保存，以免改变小数点、编码或行尾。

## 4 黄金数据集 v0.1

8 张图全部为合成图，像素值与 ROI 由脚本生成，避免第三方图像授权问题；每张图配一个 `.zip` ROI 集合，合计 42 个 ROI，满足 6×6 的最低对照规模。

| ID | 文件 | 位深 | 尺寸 | 校准 | 通道 | ROI 数 | 用途 |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| G01 | `g01-8bit-gradient-512.tif` | 8 | 512×512 | 无 | 1 | 3 矩形 | 无校准下的像素单位 |
| G02 | `g02-8bit-shapes-512.tif` | 8 | 512×512 | 无 | 1 | 3 矩形 + 3 多边形 | 已知面积的形状 |
| G03 | `g03-16bit-gradient-1024.tif` | 16 | 1024×1024 | 0.1 µm/px | 1 | 3 矩形 | 16 位与物理单位 |
| G04 | `g04-16bit-shapes-1024.tif` | 16 | 1024×1024 | 0.1 µm/px | 1 | 3 矩形 + 3 多边形 | 校准后的面积与周长 |
| G05 | `g05-16bit-noise-512.tif` | 16 | 512×512 | 0.2 µm/px | 1 | 6 矩形 | 噪声下的均值与标准差 |
| G06 | `g06-8bit-two-channel-512.tif` | 8 | 512×512 | 无 | 2 | 6 矩形 | 多通道读取与通道选择 |
| G07 | `g07-16bit-anisotropic-512.tif` | 16 | 512×512 | 0.1×0.2 µm/px | 1 | 6 矩形 | 各向异性像素 |
| G08 | `g08-8bit-edgecases-256.tif` | 8 | 256×256 | 无 | 1 | 6 混合 | 贴边 ROI、空选区、单像素 ROI |

每张图的像素内容与 ROI 坐标在生成脚本中固定，生成后不再手工修改；G02 与 G04 的矩形面积分别取 5000 与 20000 像素，多边形取直角折线，便于人工核对。

## 5 生成步骤

1. 在基准机安装 ImageJ 1.54f，记录安装包 SHA-256、Java 版本与 OS 版本。
2. 运行 `measure-options.ijm`，逐项核对 3.2 的选项状态，截图留档。
3. 用合成脚本生成 8 张 TIFF 与 8 个 ROI zip，写入 `tests/fixtures/imagej-1.54f/golden-v0.1/`。
4. 逐张打开图像，按第 4 节设置校准，保存为 TIFF（保留校准标签），再重新打开确认校准值。
5. 运行 `measure-all.ijm`，把 Results 导出到 `baseline/`。
6. 计算全部文件 SHA-256，写入 `manifest.json`；记录生成人、复核人与日期。
7. 由另一位同事在干净机器上按同一流程重跑一遍，只比较解析后的数值，不比较字节；差异写入 `README.md`。

## 6 验收标准

1. `manifest.json` 中的 8 张图、8 个 ROI zip、8 份基准 CSV 全部存在，SHA-256 一致；安装包只校验 URL 与哈希，不要求入库。
2. 每张图的位深、尺寸、校准与第 4 节表格一致；G07 的 X/Y 校准不相等。
3. 基准 CSV 的列名、列顺序与 3.2 的选项集一致；M1a 的六项指标齐全。
4. 第二台机器重跑后，面积与像素计数完全一致；均值、标准差等浮点量与基准的相对差不超过 `1e-6`。
5. fixture 中不包含任何无授权第三方图像；合成脚本与数据许可说明随 fixture 存放。
6. 复核人签署「可复现」结论；未签署的 fixture 不得用于 M1a 验收。

## 7 角色与时间

| 角色 | 人 | 职责 |
| :--- | :--- | :--- |
| 负责人 | 产品经理 | 冻结选项与数据集范围，验收 manifest 与复核结论 |
| 执行人 | QA/测试 | 安装 ImageJ、生成图像与 ROI、导出 CSV、计算哈希 |
| 复核人 | 研发 | 在干净机器重跑，比较解析值，签署结论 |

计划在 M1a 第 1–2 天完成；执行人可在第 1 天先交安装包哈希、选项截图与图像清单，第 2 天交完整 fixture。

## 8 风险

| 风险 | 影响 | 应对 |
| :--- | :--- | :--- |
| ImageJ 版本或 Java 版本漂移 | 基准不可复现 | 安装包与 Java 版本写入 manifest，升级必须升 fixture 版本 |
| 选项模板被临时改动 | CSV 列不一致 | 每次生成前重跑 `measure-options.ijm` 并截图 |
| Excel 二次保存改变格式 | 小数点、编码或行尾漂移 | 基准 CSV 只由 ImageJ 导出，禁止 Excel 另存 |
| 合成图与真实显微图差异大 | 无法覆盖真实噪声 | v0.1 先保证可判定，v0.2 再加入实验室授权样本 |
| 各向异性像素坐标换算错误 | 面积与 Feret 偏差 | G07 专门覆盖，测试先行 |

## 9 交付物

- `tests/fixtures/imagej-1.54f/` 下的完整 fixture 与 `manifest.json`；
- `docs/spec/m1/M1-acceptance-matrix.xlsx` 中 M1a 行的状态回填；
- 本文件的执行记录（生成日期、执行人、复核人、哈希）；
- 若发现 PRD 附录 B.1 与实际 ImageJ 输出冲突，提交勘误并在本文记录裁决。
