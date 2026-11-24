# UnitTest.ImageLyre.Bootstrap

`ImageLyre.Bootstrap` 的单元测试工程，覆盖 Generic Host 构建、Step/Plan/Report、插件复合 Step、Plan 执行器与关停裁决。

## 意义

守住启动机制的四个契约：`BootstrapHostBuilder` 能构建并启动一个 `IHost` 且插件管理器同源；`BootstrapPlan` 按组序与组内顺序扁平化并拒绝重复 Id；`BootstrapPlanRunner` 在 `Degraded` 时继续、`Failed` 时逆序回滚并把 StepId/异常类型/诊断写入 `StartupReport`；`ExitArbiter` 在 64 并发下只执行一次关停、停止先于开始仍可等待、超时与原始异常不被吞掉。测试替身只实现 `IBootstrapStep` 与日志接口，不启动真实插件或窗口。

## 边界（不放什么）

- 不启动 Avalonia、不创建窗口、不读取真实插件包；桌面行为由 `scripts/qa/smoke-m1.ps1` 覆盖。
- 不测试 App 的参数解析与 CLI 文本，那属于 `UnitTest.ImageLyre.App`。
- 不测 PresentationPlan 的桌面步骤顺序，那属于第 8 套测试；本工程只测共享执行器与 HostPlan。
- 不为通过测试放宽 `BootstrapPlan` 的容量/重复 Id 规则；50 步增长用例按真实规则构造多组。

## 依赖方向

引用 `ImageLyre.Bootstrap`、`xunit.v3`、`Microsoft.NET.Test.Sdk` 与 `FluentAssertions`；日志工厂与日志器由本工程手写实现，不引用 UI 或插件工程。被 `src/ImageLyre.slnx` 引用。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.Bootstrap\bin\Debug\net10.0\UnitTest.ImageLyre.Bootstrap.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而以握手失败退出，这不是测试本身的问题；换到普通终端后 `dotnet test` 同样可用，runner 已由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0002-启动宿主与编排模式.md` §7.5、§10.2、§10.4 与 `docs/plans/lukan/PLAN-0002-启动宿主与编排模式实施.md` 的 T02–T04、T09–T10 及本文件；HostPlan 或 `ExitArbiter` 的契约变化同时改 `src/App/ImageLyre.Bootstrap/README.md` 与 `ADR-0003` §2。
