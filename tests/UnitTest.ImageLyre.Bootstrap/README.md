# UnitTest.ImageLyre.Bootstrap

## 意义

守住 Bootstrap 骨架的最小启动契约：无界面模式能完整走完日志初始化、扫描、容器构建与管理器启动，没有插件时插件清单为空，日志记录 `HasUiRoute=False` 且能力插件计数按固定模板输出 `能力插件 0 个`。这条测试是 T04 的验收证据，它不证明 Feature 过滤、槽位附着或窗口显示，那些分别由后续的 T16/F 与壳插件测试覆盖，因为 B1 阶段的树内还没有 Feature 工程。

## 边界（不放什么）

- 不放集成测试：不启动 Avalonia、不创建窗口、不读取真实插件包，也不依赖任何插件工程。
- 不放端到端宿主测试：不覆盖 `ImageLyre.App` 的启动参数、宿主分派与退出码，那属于 T05 与 T06。
- 不放性能、并发与重入：启动顺序的正确性由代码审查与方案核对保证，重入保护留到 T13 装配链收口。

## 依赖方向

引用 `ImageLyre.Bootstrap`、`xunit.v3` 与 `FluentAssertions`；日志工厂与日志器由本工程手写实现，不新增日志适配包，也不引用任何 UI 或插件工程。被 `src/ImageLyre.slnx` 引用。

## 怎么跑

构建走 `dotnet build src/ImageLyre.slnx`；跑测试直接执行产出的测试程序集：

```powershell
dotnet tests\UnitTest.ImageLyre.Bootstrap\bin\Debug\net10.0\UnitTest.ImageLyre.Bootstrap.dll
```

`dotnet test` 在本机沙箱里会因为 MTP 的命名管道被拒而以握手失败退出（`NamedPipeClientStream` 报 `UnauthorizedAccessException`），这不是测试本身的问题；换到普通终端后 `dotnet test` 同样可用，runner 已由根 `global.json` 指定为 Microsoft.Testing.Platform。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.3 与 §7 及本文件；T04 的统计口径或启动步骤变化同时改 `src/Bootstrap/ImageLyre.Bootstrap/README.md`。测试口径变化先改 `.agents/skills/tdd-and-unit-testing` 的真源，不在本仓就地改技能副本。
