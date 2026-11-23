# ImageLyre.Shell.Menus

菜单注册表的默认实现，是壳渲染菜单与 Feature 贡献菜单之间唯一的登记处。

## 意义

把“菜单里有什么”从壳窗口里拿出来：壳只按注册表渲染，Feature 在启动时登记自己的顶层菜单与菜单项。它是宿主级基础设施，与 `ImageLyre.Localization` 同类——由 `ImageLyre.App` 创建实例并在 Bootstrap 容器里注册，壳与 Feature 经 `IIoCManager` 解析同一份实例。

## 边界

- 只做内存登记、按 `Order` 与注册先后排序、句柄撤销与 `Changed` 通知。
- 不做渲染、不做本地化解析、不执行菜单动作；标题只存资源键，`Func<Task>` 由壳在点击时调用。
- 不引用 Avalonia、AtomUI、cangming 机制包或任何插件工程。
- 命令 Id、快捷键、可用条件属于 G2 的命令注册表，不在本库范围。

## 依赖方向

- 引用 `ImageLyre.Shell.Contracts`（`IMenuRegistry` 与两个定义 record）。
- 被 `ImageLyre.App` 引用以创建实例；`ImageLyre.Module.Shell` 与各 Feature 只依赖契约，不引用本工程。

## 边界变化

菜单贡献模型（例如加入命令 Id、`when` 条件、分组）变化时，先改 `DESIGN-0001` 的菜单贡献一节，再改本工程的 README 与实现；契约层变化按“只增不改”处理。
