namespace ImageLyre.UI.Contracts.Commands;
/// <summary>
/// 保存相关命令接口（保存、另存为）。
/// 由可持久化的文档或工作区实现（如文本编辑器、项目文件等）。
/// 支持脏状态（IsDirty）判断是否需要保存。
/// </summary>
public interface ISaveCommands
{
    /// <summary>
    /// 获取当前是否可以执行“保存”操作。
    /// 通常当文档内容已修改（脏状态）且已有有效保存路径时为 true。
    /// </summary>
    bool CanSave { get; }

    /// <summary>
    /// 获取当前是否可以执行“另存为”操作。
    /// 通常只要文档有内容即可为 true（即使未修改）。
    /// </summary>
    bool CanSaveAs { get; }

    /// <summary>
    /// 将当前内容保存到已有路径。
    /// 若尚未指定路径（如新建未保存文档），应自动转为“另存为”逻辑。
    /// </summary>
    /// <returns>
    /// 返回 true 表示成功保存；false 表示用户取消或保存失败。
    /// </returns>
    Task<bool> SaveAsync();

    /// <summary>
    /// 弹出“另存为”对话框，将内容保存到用户指定的新路径。
    /// 成功后应更新内部路径，并清除脏状态。
    /// </summary>
    /// <returns>
    /// 返回 true 表示成功另存；false 表示用户取消或保存失败。
    /// </returns>
    Task<bool> SaveAsAsync();
}
