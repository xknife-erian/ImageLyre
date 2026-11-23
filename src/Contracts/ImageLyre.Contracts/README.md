# ImageLyre.Contracts

## 意义

本工程承载**与界面无关**的插件契约：服务接口、命令与结果的 DTO、状态流。它是插件之间唯一的公共面，也是 CLI 宿主与后台批处理不必触碰 UI 程序集的原因——凡是能用 `ILocalizationService` 这样不含界面类型的方式表达的能力，都归这里。M1 阶段只有两个本地化接口，域模型留在 `ImageLyre.Domains`，壳与面板相关的契约留给 `ImageLyre.Shell.Contracts`。

## 边界（不放什么）

- 不放 UI 类型：不引用 Avalonia、AtomUI、SkiaSharp，也不出现 `Control`、`Window`、主题令牌；
- 不放实现：本工程只声明契约，`LocalizationService` 这类实现落在 `src/Infrastructure/`；
- 不放机制包类型：不引用 `Cangming.PicoPlugins` 与 `Cangming.Toolkit`，附着物词汇在 `ImageLyre.Shell.Attachments`；
- 不放会变的数据模型：领域对象在 `ImageLyre.Domains`，本工程的契约只增不改；
- 不留演示契约：`ImageLyre.Contracts.Demo` 命名空间（`IClock`、`IGreeter`）是 M1 验收用的临时内容，验收后按命名空间整体删除。

## 依赖方向

单向依赖 `ImageLyre.Domains`；被 `ImageLyre.Shell.Contracts`、`ImageLyre.Localization`、`ImageLyre.Bootstrap`、壳插件与各能力/表达插件引用。反向引用一律禁止：插件不得让契约反过来依赖自己，契约也不得为任何单个插件的方便而定制。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§5.6 与本文件，再同批改实现方与调用方；契约的破坏性变化须按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §1.6 另开 ADR 并在新旧文件中互相登记，不允许在实现里顺手改签名。
