# UnitTest.ImageLyre.Capabilities

`ImageLyre.Module.Platform` 与 `ImageLyre.Module.Performance` 的单元测试工程，按 `tdd-and-unit-testing` 技能口径组织：xUnit v3、FluentAssertions、`Test###` 编号与 `DisplayName`。

## 意义

守住两个能力实现的契约：平台能力矩阵只在 Windows 声明八项支持，macOS/Linux/Unknown 一律 false，`CapabilityChanged` 不产生元素；性能快照的时间必须是 UTC，工作集、托管堆、线程数与运行时长非负或为正，连续两次采样的 CPU 落在 0–100，且 `PerformanceMonitor` 构造函数不读取平台依赖。

## 边界（不放什么）

- 不放插件启动测试：不构造 Bootstrap、不加载插件、不创建窗口；启动顺序与槽位内容由仓库外探针验证。
- 不放真实平台探测：平台矩阵由构造参数注入，不依赖运行测试的机器。
- 不放 CPU 的精确数值：CPU 受调度影响，只断言首次为 0、连续调用落在 0–100；精确值交给契约与算法评审。
- 不放诊断 Feature 的 UI：面板文本与语言切换由仓库外探针验证。

## 依赖方向

引用 `ImageLyre.Module.Platform`、`ImageLyre.Module.Performance`、`xunit.v3`、`xunit.runner.visualstudio`、`Microsoft.NET.Test.Sdk`、`FluentAssertions` 与 `System.Reactive`（订阅永不触发的流）。不引用 App、Bootstrap、Avalonia 或 AtomUI。

## 怎么跑

```powershell
dotnet tests\UnitTest.ImageLyre.Capabilities\bin\Debug\net10.0\UnitTest.ImageLyre.Capabilities.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 命名管道被拒而握手失败，这不是测试本身的问题；runner 由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §5.4 与两个 Module 的 README，再改本文件；契约字段或平台矩阵变化同步改 `ImageLyre.Contracts`、实现文件与本工程测试。
