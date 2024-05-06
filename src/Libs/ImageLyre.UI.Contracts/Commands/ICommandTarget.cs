namespace ImageLyre.UI.Contracts.Commands;
/// <summary>
/// 标记接口，表示该对象可作为命令目标（Command Target）。
/// 主要用于类型检查和上下文识别，本身不包含任何成员。
/// 实现此接口的对象可被命令系统（如菜单、工具栏、快捷键）识别为有效操作目标。
/// </summary>
public interface ICommandTarget
{
    // 空标记接口，仅用于标识
}
