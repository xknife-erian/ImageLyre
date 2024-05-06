namespace ImageLyre.Contracts.Documents;

/// <summary>
/// 表示一个对象具有唯一的、稳定的 ContentId。
/// 该 ID 应在对象生命周期内保持不变（除非显式“另存为”等操作），
/// 用于文档管理、标签页复用、状态恢复等场景。
/// </summary>
public interface IHasContentId
{
    /// <summary>
    /// 获取该对象的唯一内容标识符。
    /// 建议使用以下策略之一：
    /// - 文件绝对路径（如 "C:\Projects\doc.txt"）
    /// - URI（如 "file:///C:/doc.txt" 或 "http://example.com/data.json"）
    /// - GUID（适用于临时/未保存文档，如 "temp-550e8400-e29b-41d4-a716-446655440000"）
    /// 
    /// ⚠️ 注意：此属性应尽量避免频繁变更。若必须变更（如“另存为”），
    /// 请确保通知机制（如 INotifyPropertyChanged）正常工作，
    /// 以便 DocumentPaneManager 能同步更新内部映射。
    /// </summary>
    string ContentId { get; }
}
