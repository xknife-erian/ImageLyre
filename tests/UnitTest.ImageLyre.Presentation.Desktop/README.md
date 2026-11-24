# UnitTest.ImageLyre.Presentation.Desktop

## 意义

本工程是第 8 套测试，守住 `ImageLyre.Presentation.Desktop` 的桌面 Presentation 契约：`PresentationPlan` 的组序与步骤顺序、欢迎窗到主窗体的装配顺序，以及后续桌面 `StopPlan` 的释放语义。它把“桌面启动到底按什么顺序发生”变成可断言的数据，而不是只能靠启动日志与人工点窗口确认。当前只有 `Startup/PresentationPlanTests.cs`，断言 `welcome`/`composition`/`show` 三组的固定组序、组容量与扁平后的 Step Id 序列；装配顺序的失败回滚与关停语义随 T09/T10 补入。

## 边界（不放什么）

- 不放真实窗口：不启动 Avalonia、不创建 `Window`、不读真实插件产物，桌面行为由 `scripts/qa/smoke-m1.ps1` 覆盖。
- 不放 Bootstrap 内部测试：Host 构建、插件启动与 `ExitArbiter` 在 `tests/UnitTest.ImageLyre.Bootstrap`，本工程只测 Presentation 侧。
- 不放 App 入口测试：参数解析与桌面/CLI 分派属于 `tests/UnitTest.ImageLyre.App`。

## 依赖方向

引用 `ImageLyre.Presentation.Desktop`、`xunit.v3`、`xunit.runner.visualstudio`、`Microsoft.NET.Test.Sdk` 与 `FluentAssertions`；不引用 `ImageLyre.App`、不引用任何 UI 或插件工程。`PresentationPlan` 与各 Step 是 Presentation 的内部类型，测试经工程里的 `InternalsVisibleTo` 访问，不为了测试把它们改成公开契约。被 `src/ImageLyre.slnx` 引用。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.Presentation.Desktop\bin\Debug\net10.0\UnitTest.ImageLyre.Presentation.Desktop.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而以握手失败退出，这不是测试本身的问题；runner 已由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0002-启动宿主与编排模式.md` §7.5、§10.2 与 `docs/plans/lukan/PLAN-0002-启动宿主与编排模式实施.md` 的 T05–T07、T10 及本文件；桌面行为口径变化同时改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §7 与 `scripts/qa/smoke-m1.ps1` 的断言。
