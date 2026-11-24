# UnitTest.ImageLyre.App

`ImageLyre.App` 的单元测试工程，按 `tdd-and-unit-testing` 技能的口径组织：xUnit v3（`OutputType=Exe`，.NET 10 上由根 `global.json` 指定 Microsoft.Testing.Platform 作为 `dotnet test` 的 runner）、FluentAssertions 断言、测试方法按 `Test###` 编号并把用途写进 `DisplayName`。

## 意义

守住 T06 的两条契约：`StartupOptions` 是严格的命令白名单（空参数走桌面、三条命令各映射一个命令、多个命令或未知参数落 `Invalid`），`CliHost` 的三条命令分别输出可判读的文本并以 0 退出、参数错误回显原文并以 2 退出、运行期异常写 stderr 并以 1 退出。CLI 的输出文本是机器可读接口而不是界面文案，因此不经过 `ILocalizationService`，测试直接断言固定文本，语言切换不应改变这些断言；这条判断是本工程与界面文案测试的分界。

## 边界（不放什么）

- 不放桌面 UI 测试：不构造 `AppBuilder`、不创建窗口、不启动 Avalonia 桌面生命周期，桌面冒烟由入口实跑负责。
- 不放插件实现测试：`--plugins` 只验证无插件树下的清单与汇总行；具体插件的行为由各插件工程自己的测试覆盖。
- 不放第二条 `--plugins` 用例：`BootstrapHostBuilder.Build` 的 `LogHub.Initialize` 在进程内只允许一次，本工程只保留一条真正启动宿主、读取插件清单的用例；其余测试直接验证参数解析与 CLI 文本。
- 不放端到端进程测试：不拉起子进程验证真实 stdout/stderr 与退出码，那是入口冒烟与验收 F 的范围。
- 不放本地化断言：CLI 文本不随语言变化，`ILocalizationService` 的行为由 `UnitTest.ImageLyre.Localization` 覆盖。

## 依赖方向

引用 `ImageLyre.App`、`xunit.v3`、`xunit.runner.visualstudio`、`Microsoft.NET.Test.Sdk` 与 `FluentAssertions`；不引用任何插件工程，也不直接引用 Avalonia 或 AtomUI 类型。被 `src/ImageLyre.slnx` 的 `/Tests/` 引用。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.App\bin\Debug\net10.0\UnitTest.ImageLyre.App.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而以握手失败退出（`NamedPipeClientStream` 报 `UnauthorizedAccessException`），这不是测试本身的问题；换到普通终端后 `dotnet test` 同样可用，runner 已由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.3 的 CLI 分支与 §7 的 F 双宿主判据及本文件；命令集合、输出字段或退出码约定变化同时改 `src/App/ImageLyre.App/README.md` 与 `CliHost` 的 XML 注释。测试口径变化先改 `.agents/skills/tdd-and-unit-testing` 的真源，不在本仓就地改技能副本。
