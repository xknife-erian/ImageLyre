# UnitTest.ImageLyre.Module.Shell

`ImageLyre.Module.Shell` 的单元测试工程，先覆盖 T15 的 `ShellStringSource`：中英两套键、`lyre.shell.*` 前缀、未知键与同一键跨语言差异。窗口布局与语言切换的视觉刷新不进单测，由仓库外临时探针验证。

## 意义

守住壳文案源的契约：`zh-CN` 与 `en-US` 的每个键都有精确文案，键统一以 `lyre.shell.` 开头，未知键返回 `false` 且不写入回退文案。这些文案是壳菜单、状态栏、空态与语言切换控件唯一的文本来源，任何一处漏键都会让界面在切换语言后出现键名。

## 边界（不放什么）

- 不放 Avalonia 窗口测试：不构造 `ShellWindow`、不创建控件、不启动桌面生命周期；语言切换的视觉刷新由仓库外探针验证。
- 不放 Bootstrap 与插件加载：那是 `UnitTest.ImageLyre.Bootstrap` 与 App 测试的范围。
- 不放翻译完整性审计：本工程只守住当前两套语言与已登记键，新增语言或新增键时同步扩测试。

## 依赖方向

引用 `ImageLyre.Module.Shell`、`xunit.v3`、`xunit.runner.visualstudio`、`Microsoft.NET.Test.Sdk` 与 `FluentAssertions`；不引用 App、Bootstrap 或其他插件。被 `src/ImageLyre.slnx` 的 `/Tests/` 引用。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.Module.Shell\bin\Debug\net10.0\UnitTest.ImageLyre.Module.Shell.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而握手失败，这不是测试本身的问题；换到普通终端后 runner 由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §5.6 与 `ImageLyre.Module.Shell` 的 README，再改本文件；新增语言、键或切换控件时同步改壳的 `ShellStringKeys`/`ShellStringSource` 与本工程测试。
