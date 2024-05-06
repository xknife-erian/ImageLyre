namespace ImageLyre.UI.Contracts.Commands;
/// <summary>
/// 文档生命周期命令接口（新建、打开）。
/// 通常由应用主壳层（Shell）或文档管理器实现，而非单个文档实例。
/// 这些操作会创建新的工作上下文，属于全局行为。
/// </summary>
public interface IDocumentLifecycleCommands
{
    /// <summary>
    /// 创建一个新文档或工作区。
    /// 可能触发确认对话框（如当前文档未保存）。
    /// </summary>
    /// <returns>
    /// 返回 true 表示成功创建新文档；false 表示用户取消或操作失败。
    /// </returns>
    Task<bool> NewAsync();

    /// <summary>
    /// 打开一个现有文档。
    /// 通常会弹出文件选择对话框，并加载所选文件内容。
    /// </summary>
    /// <returns>
    /// 返回 true 表示成功打开文档；false 表示用户取消或加载失败。
    /// </returns>
    Task<bool> OpenAsync();    
    
    /// <summary>
    /// 关闭一个现有文档。
    /// </summary>
    /// <returns>
    /// 返回 true 表示成功关闭文档；false 表示关闭失败。
    /// </returns>
    Task<bool> CloseAsync();
}
