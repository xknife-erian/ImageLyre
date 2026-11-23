# UnitTest.ImageLyre.Feature.About

`ImageLyre.Feature.About` 的单元测试：帮助菜单注册、停止撤销与中英双语文案完整性。

## 意义

关于功能的价值在“菜单里能点开一个说得清产品信息的窗口”，菜单没注册上就等于功能不存在；本工程用假 IoC 驱动 Feature 的启动与停止，守住注册、撤销与文案键完整性。

## 边界

- 只测 Feature 的注册与本地化行为，不测窗口控件构造、菜单渲染与点击（由桌面冒烟覆盖）。
- 不启动真实插件宿主，也不依赖 Avalonia 窗口系统。

## 依赖方向

引用 `ImageLyre.Feature.About`、`ImageLyre.Shell.Menus` 与 `ImageLyre.Localization`；被 `src/ImageLyre.slnx` 的 Tests 分组引用。

## 边界变化

新增关于信息或菜单项时，在此补对应用例并更新 README。
