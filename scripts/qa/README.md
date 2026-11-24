# scripts/qa

## 用途

M1 的只读质量检查入口：`verify-m1.ps1` 在完整树上核对引用方向、机制层词表、通配引用计数、工程 README、平台专属符号、壳契约纯度与 Bootstrap 无 UI 引用七类约束，把每项结果打印成 `PASS`/`FAIL` 并给出原因；任一项失败时退出码为 1。脚本不修改任何文件，也不依赖当前工作目录。

## 运行

```powershell
pwsh scripts/qa/verify-m1.ps1
```

退出码：`0` 表示七项全部 PASS；`1` 表示至少一项 FAIL。脚本头注释是运行说明与检查项清单的同一份真源，新增检查项时必须同步更新头注释、脚本与本文。

## 七类检查

1. **引用方向**：`src/Plugins` 下插件 csproj 不引用 `ImageLyre.Module.*`/`ImageLyre.Feature.*`；三个契约工程不引用 Avalonia、AtomUI、SkiaSharp、`Cangming.*` 或任何插件工程。
2. **词表**：剥离 `//`、`/* */`、`///` 注释后，检索 `ImageLyre.Bootstrap` 与 `ImageLyre.App` 的标识符与字符串字面量，禁止图像、image、ROI、选区、测量、measure、分析、analysis、通道、像素、pixel、TIFF、标尺、校准。
3. **通配引用计数**：`ImageLyre.App.csproj` 按名引用插件为 0，指向 `Plugins` 的通配 `ProjectReference` 恰为 1。
4. **README**：`src` 与 `tests` 下每个含 `*.csproj` 的目录（排除 `src/cangming`）都有 `README.md`。
5. **平台符号**：契约、领域与插件源码不命中 `Microsoft.Win32.Registry`、`DllImport`、`System.Windows.Forms`、盘符字面量与反斜杠拼接。
6. **壳契约纯度**：`ImageLyre.Shell.Contracts.csproj` 只允许唯一一个到 `ImageLyre.Contracts` 的 `ProjectReference`，且没有 `PackageReference`。
7. **Bootstrap 无 UI 引用**：`ImageLyre.Bootstrap.csproj` 的 `ProjectReference` 不含 `ImageLyre.Presentation.*`，`PackageReference` 不含 Avalonia/AtomUI，保证 Host 阶段可脱离 UI 运行。

## 口径与后续

`ponytail`：七类检查是 M1 的最小集；G2 之后扩到依赖树与清单校验。故障样本 `ImageLyre.Module.Broken` 只进 Debug 输出是 M1 约定，G4 装包后由发行清单取代。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` 的验收 E 与 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T16，再改 `verify-m1.ps1` 头注释、脚本检查函数与本文件；不要在脚本里写死仓库盘符或当前目录。
