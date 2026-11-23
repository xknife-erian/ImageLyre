# UnitTest.ImageLyre.Localization

`ImageLyre.Localization` 的单元测试工程，按 `tdd-and-unit-testing` 技能的口径组织：xUnit v3（`OutputType=Exe`，在 .NET 10 上由根 `global.json` 指定 Microsoft.Testing.Platform 作为 `dotnet test` 的 runner）、FluentAssertions 断言、测试方法按 `Test###` 编号并把用途写进 `DisplayName`。

## 意义

守住 `LocalizationService` 的四条契约：缺键回退到 `zh-CN`、资源键按来源隔离、语言切换只广播一次、重复注册同一资源源幂等。这些是界面文案的唯一入口，任何一条走偏都会让全产品出现两套文案来源。

## 边界（不放什么）

- 不放集成测试：不启动 Avalonia、不创建窗口、不读真实资源文件；
- 不放壳与插件的测试：它们各自对自己的 `ILocalizationSource` 负责；
- 不测界面排版与视觉呈现，那属于宿主实跑冒烟的范围。

## 依赖方向

引用 `ImageLyre.Localization` 与 `xunit.v3`、`FluentAssertions`；被解决方案引用，不引用任何插件或机制包。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.Localization\bin\Debug\net10.0\UnitTest.ImageLyre.Localization.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而以握手失败退出（`NamedPipeClientStream` 报 `UnauthorizedAccessException`），这不是测试本身的问题；换到普通终端后 `dotnet test` 同样可用，runner 已由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1 与本文件；测试口径变化先改 `.agents/skills/tdd-and-unit-testing` 的真源，不在本仓就地改技能副本。
