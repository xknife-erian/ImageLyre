# ImageLyre.Bootstrap

## 意义

本工程承载“插件就绪之前”的启动链：初始化日志门面、按宿主模式决定扫描期是否保留 Feature、登记两条插件程序集前缀、构建服务容器并把同一个日志工厂注册进去、调用 cangming 的 `PicoPluginSystem.Launch` 完成扫描与注册、解析 `IPluginManager`、通知容器已构建、启动全部插件并输出能力插件计数。它把这条链固定在一处，使桌面与无界面两种宿主共享同一份启动语义，而 `ImageLyre.App` 只处理用户可见的启动画面、欢迎界面、主窗体挂载与关停转发；若没有这一层，两种宿主就会各自复制一遍扫描与启动顺序，任何一侧先改一步都会让插件清单在两边分叉。

## 边界（不放什么）

- 不放界面：不引用 Avalonia、AtomUI、SkiaSharp，不创建窗口、控件、槽位管理器或主窗体，这些属于 `ImageLyre.App` 与壳插件。
- 不放宿主分派：不解析启动参数、不判断桌面还是无界面、不初始化 Avalonia，`HostMode` 只是调用方传进来的输入。
- 不放业务服务：不创建 `LocalizationService`、不注册具体能力实现，宿主把需要共享的业务实例经 `StartAsync` 的 `configure` 回调注册进容器；日志工厂是唯一例外，它是本方法的入参且插件管理器构造必需，由本工程注册。
- 不放插件：不按名字引用任何 `ImageLyre.Module.*` 或 `ImageLyre.Feature.*` 工程，扫描只按程序集文件名前缀发生。
- 不放产品能力：M1 的 Bootstrap 里没有图像、ROI、测量、文档或设置代码，后续能力只能以插件形式接入。

## 依赖方向

引用 `Cangming.PicoPlugins`（`PicoPluginSystem`、`PicoPluginSettings`，`LogHub` 来自其传递依赖 `Cangming.Logs`）、`ImageLyre.Contracts`（供宿主在 `configure` 中注册契约实现）、`Microsoft.Extensions.DependencyInjection` 与 `Microsoft.Extensions.Logging.Abstractions`（直接使用 `IServiceCollection`、`ServiceProvider` 与 `ILoggerFactory`）。被 `ImageLyre.App` 引用；不被任何插件工程引用，也不引用任何插件工程或 UI 程序集。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.1、§3.3、§3.7 与 §3.8 及本文件：`StartAsync` 的签名或步骤顺序变化必须同步回写 §3.3 的代码块，扫描前缀增删改 §3.7，失败与关停语义改 §3.8；若改动使 Bootstrap 需要依赖 UI 或插件工程，先按 `docs/adr/lukan/ADR-0001-插件机制架构基线.md` §2.4 与 §2.5 修正架构基线，再动代码。宿主职责的挪动同时改 `ImageLyre.App` 的 README。
