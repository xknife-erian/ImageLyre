# UnitTest.ImageLyre.Presentation.Desktop

## 意义

本工程是第 8 套测试，守住 `ImageLyre.Presentation.Desktop` 的桌面 Presentation 契约：`PresentationPlan` 的组与顺序、桌面启动选项的映射、欢迎窗到主窗体的装配顺序，以及桌面 `StopPlan` 的释放语义。它把“桌面启动到底按什么顺序发生”变成可断言的数据，而不是只能靠启动日志与人工点窗口确认。

T01 阶段本工程只有工程骨架与 README，尚无测试类；测试随 T05–T07 的 Presentation 骨架与 Plan 实现一并补入，先写失败测试再写实现。

## 边界（不放什么）

- 不放真实窗口：不启动 Avalonia、不创建 `Window`、不读真实插件产物，桌面行为由 `scripts/qa/smoke-m1.ps1` 覆盖。
- 不放 Bootstrap 内部测试：Host 构建、插件启动与 `ExitArbiter` 在 `tests/UnitTest.ImageLyre.Bootstrap`，本工程只测 Presentation 侧。
- 不放 App 入口测试：参数解析与桌面/CLI 分派属于 `tests/UnitTest.ImageLyre.App`。

## 依赖方向

引用 `ImageLyre.Presentation.Desktop`、`xunit.v3`、`xunit.runner.visualstudio`、`Microsoft.NET.Test.Sdk` 与 `FluentAssertions`；不引用 `ImageLyre.App`、不引用任何 UI 或插件工程。被 `src/ImageLyre.slnx` 引用。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.Presentation.Desktop\bin\Debug\net10.0\UnitTest.ImageLyre.Presentation.Desktop.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而以握手失败退出，这不是测试本身的问题；runner 已由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0002-启动宿主与编排模式.md` §7.5、§10.2 与 `docs/plans/lukan/PLAN-0002-启动宿主与编排模式实施.md` 的 T05–T07、T10 及本文件；桌面行为口径变化同时改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §7 与 `scripts/qa/smoke-m1.ps1` 的断言。
