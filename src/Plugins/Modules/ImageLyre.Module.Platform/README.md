# ImageLyre.Module.Platform

## 意义

M1 的平台能力插件：以 `BasePicoModule<IPlatformCapabilities>` 向宿主提供当前操作系统类别与八项平台能力矩阵，是 `ImageLyre.Module.Performance` 与诊断 Feature 的依赖提供方。M1 只把 Windows 的实测结果写死为支持，其余平台按未知处理，避免把“未实测”伪装成“可用”。

## 边界（不放什么）

- 不放平台适配实现：只回答能力是否存在，不实现托盘、凭据、通知、文件关联、URL scheme、单实例、自更新或崩溃转储。
- 不放 UI：不引用 Avalonia、AtomUI 或 SkiaSharp，也不创建控件。
- 不放运行期探测：M1 读 `RuntimeInformation` 后固定矩阵，不监听系统变化；`CapabilityChanged` 返回永不触发的流。
- 不覆盖 public 生命周期方法：只覆盖 `OnStartAsync`，构造函数只装配上下文。

## 依赖方向

引用 `ImageLyre.Contracts`（`IPlatformCapabilities`、`OperatingSystemKind`、`PlatformCapability`）、`Cangming.PicoPlugins`（Module 基类与 `[Plugin]`）与 `Cangming.Logs`（启动日志），另直接引用 `System.Reactive` 7.0.0 提供 `Observable.Never`。不被其他工程编译引用；由插件扫描发现，被 `ImageLyre.Module.Performance` 与 `ImageLyre.Feature.Diagnostics` 按契约消费。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§3.6、§5.4、§5.5 与 `docs/plans/lukan/PLAN-0001-M1-插件骨架实施.md` 的 T14，再改本文件与 `ImageLyre.Contracts/Platform/IPlatformCapabilities.cs` 的 XML 注释；macOS/Linux 实测矩阵变化同步改 `Internal/PlatformCapabilities.cs` 的 ponytail 与本文件。
