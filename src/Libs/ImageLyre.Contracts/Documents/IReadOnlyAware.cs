// Cangming.Shell/Documents/IReadOnlyAware.cs

namespace ImageLyre.Contracts.Documents;

/// <summary>
/// 表示文档支持只读状态控制。
/// 实现此接口的文档在 IsReadOnly = true 时，应禁止所有修改操作。
/// Shell 将据此自动禁用相关 UI（如编辑菜单、保存按钮、可编辑控件）。
/// </summary>
public interface IReadOnlyAware
{
    /// <summary>
    /// 获取文档当前是否为只读状态。
    /// 只读原因可能包括：权限不足、文件被锁定、版本控制检出状态等。
    /// 此属性变化时，应触发 INotifyPropertyChanged.PropertyChanged。
    /// </summary>
    bool IsReadOnly { get; }
}
