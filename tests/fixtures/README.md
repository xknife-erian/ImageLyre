# tests/fixtures/

测试数据目录。真正的 fixture 由 `docs/spec/m1/M1-imagej-fixture.md` 定义，入库后按下面的结构存放；本文件只说明落点，不代替 fixture 规格。

```text
tests/fixtures/
├── imagej-1.54f/          # ImageJ 1.54f 对照基线与黄金数据集
│   ├── manifest.json      # 安装包 SHA-256、Java 与 OS 版本、生成日期、执行人
│   ├── measure-options.ijm
│   ├── measure-all.ijm
│   ├── golden-v0.1/       # 8 张合成图与同名 ROI zip
│   └── baseline/          # 由 ImageJ 导出的基准 CSV
└── cache/                 # 本机缓存的 ImageJ 安装包，体积大，不入库
```

约定：

- `cache/` 不进版本库（见 `.gitignore`），需要离线复现时由执行人放到本机；
- `golden-v0.1/` 与 `baseline/` 一旦入库即冻结，改动必须升 fixture 版本并在 `manifest.json` 记录；
- 测试通过相对路径引用 fixture，不写机器绝对路径，保证换机器可复现；
- 合成脚本与数据许可说明随 fixture 存放。
