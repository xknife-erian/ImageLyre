# ImageLyre.Module.Performance

## 意义

M1 的性能监控插件：以 `BasePicoModule<IPerformanceMonitor>` 向宿主提供工作集、托管堆、线程数、CPU 与运行时长的按需快照。它声明 `Require<IPlatformCapabilities>` 与 `After<IPlatformCapabilities>`，因为采样口径最终以平台能力为准；快照中的单位与数值口径由契约固定。

## 边界（不放什么）

- 不放后台采样线程：`GetPerformanceSnapshot` 按需计算，`SnapshotUpdated` 返回永不触发的流。
- 不把平台能力缓存进构造函数：服务只通过 `Func<IPlatformCapabilities>` 在采样时取用，构造函数零依赖求值。
- 不放 UI：不引用 Avalonia、AtomUI 或 SkiaSharp。
- 不覆盖 public 生命周期方法：只覆盖 `OnStartAsync` 与 `DeclareDependencies`。

## 依赖方向

引用 `ImageLyre.Contracts`（`IPerformanceMonitor`、`PerformanceSnapshot`、`IPlatformCapabilities`）、`Cangming.PicoPlugins`（Module 基类与依赖声明）与 `Cangming.Logs`（启动日志），另直接引用 `System.Reactive` 7.0.0。不被其他工程编译引用；由插件扫描发现，被 `ImageLyre.Feature.Diagnostics` 按契约消费。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§3.6、§5.4、§5.5 与 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T14，再改本文件与 `ImageLyre.Contracts/Diagnostics/IPerformanceMonitor.cs` 的 XML 注释；采样口径或 CPU 算法变化同步改 `Internal/PerformanceMonitor.cs` 的注释与 `UnitTest.ImageLyre.Capabilities`。
