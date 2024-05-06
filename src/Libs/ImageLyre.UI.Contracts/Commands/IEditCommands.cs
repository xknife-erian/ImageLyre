namespace ImageLyre.UI.Contracts.Commands;
/// <summary>
/// 基本编辑命令接口（剪切、复制、粘贴）。
/// 由具有可选内容区域的组件实现（如文本框、列表、画布元素等）。
/// </summary>
public interface IEditCommands
{
    /// <summary>
    /// 获取当前是否可以执行“剪切”操作。
    /// 通常当有选中内容且内容可修改时为 true。
    /// </summary>
    bool CanCut { get; }

    /// <summary>
    /// 获取当前是否可以执行“复制”操作。
    /// 通常当有选中内容时为 true（即使只读）。
    /// </summary>
    bool CanCopy { get; }

    /// <summary>
    /// 获取当前是否可以执行“粘贴”操作。
    /// 通常当剪贴板包含兼容格式内容时为 true。
    /// </summary>
    bool CanPaste { get; }

    /// <summary>
    /// 剪切当前选中内容到剪贴板，并从源中移除。
    /// 调用前应确保 <see cref="CanCut"/> 为 true。
    /// </summary>
    void Cut();

    /// <summary>
    /// 复制当前选中内容到剪贴板，不修改源内容。
    /// 调用前应确保 <see cref="CanCopy"/> 为 true。
    /// </summary>
    void Copy();

    /// <summary>
    /// 从剪贴板粘贴内容到当前焦点位置。
    /// 调用前应确保 <see cref="CanPaste"/> 为 true。
    /// </summary>
    void Paste();
}
