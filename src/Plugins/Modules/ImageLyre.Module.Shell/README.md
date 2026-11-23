# ImageLyre.Module.Shell

## 意义

M1 的壳插件：以 `BasePicoModule<IWorkbenchShell>` 的 `Build()` 产出 `ShellWindow`，并在窗口构造期向 `IAttachmentRegistry` 贡献 `lyre.shell.toolbar`、`lyre.shell.viewport`、`lyre.shell.panel` 三个一级槽位。它把主窗体骨架、菜单栏、工具栏与辅助面板容器、视口空态和状态栏五段占位收在一个 UI 型 Module 里，App 只负责取壳挂窗与关停转发，不写布局代码；这样换壳实现时只换这个插件，启动链与 Feature 都不动。

## 边界（不放什么）

- 不放 App 编排：不创建桌面生命周期、不注册本地化服务、不驱动 Bootstrap、不执行 `RegisterFeatureAttachments`，这些属于 `ImageLyre.App` 与 T12；壳只回答“窗口长什么样、槽位在哪里”。
- 不放 Feature 内容：工具栏与辅助面板里的真实附着物由 Feature 贡献，壳只提供容器、空态和排序渲染；面板空态“尚无测量”与视口空态“无图像”是壳的静态内容。
- 不放 Bootstrap 与插件扫描：不解析启动参数、不初始化日志、不调用 `PicoPluginSystem`，这些在 `ImageLyre.Bootstrap` 与 App 入口。
- 不放语言切换与资源源：T15 才接 `ILocalizationSource`、`lyre.shell.*` 资源键与标题栏语言入口；M1 的界面文案是中文字面量。
- 不放产品能力：没有图像、ROI、测量、文档、设置代码，视口只是槽位容器。

## 依赖方向

引用 `ImageLyre.Shell.Contracts` 取得 `ShellSlots` 与 `IWorkbenchShell`，引用 `ImageLyre.Shell.Attachments` 取得 `DockPaneAttachment`，引用 `ImageLyre.Contracts` 维持契约方向；机制侧直接使用 `Cangming.PicoPlugins`（Module、上下文、槽位契约）、`Cangming.PicoPlugins.Avalonias`（`SlotPanel`）、`Cangming.Logs`（槽位注入日志）与 `Cangming.Toolkit`（面板附着物排序），界面侧使用 `Avalonia` 与 `AtomUI.Desktop.Controls`。不引用 `ImageLyre.App`、`ImageLyre.Bootstrap`、任何其他插件工程或 Feature 工程；本工程只被 App 的通配工程引用发现，不被其他插件编译引用。

## 边界变化时改哪份文档

先改 `docs/design/lukan/DESIGN-0001-M1-插件骨架.md` §3.4（主窗体由壳插件承载）、§3.5（槽位先于附着）、§3.6（上下文）、§4.2–§4.4（槽位表与非槽位区域）、§5.1–§5.3（壳契约与附着物）及本文件；槽位名或 `IWorkbenchShell` 变化先改 `ImageLyre.Shell.Contracts`，`DockPaneAttachment` 变化先改 `ImageLyre.Shell.Attachments`。T15 把文案资源化时改 §5.6 与本节；T12 调整取壳顺序时改 §3.3、§3.5 与本文件的边界说明。
