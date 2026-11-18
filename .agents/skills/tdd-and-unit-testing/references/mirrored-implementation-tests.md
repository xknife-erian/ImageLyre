# 孪生实现的测试口径

## When to Use

同一份逻辑在一个仓里存在两份实现时加载本文件（只差 logger、命名空间或框架适配层，例如两份插件基础库，一份用 NLog、一份用 Microsoft.Extensions.Logging）。下面每条都只回答一件事：怎么让「只改了一侧」被测试拦住。

- **一个测试文件同时引用两侧。** 两侧同名类型（`Util`、`PluginsCore`、`PluginInfo`、`PicoPluginLoader`……）必然撞名，用别名分开并加 `global::` 前缀：`using V1Util = global::Rays.Plugins.Default.Internal.Util;`。测试工程的命名空间常与被测命名空间共享前缀（`UnitTest.Rays.*` 对 `Rays.*`），`global::` 让解析不受外层命名空间影响。
- **每条用例两侧各断言一遍。** 同一 Arrange，两组 Act/Assert，断言内容一致。拿「另一侧同理」当注释不算覆盖。
- **两侧的静态状态是两套独立静态。** 复位方法两侧一起清（只清一侧会让另一侧的用例带着上一条测试留下的注册跑）；把用到这些静态的测试类放进共享的不并行 collection（`[CollectionDefinition(name, DisableParallelization = true)]` 加类上 `[Collection(name)]`），因为跨测试类默认并行，会互相看见对方注册的 handler/回调。
- **接口已在两侧分叉时，夹具类按侧各写一个，不要合成一个类实现两套接口。** 每侧一个夹具反而让「A 加载器只找到 A 插件」成为可断言的结论，既比一个类同时实现两套接口省事，也多守住一层「两侧没有串在一起」的性质。
- **断言用稳定标识而非类型实例。** 程序集可能被加载到不同上下文，同一个类型会存在两个 `Type` 实例，比对 `FullName` 而不是 `typeof(...)` 的实例相等。
