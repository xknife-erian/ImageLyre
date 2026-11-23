# ImageLyre.Contracts

## 意义

本工程承载**与界面无关**的插件契约：服务接口、命令与结果的 DTO、状态流。它是插件之间唯一的公共面，也是 CLI 宿主与后台批处理不必触碰 UI 程序集的原因——凡是能用 `ILocalizationService` 这样不含界面类型的方式表达的能力，都归这里。M1 阶段自有的只有两个本地化接口，另引用 `Cangming.Platform.Contracts` 借用 `IPlatformCapabilities` 与 `IPerformanceMonitor` 两个架构级契约；域模型留在 `ImageLyre.Domains`，壳与面板相关的契约留给 `ImageLyre.Shell.Contracts`。

## 边界（不放什么）

- 不放 UI 类型：不引用 Avalonia、AtomUI、SkiaSharp，也不出现 `Control`、`Window`、主题令牌；
- 不放实现：本工程只声明契约，`LocalizationService` 这类实现落在 `src/Infrastructure/`；
- 不放机制包类型：不引用 `Cangming.PicoPlugins` 与 `Cangming.Toolkit`，附着物词汇在 `ImageLyre.Shell.Attachments`；
- 不放会变的数据模型：领域对象在 `ImageLyre.Domains`，本工程的契约只增不改；
- 不自造一次性契约：验收用的能力直接从 cangming 契约库借用，早先的 `ImageLyre.Contracts.Demo`（`IClock`、`IGreeter`）已取消——那种接口在 M1 结束后没人维护，还要在验收里专门删除，等于用一个必然废弃的接口去验证一个长期机制；
- 不借用会在本仓落地的实现：只引 `Cangming.Platform.Contracts` 这类纯契约包，`Cangming.Shell`、`Cangming.Platform` 的实现包一律不进。

## 依赖方向

单向依赖 `ImageLyre.Domains`，另依赖 cangming 的纯契约包 `Cangming.Platform.Contracts`；被 `ImageLyre.Shell.Contracts`、`ImageLyre.Localization`、`ImageLyre.Bootstrap`、壳插件与各能力/表达插件引用。反向引用一律禁止：插件不得让契约反过来依赖自己，契约也不得为任何单个插件的方便而定制。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§5.6 与本文件，再同批改实现方与调用方；契约的破坏性变化须按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §1.6 另开 ADR 并在新旧文件中互相登记，不允许在实现里顺手改签名。
