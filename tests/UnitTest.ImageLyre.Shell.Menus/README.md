# UnitTest.ImageLyre.Shell.Menus

`ImageLyre.Shell.Menus` 的单元测试：菜单注册、排序、撤销、校验与变更通知。

## 意义

菜单注册表是壳与 Feature 之间唯一的菜单登记处，排序与撤销错了会让菜单项消失或乱序；本工程用 12 条用例守住注册、重复拒绝、Order 排序、句柄幂等撤销与参数校验。

## 边界

- 只测注册表本身，不测壳渲染、本地化解析与菜单动作执行（那些由桌面冒烟覆盖）。
- 不引用 Avalonia、AtomUI 或任何插件工程。

## 依赖方向

引用 `ImageLyre.Shell.Menus` 与被测契约 `ImageLyre.Shell.Contracts`；被 `src/ImageLyre.slnx` 的 Tests 分组引用。

## 边界变化

注册表加入新能力（命令 Id、分组、启用条件）时，在此补对应用例并更新 README。
